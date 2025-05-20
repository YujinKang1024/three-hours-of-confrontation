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

    void Start()
    {
        string longText = "거 참, 영문을 모르겠네요. 무슨 말씀을 하시는 겁니까? 분할 테스트용 테스트 작성 중입니다.";
        SplitText(longText, 40);
        ShowTextPage(0);

        nextButton.onClick.AddListener(ShowNext);
        prevButton.onClick.AddListener(ShowPrev);
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
