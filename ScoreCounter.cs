using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

public class ScoreCounter : MonoBehaviour
{
    public static Action<int, int> scoreChanged;

    private List<float[]> leveldata;
    private Text foregroundText;
    private Text shadowText;
    private float gameMaxScore;

    public void Init(List<float[]> _leveldata)
    {
        leveldata = _leveldata;
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
        gameMaxScore += Utils.GetGameMax(leveldata[noteIndex][1]);
        float percent = totalScore / gameMaxScore;
        string score = Utils.ScoreLetter(percent);
        foregroundText.text = score;
        shadowText.text = score;
    }
}
