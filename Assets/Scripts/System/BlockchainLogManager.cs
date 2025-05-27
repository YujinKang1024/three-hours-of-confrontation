using System.Collections.Generic;
using Libplanet.Action;
using Libplanet.Blocks;
using Libplanet.Blockchain.Renderers;
using Libplanet.Unity;
using UnityEngine;
using UnityEngine.Events;
using Scripts.Actions;

namespace Scripts
{
    public class BlockchainLogManager : MonoBehaviour
    {
        [Header("게임 종료 시 저장할 정보")]
        public ConversationLogger conversationLogger;
        public GameStateManager gameStateManager;
        public TimeManager timeManager;

        [Header("UI 컴포넌트")]
        public GameObject logViewerPanel;
        public TMPro.TMP_Text statsText;
        public TMPro.TMP_Text allLogsText;
        public UnityEngine.UI.ScrollRect logsScrollRect;

        private IEnumerable<IRenderer<PolymorphicAction<ActionBase>>> _renderers;
        private Agent _agent;
        private bool _isInitialized = false;
        private string _currentPlayerName = "";
        private System.DateTime _gameStartTime;
        private float _lastSaveTime = 0f; // 중복 저장 방지용
        private bool _isSaving = false; // 저장 중 상태

        public List<LogPanel.LogEntry> GetParsedLogs()
        {
            var result = new List<LogPanel.LogEntry>();

            if (_agent == null)
            {
                Debug.LogWarning("[BlockchainLogManager] Agent가 null입니다.");
                return result;
            }

            try
            {
                var currentState = _agent.GetState(_agent.Address);
                if (currentState is Bencodex.Types.Text logText && !string.IsNullOrEmpty(logText.Value))
                {
                    Debug.Log($"[BlockchainLogManager] 블록체인에서 로그 데이터 발견: {logText.Value.Length}자");

                    // 여러 개 로그 분리 (‖로 구분)
                    var entries = logText.Value.Split(new string[] { "‖" }, System.StringSplitOptions.RemoveEmptyEntries);

                    Debug.Log($"[BlockchainLogManager] 분리된 엔트리 수: {entries.Length}");

                    foreach (var entry in entries)
                    {
                        if (string.IsNullOrWhiteSpace(entry)) continue;

                        var parts = entry.Split('|');
                        if (parts.Length >= 4)
                        {
                            var logEntry = new LogPanel.LogEntry
                            {
                                timestamp = parts[0].Trim(),
                                playerName = parts[1].Trim(),
                                result = parts[2].Trim(),
                                fullConversation = parts[3].Trim(),
                                isMyLog = true
                            };

                            result.Add(logEntry);
                            Debug.Log($"[BlockchainLogManager] 로그 엔트리 파싱 완료: {logEntry.timestamp} - {logEntry.result}");
                        }
                        else
                        {
                            Debug.LogWarning($"[BlockchainLogManager] 잘못된 로그 형식: {entry} (파트 수: {parts.Length})");
                        }
                    }
                }
                else
                {
                    Debug.Log("[BlockchainLogManager] 블록체인에 저장된 로그가 없습니다.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BlockchainLogManager] 로그 파싱 중 오류: {ex.Message}");
            }

            Debug.Log($"[BlockchainLogManager] 최종 파싱된 로그 수: {result.Count}");
            return result;
        }

        void Awake()
        {
            InitializeBlockchain();
            _gameStartTime = System.DateTime.Now;
        }

        void InitializeBlockchain()
        {
            try
            {
                _renderers = new List<IRenderer<PolymorphicAction<ActionBase>>>()
                {
                    new AnonymousRenderer<PolymorphicAction<ActionBase>>()
                    {
                        BlockRenderer = (oldTip, newTip) =>
                        {
                            if (newTip.Index > 0 && _agent != null && _isInitialized)
                            {
                                try
                                {
                                    _agent.RunOnMainThread(() => UpdateLogsFromBlockchain());
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogWarning($"[BlockchainLogManager] BlockRenderer 오류: {ex.Message}");
                                }
                            }
                        }
                    }
                };

                // Agent 생성 전 약간 대기
                StartCoroutine(DelayedAgentInitialization());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BlockchainLogManager] 블록체인 초기화 실패: {ex.Message}");
                _isInitialized = false;
            }
        }

        System.Collections.IEnumerator DelayedAgentInitialization()
        {
            // 약간 대기 후 Agent 초기화
            yield return new WaitForSeconds(0.5f);

            try
            {
                _agent = Agent.AddComponentTo(gameObject, _renderers);

                if (_agent != null)
                {
                    _isInitialized = true;
                    Debug.Log("[BlockchainLogManager] 블록체인 로그 시스템 초기화 완료");
                }
                else
                {
                    Debug.LogError("[BlockchainLogManager] Agent 생성 실패");
                    _isInitialized = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BlockchainLogManager] Agent 생성 중 오류: {ex.Message}");
                _isInitialized = false;
            }
        }

