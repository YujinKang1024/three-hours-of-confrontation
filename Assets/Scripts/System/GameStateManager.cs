using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    private Dictionary<EmotionType, int> emotionScores = new();
    public bool hasConfessed { get; private set; } = false;
    public bool isGameOver { get; private set; } = false;
    public bool isVictory { get; private set; } = false;

    public enum EmotionType
    {
        감정폭발,
        회유,
        공감,
        거짓말,
        평이
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        InitializeState();
    }

    public void InitializeState()
    {
        emotionScores = new Dictionary<EmotionType, int>();
        foreach (EmotionType type in System.Enum.GetValues(typeof(EmotionType)))
        {
            emotionScores[type] = 0;
        }

        hasConfessed = false;
        isGameOver = false;
        isVictory = false;
    }

    public void AddEmotionScore(EmotionType type, int value)
    {
        if (emotionScores.ContainsKey(type))
            emotionScores[type] += value;
    }

    public int GetEmotionScore(EmotionType type)
    {
        return emotionScores.TryGetValue(type, out int score) ? score : 0;
    }

    public void MarkConfession() => hasConfessed = true;
    public void MarkVictory() => isVictory = true;
    public void MarkGameOver() => isGameOver = true;
}
