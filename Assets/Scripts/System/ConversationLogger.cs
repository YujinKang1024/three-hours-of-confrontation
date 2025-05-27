using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConversationLogger : MonoBehaviour
{
    [Header("로그 데이터")]
    public List<string> log = new();

    [Header("UI 연동")]
    public TMP_Text logDisplayText;
    public ScrollRect logScrollRect;
    public bool autoUpdateUI = true;

    [Header("이벤트")]
    public UnityEvent<string> OnLogUpdated; 

    private string cachedLogText = "";

    void Start()
    {
        if (autoUpdateUI)
        {
            UpdateLogUI();
        }
    }

    public void AddEntry(string speaker, string text)
    {
        string formattedEntry = $"{speaker} {text}";
        log.Add(formattedEntry);

        Debug.Log($"[ConversationLogger] 로그 추가: {speaker} {text}");

        if (autoUpdateUI)
        {
            UpdateLogUI();
        }

        OnLogUpdated?.Invoke(formattedEntry);
    }

    public void AddEntryForUI(string speaker, string text, string displayFormat = null)
    {
        string standardEntry = $"{speaker} {text}";
        log.Add(standardEntry);

        Debug.Log($"[ConversationLogger] UI 전용 로그 추가: {speaker} {text}");

        if (autoUpdateUI)
        {
            UpdateLogUI(displayFormat);
        }

        OnLogUpdated?.Invoke(standardEntry);
    }

    public string GetLogText(int maxEntries = -1)
    {
        if (maxEntries == -1 || maxEntries >= log.Count)
        {
            return string.Join("\n", log);
        }

        int start = Mathf.Max(0, log.Count - maxEntries);
        return string.Join("\n", log.GetRange(start, log.Count - start));
    }

    // GPT 프롬프트용 (메모리 제한을 위해 최근 대화만)
    public string GetRecentLogText(int maxEntries = 20)
    {
        int start = Mathf.Max(0, log.Count - maxEntries);
        return string.Join("\n", log.GetRange(start, log.Count - start));
    }

    // UI 표시용 텍스트 가져오기 (포맷팅 적용)
    public string GetDisplayText(string customFormat = null, int maxDisplayEntries = -1)
    {
        if (log.Count == 0) return "대화 기록이 없습니다.";

        List<string> displayEntries = new List<string>();

        List<string> logsToDisplay = log;
        if (maxDisplayEntries > 0 && maxDisplayEntries < log.Count)
        {
            int start = log.Count - maxDisplayEntries;
            logsToDisplay = log.GetRange(start, maxDisplayEntries);

            displayEntries.Add($"... (이전 {log.Count - maxDisplayEntries}개 대화 생략) ...\n");
        }

        foreach (string entry in logsToDisplay)
        {
            string displayEntry = FormatEntryForDisplay(entry, customFormat);
            displayEntries.Add(displayEntry);
        }

        return string.Join("\n", displayEntries);
    }

    // UI 업데이트 (LogModal 텍스트 갱신)
    public void UpdateLogUI(string customFormat = null, int maxDisplayEntries = -1)
    {
        if (logDisplayText == null) return;

        string displayText = GetDisplayText(customFormat, maxDisplayEntries);

        if (displayText != cachedLogText)
        {
            logDisplayText.text = displayText;
            cachedLogText = displayText;

            if (logScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                logScrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }

    private string FormatEntryForDisplay(string entry, string customFormat = null)
    {
        // 커스텀 포맷이 지정된 경우
        if (!string.IsNullOrEmpty(customFormat))
        {
            return string.Format(customFormat, entry);
        }

        // 기본 포맷팅: "[플레이어] :" → "[당신] :"
        if (entry.StartsWith("[플레이어] :"))
        {
            return entry.Replace("[플레이어] :", "[당신] :");
        }

        // "[수현] :" → "[진범] :"
        if (entry.StartsWith("[수현] :"))
        {
            return entry.Replace("[수현] :", "[진범] :");
        }

        return entry;
    }

    // 로그 클리어
    public void ClearLog()
    {
        log.Clear();
        cachedLogText = "";

        if (autoUpdateUI)
        {
            UpdateLogUI();
        }

        Debug.Log("[ConversationLogger] 로그 클리어됨");
    }

    public void SetLogDisplayUI(TMP_Text displayText, ScrollRect scrollRect = null)
    {
        logDisplayText = displayText;
        logScrollRect = scrollRect;

        if (autoUpdateUI)
        {
            UpdateLogUI();
        }
    }

    public void SetAutoUpdateUI(bool enabled)
    {
        autoUpdateUI = enabled;

        if (enabled)
        {
            UpdateLogUI();
        }
    }

    public void AddPlayerEntry(string text)
    {
        AddEntry("[플레이어] :", text);
    }

    public void AddNPCEntry(string text)
    {
        AddEntry("[수현] :", text);
    }

    // 디버깅용
    [ContextMenu("Debug: Print All Logs")]
    public void DebugPrintLogs()
    {
        Debug.Log($"[ConversationLogger] === 전체 로그 ({log.Count}개) ===");
        for (int i = 0; i < log.Count; i++)
        {
            Debug.Log($"[ConversationLogger] {i + 1}: {log[i]}");
        }
    }

    [ContextMenu("Debug: Force UI Update")]
    public void DebugForceUIUpdate()
    {
        cachedLogText = ""; 
        UpdateLogUI();
    }
}