        void Start()
        {
            // Agent가 초기화될 때까지 대기
            StartCoroutine(WaitForAgentInitialization());
        }

        System.Collections.IEnumerator WaitForAgentInitialization()
        {
            // Agent가 초기화될 때까지 대기 (최대 10초)
            float waitTime = 0f;
            while (_agent == null && waitTime < 10f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }

            if (_agent != null)
            {
                _currentPlayerName = $"Player_{_agent.Address.ToHex().Substring(0, 8)}";
                Debug.Log($"[BlockchainLogManager] 플레이어 설정 완료: {_currentPlayerName}");
            }
            else
            {
                Debug.LogError("[BlockchainLogManager] Agent 초기화 타임아웃");
                _currentPlayerName = "Player_Unknown";
            }
        }

        // 게임 로그를 블록체인에 저장
        public void SaveGameLogToBlockchain()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[BlockchainLogManager] 블록체인이 초기화되지 않았습니다.");
                return;
            }

            // 중복 저장 방지: 짧은 시간 내 중복 호출 차단
            if (Time.time - _lastSaveTime < 2f)
            {
                Debug.LogWarning("[BlockchainLogManager] 너무 빠른 연속 저장 요청 - 무시됨");
                return;
            }

            if (_isSaving)
            {
                Debug.LogWarning("[BlockchainLogManager] 이미 저장 중입니다.");
                return;
            }

            _lastSaveTime = Time.time;
            _isSaving = true;

            string conversationLog = conversationLogger.GetLogText();
            string gameResult = gameStateManager.hasConfessed ? "승리" : "패배";
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Debug.Log($"[BlockchainLogManager] 게임 로그 저장 시작: {_currentPlayerName} ({gameResult})");
            Debug.Log($"[BlockchainLogManager] 대화 로그 길이: {conversationLog.Length}자");

            try
            {
                List<PolymorphicAction<ActionBase>> actions = new List<PolymorphicAction<ActionBase>>()
                {
                    new GameLogAction(_currentPlayerName, conversationLog, gameResult, timestamp)
                };

                _agent.MakeTransaction(actions);

                Debug.Log($"[BlockchainLogManager] 게임 로그 저장 트랜잭션 생성 완료: {_currentPlayerName} ({gameResult})");

                // 저장 완료 후 상태 리셋 (약간의 지연 후)
                StartCoroutine(ResetSavingState());

                ShowSaveConfirmation();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BlockchainLogManager] 게임 로그 저장 실패: {ex.Message}");
                _isSaving = false;
            }
        }

        // 저장 상태 리셋
        private System.Collections.IEnumerator ResetSavingState()
        {
            yield return new WaitForSeconds(1f);
            _isSaving = false;
            Debug.Log("[BlockchainLogManager] 저장 상태 리셋 완료");
        }

        // 저장 중 상태 확인
        public bool IsSaving()
        {
            return _isSaving;
        }

        // 로그 뷰어 열기
        public void OpenLogViewer()
        {
            if (logViewerPanel != null)
            {
                logViewerPanel.SetActive(true);
                UpdateLogsFromBlockchain();
            }
        }

        // 로그 뷰어 닫기
        public void CloseLogViewer()
        {
            if (logViewerPanel != null)
            {
                logViewerPanel.SetActive(false);
            }
        }

        private void UpdateLogsFromBlockchain()
        {
            try
            {
                if (!_isInitialized) return;

                var allLogs = GetParsedLogs();

                // ⚠️ statsText 업데이트 제거 - LogPanel에서만 처리
                // UI 업데이트는 LogPanel에서 담당
                Debug.Log($"[BlockchainLogManager] 로그 업데이트 완료: {allLogs.Count}개 (UI는 LogPanel에서 처리)");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BlockchainLogManager] 로그 업데이트 실패: {ex.Message}");
            }
        }

        private void ShowSaveConfirmation()
        {
            Debug.Log("[BlockchainLogManager] 게임 로그가 블록체인에 성공적으로 저장되었습니다!");
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _agent != null)
            {
                try
                {
                    Debug.Log("[BlockchainLogManager] 애플리케이션 일시정지");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[BlockchainLogManager] 일시정지 처리 중 오류: {ex.Message}");
                }
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _agent != null)
            {
                try
                {
                    Debug.Log("[BlockchainLogManager] 애플리케이션 포커스 해제");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[BlockchainLogManager] 포커스 해제 처리 중 오류: {ex.Message}");
                }
            }
        }

        void OnDestroy()
        {
            try
            {
                _isInitialized = false;
                StopAllCoroutines();

                if (_agent != null)
                {
                    Debug.Log("[BlockchainLogManager] BlockchainLogManager 파괴 - Agent 정리");
                    _agent = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[BlockchainLogManager] OnDestroy 처리 중 오류: {ex.Message}");
            }
        }
    }
}
