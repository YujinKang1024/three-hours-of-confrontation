using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameFlowManager : MonoBehaviour
{
    public TimeManager timeManager;
    public GameObject gameEndingPanel;
    public TMP_Text endingTitle;
    public TMP_Text endingMessage;
    public float endingDelaySeconds = 5f;

    public void RestartGame()
    {
        GameStateManager.Instance.InitializeState();
        if (timeManager != null) timeManager.ResetTime();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
