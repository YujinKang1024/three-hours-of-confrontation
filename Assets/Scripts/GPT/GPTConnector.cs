using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class GPTConnector : MonoBehaviour
{
    private string apiKey;
    private string systemMessage;

    void Start()
    {
        apiKey = LoadResource("apiKey");
        systemMessage = LoadResource("systemPrompt");
        Debug.Log("[GPTConnector] API Key Loaded: " + (string.IsNullOrEmpty(apiKey) ? "❌" : "✅"));
    }

    string LoadResource(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset == null)
        {
            Debug.LogError($"[GPTConnector] {fileName}.txt 로딩 실패");
            return "";
        }
        return textAsset.text.Trim();
    }

    public IEnumerator RequestToCustomGPT(string userMessage, System.Action<string> onResponse)
    {
        string url = "https://api.openai.com/v1/chat/completions";
        ChatRequest chatRequest = new ChatRequest(systemMessage, userMessage);
        string jsonData = JsonUtility.ToJson(chatRequest);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("[GPTConnector] 응답 수신:\n" + json);
            string message = GPTJsonParser.ExtractMessage(json);
            onResponse?.Invoke(message);
        }
        else
        {
            Debug.LogError("GPT 요청 실패: " + request.error);
            Debug.LogError("응답 코드: " + request.responseCode);
            Debug.LogError("응답 내용: " + request.downloadHandler.text);
            onResponse?.Invoke("에러 발생");
        }
    }
}
