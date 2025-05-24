using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    public Button saveButton;
    public Button restartButton;
    public GameFlowManager gameFlowManager;

    void Start()
    {
        saveButton.onClick.AddListener(() =>
        {
            Debug.Log("저장 기능은 아직 구현되지 않았습니다.");
        });

        restartButton.onClick.AddListener(() =>
        {
            gameFlowManager.RestartGame();
        });
    }
}
