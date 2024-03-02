using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

public class ScoreCounter : MonoBehaviour
{
    public static Action<int, int> scoreChanged;

    private int[] scoreSums;
    private Text foregroundText;
    private Text shadowText;

    public void Init(int[] scoreSums)
    {
        this.scoreSums = scoreSums;
        transform.localScale = Vector3.one;
    }

    void Start()
    {
        foregroundText = transform.Find("Score").GetComponent<Text>();
        shadowText = GetComponent<Text>();
        foregroundText.text = "S";
        shadowText.text = "S";
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 25f);
        scoreChanged += OnScoreChanged;
    }

    void OnDestroy() => scoreChanged -= OnScoreChanged;

    internal void OnScoreChanged(int totalScore, int noteIndex)
    {
        if (scoreSums == null) return;
        float percent = (float)totalScore / scoreSums[noteIndex] * 100;
        string score = Utils.ScoreLetter(percent);
        foregroundText.text = score;
        shadowText.text = score;
    }
}
