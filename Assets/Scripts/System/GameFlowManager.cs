using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using Scripts;

public class GameFlowManager : MonoBehaviour
{
    public TimeManager timeManager;
    public GameObject gameEndingPanel;
    public TMP_Text endingTitle;
    public TMP_Text endingMessage;
    public float endingDelaySeconds = 5f;

    public void RestartGame()
    {
        Debug.Log("[GameFlowManager] 게임 재시작 시작...");
        
        try
        {
            // 1. 모든 코루틴 정지
            StopAllCoroutines();
            
            // 2. GameState 초기화
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.InitializeState();
            }
            
            // 3. TimeManager 리셋
            if (timeManager != null)
            {
                timeManager.ResetTime();
            }
            
            // 4. 블록체인 관련 정리 (안전하게)
            var blockchainManager = FindObjectOfType<BlockchainLogManager>();
            if (blockchainManager != null)
            {
                // 블록체인 매니저는 그대로 두고 상태만 리셋
                Debug.Log("[GameFlowManager] 블록체인 매니저 상태 유지");
            }
            
            // 5. 씬 리로드 (코루틴으로 안전하게)
            StartCoroutine(SafeSceneReload());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameFlowManager] 재시작 중 오류: {ex.Message}");
            // 오류 발생 시 강제 씬 리로드
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    System.Collections.IEnumerator SafeSceneReload()
    {
        Debug.Log("[GameFlowManager] 안전한 씬 리로드 시작");
        
        // 약간 대기 후 씬 리로드
        yield return new WaitForSeconds(0.1f);
        
        try
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameFlowManager] 씬 리로드 실패: {ex.Message}");
        }
    }

    public void ShowEndingDelayed(bool isVictory)
    {
        StartCoroutine(ShowEndingWithDelay(isVictory, endingDelaySeconds));
    }

    IEnumerator ShowEndingWithDelay(bool isVictory, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowEnding(isVictory);
    }

    void ShowEnding(bool isVictory)
    {
        if (gameEndingPanel != null)
            gameEndingPanel.SetActive(true);

        if (endingTitle != null && endingMessage != null)
        {
            if (isVictory)
            {
                endingTitle.text = "<color=#4AA2F0>승리했습니다</color>";
                endingMessage.text = "진범에게서 자백을 받아냈습니다. 이제는 일상으로 돌아갈 때입니다.";
            }
            else
            {
                endingTitle.text = "<color=#F05D5D>패배하였습니다</color>";
                endingMessage.text = "시간 내에 진범의 자백을 받아내지 못했습니다.";
            }
        }
    }
    
    public void TriggerGameOver()
    {
        ShowEndingDelayed(false);
    }
}
