using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public int remainingTime = 180; // 초기 시간
    public int timePerChat = 5;     // 채팅 1회당 감소 시간
    public TMP_Text timeText;       // 시간 UI 
    public GameObject gameOverPanel; // 게임 오버 패널 
    public GPTManager gptManager;
    public ConversationLogger conversationLogger;
    public GameObject timeOverButton;

    void Start()
    {
        UpdateTimeUI();
        gameOverPanel.SetActive(false);
    }

    public void DecreaseTime()
    {
        remainingTime -= timePerChat;
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            UpdateTimeUI();

            gptManager.DisableInputField();
            timeOverButton.SetActive(true);
        }
        UpdateTimeUI();
    }

    void UpdateTimeUI()
    {
        timeText.text = remainingTime + "분";
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void TriggerFinalDialogue()
    {
        string history = gptManager.conversationLogger.GetLogText();
        string prompt = $"{history}\n\n" +
            "당신은 지금까지 플레이어와 180분 대화를 나눈 캐릭터입니다.\n" +
            "이제 시간이 다 되었고, 플레이어에게 마지막 작별 인사를 해줘야 합니다.";

        gptManager.RequestFinalGptMessage(prompt);
    }
}
