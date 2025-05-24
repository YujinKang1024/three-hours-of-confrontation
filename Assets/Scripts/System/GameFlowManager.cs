using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public TimeManager timeManager;
    public GameObject gameOverPanel;

    public void RestartGame()
    {
        GameStateManager.Instance.InitializeState();

        if (timeManager != null)
            timeManager.ResetTime();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TriggerGameOver(float delaySeconds = 0f)
    {
        if (delaySeconds > 0)
            StartCoroutine(GameOverAfterDelay(delaySeconds));
        else
            gameOverPanel.SetActive(true);
    }

    IEnumerator GameOverAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ShowGameOverPanel();
    }

    void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true); 
    }
}
