using UnityEngine;
using UnityEngine.UI;

public class ModalManager : MonoBehaviour
{
    [Header("모달 창들")]
    public GameObject logModal;
    public GameObject thinkingModal;

    [Header("로그 모달 설정")]
    public ScrollRect logModalScrollRect;
    public ConversationLogger conversationLogger; // 추가: ConversationLogger 연결

    void Start()
    {
        // ConversationLogger와 LogModal UI 연결
        if (conversationLogger != null && logModalScrollRect != null)
        {
            TMPro.TMP_Text logText = logModalScrollRect.GetComponentInChildren<TMPro.TMP_Text>();
            if (logText != null)
            {
                conversationLogger.SetLogDisplayUI(logText, logModalScrollRect);
                Debug.Log("[ModalManager] ConversationLogger와 LogModal UI 연결 완료");
            }
            else
            {
                Debug.LogWarning("[ModalManager] LogModal에서 TMP_Text를 찾을 수 없습니다!");
            }
        }
    }

    public void OpenLogModal()
    {
        if (logModal != null)
        {
            logModal.SetActive(true);
        }

        if (thinkingModal != null)
        {
            thinkingModal.SetActive(false);
        }

        // ConversationLogger를 통해 최신 로그로 UI 업데이트
        if (conversationLogger != null)
        {
            conversationLogger.UpdateLogUI();
        }

        // 스크롤을 맨 아래로 이동
        if (logModalScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            logModalScrollRect.verticalNormalizedPosition = 0f;
        }

        Debug.Log("[ModalManager] 로그 모달 열림");
    }

    public void OpenThinkingModal()
    {
        if (logModal != null)
        {
            logModal.SetActive(false);
        }

        if (thinkingModal != null)
        {
            thinkingModal.SetActive(true);
        }

        Debug.Log("[ModalManager] 생각 모달 열림");
    }

    public void CloseAllModals()
    {
        if (logModal != null)
        {
            logModal.SetActive(false);
        }

        if (thinkingModal != null)
        {
            thinkingModal.SetActive(false);
        }

        Debug.Log("[ModalManager] 모든 모달 닫힘");
    }

    // ConversationLogger 수동 연결 (Inspector에서 할당하지 않은 경우)
    public void SetConversationLogger(ConversationLogger logger)
    {
        conversationLogger = logger;

        if (logModalScrollRect != null)
        {
            TMPro.TMP_Text logText = logModalScrollRect.GetComponentInChildren<TMPro.TMP_Text>();
            if (logText != null)
            {
                conversationLogger.SetLogDisplayUI(logText, logModalScrollRect);
                Debug.Log("[ModalManager] ConversationLogger 수동 연결 완료");
            }
        }
    }

    // 디버깅용
    [ContextMenu("Debug: Refresh Log Modal")]
    public void DebugRefreshLogModal()
    {
        if (conversationLogger != null)
        {
            conversationLogger.DebugForceUIUpdate();
        }
    }
}
