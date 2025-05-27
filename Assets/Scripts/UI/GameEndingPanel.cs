using Scripts;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameEndingPanel : MonoBehaviour
{
    public Button saveButton;
    public Button restartButton;
    public Button viewLogsButton;
    public GameFlowManager gameFlowManager;
    public LogPanel logPanel;
    public ConversationLogger conversationLogger;

    private bool isSaving = false; // 저장 중 상태 관리

    void Start()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(() =>
            {
                if (isSaving) return; // 중복 저장 방지

                Debug.Log("[GameEndingPanel] 저장 버튼 클릭됨");
                StartCoroutine(SaveAndUpdateUI());
            });
        }

        if (restartButton != null && gameFlowManager != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                gameFlowManager.RestartGame();
            });
        }

        if (viewLogsButton != null && logPanel != null)
        {
            viewLogsButton.onClick.AddListener(() =>
            {
                // 저장이 완료된 후에만 로그 뷰어 열기
                if (isSaving)
                {
                    Debug.Log("[GameEndingPanel] 저장 중이므로 잠시 후 다시 시도해주세요.");
                    return;
                }

                StartCoroutine(OpenLogViewerWithRefresh());
            });
        }
    }

    // 저장 후 UI 업데이트 코루틴
    private IEnumerator SaveAndUpdateUI()
    {
        isSaving = true;

        // 버튼 상태 변경
        saveButton.interactable = false;
        var buttonText = saveButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = "저장 중...";
        }

        bool saveSuccessful = false;

        // 1. 로그 저장 (코루틴 없이 직접 저장)
        if (logPanel != null && conversationLogger != null)
        {
            // LogPanel의 코루틴 대신 직접 저장 처리
            try
            {
                // 블록체인에 직접 저장
                var blockchainManager = logPanel.blockchainManager;
                if (blockchainManager != null)
                {
                    blockchainManager.SaveGameLogToBlockchain();
                    Debug.Log("[GameEndingPanel] 블록체인 저장 요청 완료");
                    saveSuccessful = true;
                }
                else
                {
                    Debug.LogError("[GameEndingPanel] BlockchainLogManager를 찾을 수 없습니다!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameEndingPanel] 저장 중 오류: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("[GameEndingPanel] 필수 컴포넌트가 없습니다!");
        }

        if (saveSuccessful)
        {
            // 2. 블록체인 저장 완료 대기 (최대 5초)
            float waitTime = 0f;
            while (waitTime < 5f)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;

                // 최소 2초 대기
                if (waitTime >= 2f)
                {
                    break;
                }
            }

            // 3. LogPanel 데이터 새로고침 (코루틴 없이)
            if (logPanel != null)
            {
                // 직접 데이터 로드 (코루틴 사용 안 함)
                logPanel.LoadFromBlockchainDirectly();
                Debug.Log("[GameEndingPanel] LogPanel 데이터 새로고침 완료");
            }

            // 4. 완료 상태 표시
            if (buttonText != null)
            {
                buttonText.text = "저장 완료";
            }

            Debug.Log("[GameEndingPanel] 저장 및 UI 업데이트 완료");
        }
        else
        {
            // 저장 실패
            if (buttonText != null)
            {
                buttonText.text = "저장 실패";
            }
        }

        isSaving = false;
    }

    // 로그 뷰어를 새로고침과 함께 열기
    private IEnumerator OpenLogViewerWithRefresh()
    {
        Debug.Log("[GameEndingPanel] 로그 뷰어 열기 (새로고침 포함)");

        // 1. 최신 데이터 로드
        if (logPanel != null)
        {
            logPanel.RefreshFromBlockchain();
        }

        // 2. 약간의 지연 후 뷰어 열기
        yield return new WaitForSeconds(0.2f);

        // 3. 로그 뷰어 열기
        if (logPanel != null)
        {
            logPanel.OpenViewer();
        }
    }

    System.Collections.IEnumerator ShowSaveComplete()
    {
        yield return new WaitForSeconds(1f);

        var buttonText = saveButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = "저장 완료";
        }
    }
}
