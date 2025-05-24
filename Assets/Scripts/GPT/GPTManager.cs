using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    public GameFlowManager gameFlowManager;

    void Start()
    {
        sendButton.onClick.AddListener(OnClickSend);

        inputField.onEndEdit.RemoveAllListeners();
        inputField.onSubmit.RemoveAllListeners();
    }

    public void OnClickSend() => OnClickSend(inputField.text);

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

        string history = conversationLogger.GetLogText();
        string systemPrompt = GetSystemPrompt();
        string prompt = history + $"\n\n[플레이어] : {input}";

        StartCoroutine(gptConnector.RequestToCustomGPT(systemPrompt, prompt, response =>
        {
            Debug.Log("[GPTManager] GPT 응답 수신:\n" + response);

            UpdateEmotionScore(response);
            CheckConfessionTag(response);
            string cleanResponse = CleanGPTResponse(response);

            conversationLogger.AddEntry("[수현] :", cleanResponse);
            logUiText.text += $"[진범] : {cleanResponse}\n\n";
            Canvas.ForceUpdateCanvases();

            chatBubble.SetText(cleanResponse);
        }));

        timeManager.DecreaseTime();
        inputField.ActivateInputField();
    }

    string CleanGPTResponse(string response)
    {
        response = Regex.Replace(response, @"\[[^\[\]]+\s\+\d+\]", "").Trim();
        response = Regex.Replace(response, @"\[자백\]", "").Trim();
        response = Regex.Replace(response, @"^\[?수현\]?\s*::?\s*", "", RegexOptions.IgnoreCase).Trim();
        return response;
    }

    void UpdateEmotionScore(string response)
    {
        var match = Regex.Match(response, @"\[(감정폭발|회유|공감|거짓말|평이) \+(\d+)\]");
        if (match.Success)
        {
            string tagString = match.Groups[1].Value;
            int value = int.Parse(match.Groups[2].Value);

            if (Enum.TryParse(tagString, out GameStateManager.EmotionType type))
            {
                GameStateManager.Instance.AddEmotionScore(type, value);

                Debug.Log($"[GPTManager] 감정 태그 감지: {type} +{value} (누적: {GameStateManager.Instance.GetEmotionScore(type)})");
            }
            else
            {
                Debug.LogWarning($"[GPTManager] 알 수 없는 감정 태그: {tagString}");
            }
        }
    }

    void CheckConfessionTag(string response)
    {
        if (response.Contains("[자백]"))
        {
            GameStateManager.Instance.MarkConfession();
            Debug.Log("[GPTManager] 자백 태그 감지됨");
        }
    }

    string GetSystemPrompt()
    {
        int angerScore = Mathf.Max(
            GameStateManager.Instance.GetEmotionScore(GameStateManager.EmotionType.감정폭발),
            GameStateManager.Instance.GetEmotionScore(GameStateManager.EmotionType.거짓말)
        );

        int empathyScore = Mathf.Max(
            GameStateManager.Instance.GetEmotionScore(GameStateManager.EmotionType.회유),
            GameStateManager.Instance.GetEmotionScore(GameStateManager.EmotionType.공감)
        );

        bool canAngry = angerScore >= 12;
        bool canEmpathy = empathyScore >= 10;

        if (canAngry && canEmpathy)
        {
            return (angerScore >= empathyScore)
                ? LoadPrompt("systemPrompt_angry.txt")
                : LoadPrompt("systemPrompt_sympathy.txt");
        }

        if (canAngry) return LoadPrompt("systemPrompt_angry.txt");
        if (canEmpathy) return LoadPrompt("systemPrompt_sympathy.txt");

        return LoadPrompt("systemPrompt.txt");
    }

    string LoadPrompt(string fileName)
    {
        string path = Application.streamingAssetsPath + "/" + fileName;
        return System.IO.File.ReadAllText(path);
    }

    public void DisableInputField()
    {
        inputField.text = "약속한 시간이 다 되었다. 더는 대화할 수 없다.";
        inputField.interactable = false;
        sendButton.interactable = false;
    }
    public void RequestFinalGptMessage(string history)
    {
        string finalSystemPrompt = LoadPrompt("systemPrompt_final.txt");
        string finalPrompt = history + "\n\n위 상황을 참고해, 마지막 인사를 정리하여 플레이어에게 전하세요.";

        StartCoroutine(gptConnector.RequestToCustomGPT(finalSystemPrompt, finalPrompt, response =>
        {
            string cleanResponse = CleanGPTResponse(response);

            chatBubble.SetText(cleanResponse, 84, () =>
            {
                Debug.Log("콜백 실행됨 - 게임 오버 딜레이 시작");
                gameFlowManager.TriggerGameOver(5f);
            });
        }));
    }

    public void OnTimeOverButtonClicked()
    {
        string history = conversationLogger.GetLogText();
        RequestFinalGptMessage(history);
        timeOverButton.SetActive(false);
    }

}
