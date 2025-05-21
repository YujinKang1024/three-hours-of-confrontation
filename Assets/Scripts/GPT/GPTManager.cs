using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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

    void Start()
    {
        sendButton.onClick.AddListener(OnClickSend);
    }
    
    public void OnEndEdit(string _)
    {
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            StartCoroutine(DeferredSend());
        }
    }

    public void OnClickSend()
    {
        string input = inputField.text;
        if (string.IsNullOrWhiteSpace(input)) return;

        logUiText.text += $"[당신] : {input}\n";
        playerDialogueText.SetText(input);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;

        inputField.text = "";

        StartCoroutine(gptConnector.RequestToCustomGPT(input, response =>
        {
            Debug.Log("[GPTManager] GPT 응답 수신:\n" + response);

            logUiText.text += $"[진범] : {response}\n\n";
            Canvas.ForceUpdateCanvases();

            chatBubble.SetText(response);
        }));

        inputField.ActivateInputField();
    }

    private IEnumerator DeferredSend()
    {
        yield return null;
        OnClickSend(); 
    }
}
