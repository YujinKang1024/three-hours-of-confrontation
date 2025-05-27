using UnityEngine;
using System.Collections;
using Scripts;

public class GameLogCoordinator : MonoBehaviour
{
    [Header("연결된 컴포넌트")]
    public BlockchainLogManager blockchainManager;
    public LogPanel logPanel;
    public ConversationLogger conversationLogger;
    public GameStateManager gameStateManager;

    private static GameLogCoordinator _instance;
    public static GameLogCoordinator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameLogCoordinator>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameLogCoordinator");
                    _instance = go.AddComponent<GameLogCoordinator>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// 게임 완료 시 블록체인에 저장하고 UI 업데이트
    public void SaveGameCompletionLog()
    {
        if (blockchainManager == null)
        {
            Debug.LogError("[GameLogCoordinator] BlockchainLogManager가 할당되지 않았습니다!");
            return;
        }

        Debug.Log("[GameLogCoordinator] 게임 완료 로그 저장 시작");

        // 블록체인에 저장
        blockchainManager.SaveGameLogToBlockchain();

        // 저장 후 UI 업데이트 (코루틴으로 안전하게)
        StartCoroutine(WaitAndRefreshUI());
    }

    private IEnumerator WaitAndRefreshUI()
    {
        Debug.Log("[GameLogCoordinator] 블록체인 저장 후 UI 새로고침 대기 중...");

        // 블록체인 저장 완료 대기 (최대 5초)
        float waitTime = 0f;
        int initialLogCount = 0;

        // 현재 로그 개수 확인
        if (blockchainManager != null)
        {
            var logs = blockchainManager.GetParsedLogs();
            initialLogCount = logs.Count;
        }

        while (waitTime < 5f)
        {
            yield return new WaitForSeconds(0.5f);
            waitTime += 0.5f;

            // 새 로그가 추가되었는지 확인
            if (blockchainManager != null)
            {
                var currentLogs = blockchainManager.GetParsedLogs();
                if (currentLogs.Count > initialLogCount)
                {
                    Debug.Log($"[GameLogCoordinator] 새 로그 감지됨! {initialLogCount} → {currentLogs.Count}");
                    RefreshLogPanelUI();
                    yield break;
                }
            }
        }

        Debug.LogWarning("[GameLogCoordinator] 블록체인 저장 확인 타임아웃 - 강제 새로고침");
        RefreshLogPanelUI();
    }

    private void RefreshLogPanelUI()
    {
        if (logPanel != null)
        {
            // LogPanel이 활성화되어 있으면 즉시 새로고침
            if (logPanel.gameObject.activeInHierarchy)
            {
                logPanel.RefreshFromBlockchain();
            }
            else
            {
                Debug.Log("[GameLogCoordinator] LogPanel이 비활성화되어 있음. 다음에 열 때 새로고침됩니다.");
            }
        }
    }

    /// 컴포넌트들을 자동으로 찾아서 할당
    [ContextMenu("Auto Assign Components")]
    public void AutoAssignComponents()
    {
        if (blockchainManager == null)
            blockchainManager = FindObjectOfType<BlockchainLogManager>();

        if (logPanel == null)
            logPanel = FindObjectOfType<LogPanel>();

        if (conversationLogger == null)
            conversationLogger = FindObjectOfType<ConversationLogger>();

        if (gameStateManager == null)
            gameStateManager = FindObjectOfType<GameStateManager>();

        Debug.Log("[GameLogCoordinator] 컴포넌트 자동 할당 완료");
    }
}
