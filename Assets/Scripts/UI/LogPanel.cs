using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Scripts;
using System.Collections;

public class LogPanel : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public GameObject logViewerPanel;
    public Transform logListParent;
    public GameObject logEntryPrefab;
    public TMP_Text detailText;
    public TMP_Text statsText;
    public Button closeButton;

    [Header("블록체인 연동")]
    public BlockchainLogManager blockchainManager;

    private List<LogEntry> allLogs = new List<LogEntry>();
    private Button selectedButton = null;
    private bool isInitialized = false;
    private bool isUpdatingStats = false; // 통계 업데이트 중복 방지

    [System.Serializable]
    public class LogEntry
    {
        public string timestamp;
        public string playerName;
        public string result;
        public string fullConversation;
        public bool isMyLog;

        public string GetSummary()
        {
            string marker = isMyLog ? " (내 기록)" : "";
            return $"[{timestamp}] {playerName} - {result}{marker}";
        }

        public string GetUniqueKey()
        {
            return $"{timestamp}_{playerName}_{result}";
        }
    }

    void Start()
    {
        Debug.Log("[LogPanel] 시작됨 - 블록체인 중심 모드");

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseViewer);
        }

        if (blockchainManager == null)
        {
            Debug.LogError("[LogPanel] BlockchainLogManager가 할당되지 않았습니다!");
        }

        isInitialized = true;
    }

    public void OpenViewer()
    {
        Debug.Log("[LogPanel] OpenViewer 호출됨");

        if (logViewerPanel != null)
        {
            logViewerPanel.SetActive(true);

            StartCoroutine(LoadAndRefreshWithDelay());
        }
    }

    private IEnumerator LoadAndRefreshWithDelay()
    {
        Debug.Log("[LogPanel] 최신 데이터 로드 시작");

        // 1. 약간의 지연 (블록체인 동기화 대기)
        yield return new WaitForSeconds(0.3f);

        // 2. 블록체인에서 강제 재로드
        LoadFromBlockchain();

        // 3. UI 새로고침
        RefreshLogList();

        Debug.Log("[LogPanel] 최신 데이터 로드 완료");
    }

    public void CloseViewer()
    {
        if (logViewerPanel != null)
        {
            logViewerPanel.SetActive(false);
        }
    }

    private IEnumerator LoadLogsWithDelay()
    {
        // 블록체인 초기화 대기
        yield return new WaitForSeconds(0.2f);

        LoadFromBlockchain();
        RefreshLogList();
    }

    private void LoadFromBlockchain()
    {
        if (blockchainManager == null)
        {
            allLogs.Clear();
            return;
        }

        try
        {

            var blockchainLogs = blockchainManager.GetParsedLogs();

            var deduplicatedLogs = new List<LogEntry>();
            var seenKeys = new HashSet<string>();

            foreach (var log in blockchainLogs)
            {
                string timeKey = "";
                if (System.DateTime.TryParse(log.timestamp, out var logTime))
                {
                    timeKey = logTime.ToString("yyyy-MM-dd HH:mm");
                }

                string strictKey = $"{timeKey}_{log.result}_{log.playerName}";

                if (!seenKeys.Contains(strictKey))
                {
                    seenKeys.Add(strictKey);
                    deduplicatedLogs.Add(log);
                }
                else
                {
                    Debug.LogWarning($"[LogPanel] 엄격한 중복 제거: {strictKey}");
                }
            }

            allLogs.Clear();
            allLogs.AddRange(deduplicatedLogs);

            // 타임스탬프 순으로 정렬 (최신순)
            allLogs = allLogs.OrderByDescending(log =>
                System.DateTime.TryParse(log.timestamp, out var date) ? date : System.DateTime.MinValue)
                .ToList();

            Debug.Log($"[LogPanel] 중복 제거 후 최종 로그 수: {allLogs.Count}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LogPanel] 블록체인 로그 로드 실패: {ex.Message}");
            allLogs.Clear();
        }
    }

    private void RefreshLogList()
    {
        Debug.Log($"[LogPanel] RefreshLogList 시작: {allLogs.Count}개 로그");

        if (logListParent == null || logEntryPrefab == null)
        {
            Debug.LogError("[LogPanel] 필수 컴포넌트가 null입니다!");
            return;
        }

        // 기존 UI 아이템 모두 삭제
        for (int i = logListParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(logListParent.GetChild(i).gameObject);
        }

        // 선택 상태 초기화
        selectedButton = null;

        // 로그가 없으면 안내 메시지
        if (allLogs.Count == 0)
        {
            UpdateStatisticsText("아직 블록체인에 저장된 로그가 없습니다");
            if (detailText != null)
            {
                detailText.text = "아직 블록체인에 저장된 게임 로그가 없습니다.\n\n게임을 완료한 후 '로그 저장' 버튼을 눌러주세요.";
            }
            return;
        }

        UpdateDetailedStatistics();

        // 새 UI 아이템 생성
        Debug.Log($"[LogPanel] UI 아이템 생성 시작: {allLogs.Count}개");
        for (int i = 0; i < allLogs.Count; i++)
        {
            try
            {
                CreateLogListItem(allLogs[i], i);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LogPanel] UI 아이템 {i} 생성 실패: {ex.Message}");
            }
        }

        Debug.Log($"[LogPanel] UI 아이템 생성 완료. 실제 자식 수: {logListParent.childCount}");

        // 레이아웃 강제 새로고침
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(logListParent as RectTransform);

        // 첫 번째 로그 자동 선택
        if (allLogs.Count > 0 && logListParent.childCount > 0)
        {
            Button firstButton = logListParent.GetChild(0).GetComponent<Button>();
            if (firstButton != null)
            {
                SelectLogEntry(allLogs[0], firstButton);
            }
        }

        Debug.Log($"[LogPanel] RefreshLogList 완료");
    }

    private void UpdateStatisticsText(string text)
    {
        if (statsText != null && !isUpdatingStats)
        {
            isUpdatingStats = true;
            statsText.text = text;
            Debug.Log($"[LogPanel] 통계 텍스트 업데이트: {text}");
            isUpdatingStats = false;
        }
    }

    private void UpdateDetailedStatistics()
    {
        if (allLogs.Count == 0) return;

        try
        {
            // 승률 계산
            int victories = allLogs.Count(log => log.result == "승리");
            int defeats = allLogs.Count(log => log.result == "패배");
            float winRate = allLogs.Count > 0 ? (float)victories / allLogs.Count * 100f : 0f;

            // 최근 게임 날짜
            string recentGameDate = "없음";
            if (allLogs.Count > 0)
            {
                if (System.DateTime.TryParse(allLogs[0].timestamp, out var latestDate))
                {
                    recentGameDate = latestDate.ToString("MM/dd HH:mm");
                }
            }

            // 연승/연패 계산
            int currentStreak = CalculateCurrentStreak();
            string streakText = currentStreak > 0 ?
                $"{currentStreak}연승" :
                (currentStreak < 0 ? $"{-currentStreak}연패" : "기록 없음");

            // 통계 텍스트 구성
            string statsString = $"총 {allLogs.Count}게임 | 승률 {winRate:F1}% ({victories}승 {defeats}패)\n" +
                               $"최근: {recentGameDate} | 현재: {streakText}";

            UpdateStatisticsText(statsString);

            Debug.Log($"[LogPanel] 상세 통계 계산 완료: {allLogs.Count}게임, 승률 {winRate:F1}%");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LogPanel] 통계 계산 중 오류: {ex.Message}");
            UpdateStatisticsText($"총 {allLogs.Count}개 로그 (통계 계산 오류)");
        }
    }

    // 현재 연승/연패 계산
    private int CalculateCurrentStreak()
    {
        if (allLogs.Count == 0) return 0;

        string lastResult = allLogs[0].result;
        int streak = 0;

        foreach (var log in allLogs)
        {
            if (log.result == lastResult)
            {
                streak += (lastResult == "승리") ? 1 : -1;
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private void CreateLogListItem(LogEntry log, int index)
    {
        if (logEntryPrefab == null || logListParent == null)
        {
            Debug.LogError($"[LogPanel] CreateLogListItem: 필수 컴포넌트가 null입니다!");
            return;
        }

        GameObject item = Instantiate(logEntryPrefab, logListParent);
        item.name = $"LogItem_{index}";

        Button button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            LogEntry capturedLog = log;
            button.onClick.AddListener(() => SelectLogEntry(capturedLog, button));
        }

        TMP_Text text = item.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = log.GetSummary();
            if (log.isMyLog) text.color = Color.yellow;
        }

        Image background = item.GetComponent<Image>();
        if (background != null)
        {
            background.color = log.result == "승리"
                ? new Color(0.2f, 0.8f, 0.2f, 0.3f)
                : new Color(0.8f, 0.2f, 0.2f, 0.3f);
        }

        item.SetActive(true);
    }

    private void SelectLogEntry(LogEntry log, Button clickedButton)
    {
        // 이전 선택 해제
        if (selectedButton != null)
        {
            RestoreButtonColor(selectedButton);
        }

        // 새 선택 적용
        selectedButton = clickedButton;
        var image = clickedButton.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(1f, 1f, 0f, 0.6f);
        }

        ShowLogDetail(log);
    }

    private void RestoreButtonColor(Button button)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            LogEntry log = allLogs.FirstOrDefault(l => l.GetSummary() == text.text);
            if (log != null)
            {
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = log.result == "승리"
                        ? new Color(0.2f, 0.8f, 0.2f, 0.3f)
                        : new Color(0.8f, 0.2f, 0.2f, 0.3f);
                }
            }
        }
    }

    private void ShowLogDetail(LogEntry log)
    {
        if (detailText != null)
        {
            detailText.text = log.fullConversation;
        }
    }

    // 외부에서 호출: 새 게임 완료 시 (단일 진입점)
    public void OnGameCompleted(ConversationLogger conversationLogger, GameStateManager gameStateManager)
    {
        Debug.Log("[LogPanel] OnGameCompleted 호출됨 - 블록체인 저장 시작");

        if (!isInitialized)
        {
            Debug.LogWarning("[LogPanel] 아직 초기화되지 않았습니다.");
            return;
        }

        if (blockchainManager == null)
        {
            Debug.LogError("[LogPanel] BlockchainLogManager가 없어서 저장할 수 없습니다!");
            return;
        }

        try
        {
            Debug.Log("[LogPanel] 블록체인에 게임 로그 저장 중...");

            // 블록체인에 저장
            blockchainManager.SaveGameLogToBlockchain();

            Debug.Log("[LogPanel] 블록체인 저장 요청 완료");

            // 저장 후 새로고침은 GameObject가 활성화된 경우에만
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(RefreshAfterSave());
            }
            else
            {
                Debug.Log("[LogPanel] LogPanel이 비활성화되어 있어서 백그라운드 새로고침은 건너뜀");
                // 백그라운드에서 데이터만 미리 로드 (코루틴 없이)
                LoadFromBlockchain();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LogPanel] 게임 완료 처리 실패: {ex.Message}");
        }
    }

    private IEnumerator RefreshAfterSave()
    {
        yield return new WaitForSeconds(2f);

        LoadFromBlockchain();

        if (logViewerPanel != null && logViewerPanel.activeSelf)
        {
            RefreshLogList();
        }

        Debug.Log("[LogPanel] 백그라운드 새로고침 완료");
    }

    // GameLogCoordinator에서 호출하는 새로고침 메서드
    public void RefreshFromBlockchain()
    {
        Debug.Log("[LogPanel] RefreshFromBlockchain 호출됨");
        LoadFromBlockchain();
        RefreshLogList();
    }

    public void LoadFromBlockchainDirectly()
    {
        LoadFromBlockchain(); 
    }

    // 수동 새로고침 버튼용
    [ContextMenu("Manual Refresh")]
    public void ManualRefresh()
    {
        LoadFromBlockchain();
        RefreshLogList();
    }

    // 디버깅용
    [ContextMenu("Debug: Print Current Stats")]
    public void DebugPrintStats()
    {
        Debug.Log($"[LogPanel] 현재 상태:");
        Debug.Log($"- allLogs.Count: {allLogs.Count}");
        Debug.Log($"- UI 자식 수: {(logListParent != null ? logListParent.childCount : -1)}");
        Debug.Log($"- statsText: {(statsText != null ? statsText.text : "null")}");
        Debug.Log($"- isUpdatingStats: {isUpdatingStats}");
    }
}
