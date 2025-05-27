using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Scripts;

public class BlockchainDataCleaner : MonoBehaviour
{
    [Header("연결된 컴포넌트")]
    public BlockchainLogManager blockchainManager;

    [Header("정리 옵션")]
    [Tooltip("중복 제거 후 정리된 데이터로 덮어쓸지 여부")]
    public bool overwriteWithCleanData = false;

    [Header("디버그 정보")]
    [SerializeField] private int totalLogsFound = 0;
    [SerializeField] private int duplicatesFound = 0;
    [SerializeField] private int cleanLogsCount = 0;

    [ContextMenu("1. 중복 로그 분석")]
    public void AnalyzeDuplicateLogs()
    {
        if (blockchainManager == null)
        {
            Debug.LogError("[BlockchainDataCleaner] BlockchainLogManager가 할당되지 않았습니다!");
            return;
        }

        Debug.Log("[BlockchainDataCleaner] === 중복 로그 분석 시작 ===");

        try
        {
            var allLogs = blockchainManager.GetParsedLogs();
            totalLogsFound = allLogs.Count;

            Debug.Log($"[BlockchainDataCleaner] 블록체인에서 찾은 총 로그 수: {totalLogsFound}");

            // 중복 분석
            var duplicateGroups = allLogs
                .GroupBy(log => $"{log.timestamp}_{log.result}")
                .Where(group => group.Count() > 1)
                .ToList();

            duplicatesFound = duplicateGroups.Sum(group => group.Count() - 1);
            cleanLogsCount = totalLogsFound - duplicatesFound;

            Debug.Log($"[BlockchainDataCleaner] 중복 그룹 수: {duplicateGroups.Count}");
            Debug.Log($"[BlockchainDataCleaner] 중복 로그 수: {duplicatesFound}");
            Debug.Log($"[BlockchainDataCleaner] 정리 후 로그 수: {cleanLogsCount}");

            // 중복 상세 정보
            foreach (var group in duplicateGroups)
            {
                Debug.LogWarning($"[BlockchainDataCleaner] 중복 발견: '{group.Key}' - {group.Count()}개");
            }

            Debug.Log("[BlockchainDataCleaner] === 중복 로그 분석 완료 ===");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BlockchainDataCleaner] 분석 중 오류: {ex.Message}");
        }
    }

    [ContextMenu("2. 정리된 로그 미리보기")]
    public void PreviewCleanedLogs()
    {
        if (blockchainManager == null)
        {
            Debug.LogError("[BlockchainDataCleaner] BlockchainLogManager가 할당되지 않았습니다!");
            return;
        }

        Debug.Log("[BlockchainDataCleaner] === 정리된 로그 미리보기 ===");

        try
        {
            var allLogs = blockchainManager.GetParsedLogs();
            var cleanedLogs = RemoveDuplicates(allLogs);

            Debug.Log($"[BlockchainDataCleaner] 원본 로그 수: {allLogs.Count}");
            Debug.Log($"[BlockchainDataCleaner] 정리된 로그 수: {cleanedLogs.Count}");

            Debug.Log("[BlockchainDataCleaner] 정리된 로그 목록:");
            for (int i = 0; i < cleanedLogs.Count; i++)
            {
                var log = cleanedLogs[i];
                Debug.Log($"[BlockchainDataCleaner] {i + 1}. [{log.timestamp}] {log.playerName} - {log.result}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BlockchainDataCleaner] 미리보기 중 오류: {ex.Message}");
        }
    }

    [ContextMenu("3. 블록체인 데이터 완전 삭제 (주의!)")]
    public void ClearAllBlockchainData()
    {
        Debug.LogWarning("[BlockchainDataCleaner] ⚠️ 블록체인 데이터 완전 삭제 - 이 작업은 되돌릴 수 없습니다!");

        if (!Application.isEditor)
        {
            Debug.LogError("[BlockchainDataCleaner] 이 기능은 에디터 모드에서만 사용할 수 있습니다!");
            return;
        }

        // 에디터에서만 실행되도록 추가 안전장치
#if UNITY_EDITOR
        if (UnityEditor.EditorUtility.DisplayDialog(
            "블록체인 데이터 삭제",
            "정말로 모든 블록체인 로그 데이터를 삭제하시겠습니까?\n\n이 작업은 되돌릴 수 없습니다!",
            "삭제", "취소"))
        {
            try
            {
                Debug.Log("[BlockchainDataCleaner] 블록체인 데이터 삭제 완료");

                if (blockchainManager != null)
                {
                    var logPanel = FindObjectOfType<LogPanel>();
                    if (logPanel != null)
                    {
                        logPanel.RefreshFromBlockchain();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[BlockchainDataCleaner] 데이터 삭제 중 오류: {ex.Message}");
            }
        }
#endif
    }

    private List<LogPanel.LogEntry> RemoveDuplicates(List<LogPanel.LogEntry> logs)
    {
        var seen = new HashSet<string>();
        var cleanedLogs = new List<LogPanel.LogEntry>();

        // 최신순으로 정렬된 로그에서 중복 제거
        var sortedLogs = logs.OrderByDescending(log =>
            System.DateTime.TryParse(log.timestamp, out var date) ? date : System.DateTime.MinValue);

        foreach (var log in sortedLogs)
        {
            string uniqueKey = $"{log.timestamp}_{log.result}";

            if (!seen.Contains(uniqueKey))
            {
                seen.Add(uniqueKey);
                cleanedLogs.Add(log);
            }
        }

        return cleanedLogs.OrderByDescending(log =>
            System.DateTime.TryParse(log.timestamp, out var date) ? date : System.DateTime.MinValue)
            .ToList();
    }

    void OnValidate()
    {
        // Inspector에서 컴포넌트 자동 할당
        if (blockchainManager == null)
        {
            blockchainManager = FindObjectOfType<BlockchainLogManager>();
        }
    }
}
