using System.Collections.Generic;

[System.Serializable]
public class ChatRequest
{
    public string model = "gpt-4o";
    public List<Message> messages;
    public float temperature = 0.7f;

    public ChatRequest(string systemMessage, string userMessage)
    {
        messages = new List<Message>
        {
            new Message("system", systemMessage),
            new Message("user", userMessage)
        };
    }
}
