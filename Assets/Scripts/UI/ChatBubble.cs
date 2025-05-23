using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    public TMP_Text textDisplay;
    public Button nextButton;
    public Button prevButton;

    private List<string> splitTexts = new();
    private int currentIndex = 0;
    private System.Action onComplete;

    void Start()
    {
        string longText = "······.";
        SplitText(longText, 84);
        ShowTextPage(0);

        nextButton.onClick.AddListener(ShowNext);
        prevButton.onClick.AddListener(ShowPrev);
    }


    public void SetText(string text, int maxChars = 84, System.Action onComplete = null)
    {
        this.onComplete = onComplete;
        SplitText(text, maxChars);
        ShowTextPage(0);
    }

    void SplitText(string text, int maxChars)
    {
        splitTexts.Clear();
        for (int i = 0; i < text.Length; i += maxChars)
        {
            int len = Mathf.Min(maxChars, text.Length - i);
            splitTexts.Add(text.Substring(i, len));
        }
    }

    void ShowTextPage(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, splitTexts.Count - 1);
        textDisplay.text = splitTexts[currentIndex];

        prevButton.gameObject.SetActive(currentIndex > 0);
        nextButton.gameObject.SetActive(currentIndex < splitTexts.Count - 1);
    }

    
    void ShowNext() => ShowTextPage(currentIndex + 1);
    void ShowPrev() => ShowTextPage(currentIndex - 1);

}
