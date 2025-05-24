using System.Collections;   
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public int remainingTime = 60;
    public int timePerChat = 2;    
    public TMP_Text timeText;       
    public GameObject gameEndingPanel;
    public ConversationLogger conversationLogger;
    public GameObject endingButton;
    public GPTManager gptManager;

    void Start()
    {
        UpdateTimeUI();
        gameEndingPanel.SetActive(false);
    }

    public void DecreaseTime()
    {
        remainingTime -= timePerChat;
        if (remainingTime <= 0)
        {
            remainingTime = 0;
            UpdateTimeUI();

            endingButton.SetActive(true);
            gptManager.DisableInputField();
        }

        UpdateTimeUI();
    }

    void UpdateTimeUI()
    {
        timeText.text = remainingTime + "분";
    }

    public void ResetTime()
    {
        remainingTime = 180;
        UpdateTimeUI();
        endingButton.SetActive(false);
    }
}
