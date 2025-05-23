using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GPTManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button sendButton;
    public TMP_Text outputText;
    public GPTConnector gptConnector;
    public ChatBubble chatBubble;
    public TMP_Text playerDialogueText;
    public TMP_Text logUiText;
    public ScrollRect scrollRect;
    public TimeManager timeManager;
    public ConversationLogger conversationLogger;
    public GameObject timeOverButton;

    public void OnClickSend() => OnClickSend(inputField.text);

    void Start()
    {
        sendButton.onClick.AddListener(OnClickSend);

        inputField.onEndEdit.RemoveAllListeners();
        inputField.onSubmit.RemoveAllListeners();
    }

    public void OnClickSend(string input)
    {
        if (!inputField.interactable || !sendButton.interactable) return;
        if (string.IsNullOrWhiteSpace(input)) return;

        conversationLogger.AddEntry("[플레이어] :", input);
        logUiText.text += $"[당신] : {input}\n";
        playerDialogueText.SetText(input);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;

        inputField.text = "";

        StartCoroutine(gptConnector.RequestToCustomGPT(input, response =>
        {
            Debug.Log("[GPTManager] GPT 응답 수신:\n" + response);

            conversationLogger.AddEntry("[수현] :", response);
            logUiText.text += $"[진범] : {response}\n\n";
            Canvas.ForceUpdateCanvases();

            chatBubble.SetText(response);
        }));

        timeManager.DecreaseTime();
        inputField.ActivateInputField();
    }

    public void RequestFinalGptMessage(string history)
    {
        string finalPrompt = history + "\n\n지금까지 나눈 대화를 바탕으로, 플레이어에게 마지막 인사를 해주세요.";

        StartCoroutine(gptConnector.RequestToCustomGPT(finalPrompt, response =>
        {
            chatBubble.SetText(response, 84, () =>
            {
                timeManager.TriggerFinalDialogue();
            });
        }));
    }

    public void OnTimeOverButtonClicked()
    {
        string history = conversationLogger.GetLogText();
        RequestFinalGptMessage(history);
        timeOverButton.SetActive(false);
    }

    public void DisableInputField()
    {
        inputField.text = "약속한 시간이 다 되었다. 더는 대화할 수 없다.";
        inputField.interactable = false;
        sendButton.interactable = false;
    }
}
