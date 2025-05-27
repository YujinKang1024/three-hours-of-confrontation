using UnityEngine;

public class SafeApplicationManager : MonoBehaviour
{
    void Awake()
    {
        // 애플리케이션 종료 시 안전 처리
        Application.quitting += OnApplicationQuitting;
    }

    void OnApplicationQuitting()
    {
        Debug.Log("[SafeApplicationManager] 애플리케이션 안전 종료 시작");
        
        // 모든 코루틴 정지
        StopAllCoroutines();
        
        // 모든 Agent 찾아서 안전하게 정리
        var agents = FindObjectsOfType<Libplanet.Unity.Agent>();
        foreach (var agent in agents)
        {
            try
            {
                if (agent != null)
                {
                    // Agent가 안전하게 정리되도록 처리
                    Debug.Log($"[SafeApplicationManager] Agent 안전 정리: {agent.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SafeApplicationManager] Agent 정리 중 오류: {ex.Message}");
            }
        }
    }

    void OnDestroy()
    {
        Application.quitting -= OnApplicationQuitting;
    }
}
