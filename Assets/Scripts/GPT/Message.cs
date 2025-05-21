using System.Collections.Generic;

[System.Serializable]
public class Message
{
    public string role;
    public string content;

    public Message(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}
