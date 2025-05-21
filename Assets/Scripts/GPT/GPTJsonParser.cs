using UnityEngine;

[System.Serializable]
public class GPTChoice
{
    public GPTMessage message;
}

[System.Serializable]
public class GPTMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class GPTResponse
{
    public GPTChoice[] choices;
}

public static class GPTJsonParser
{
    public static string ExtractMessage(string json)
    {
        try
        {
            GPTResponse response = JsonUtility.FromJson<GPTResponse>(json);
            if (response.choices != null && response.choices.Length > 0)
            {
                return response.choices[0].message.content;
            }
            return "응답 없음";
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[GPTJsonParser] JSON 파싱 오류: " + ex.Message);
            return "응답 파싱 실패";
        }
    }
}
