using System.Collections.Generic;
using UnityEngine;

public class ConversationLogger : MonoBehaviour
{
    public List<string> log = new();

    public void AddEntry(string speaker, string text)
    {
        log.Add($"{speaker}: {text}");
    }

    public string GetLogText(int maxEntries = 20)
    {
        int start = Mathf.Max(0, log.Count - maxEntries);
        return string.Join("\n", log.GetRange(start, log.Count - start));
    }
}
