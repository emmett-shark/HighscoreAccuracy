using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

public class PercentCounter : MonoBehaviour
{
    public static Action<int, int> scoreChanged;

    private List<float[]> leveldata;
    private Text foregroundText;
    private Text shadowText;
    private float pb;

    private int maxScore;
    private int[] scoreLeftover;
    private int[] scoreSums;

    public void Init(List<float[]> _leveldata, float _pb)
    {
        pb = _pb;
        leveldata = _leveldata;
        transform.localScale = Vector3.one;

        var scorePerNote = Utils.GetMaxScores(Plugin.accType.Value, leveldata).ToArray();
        scoreSums = new int[scorePerNote.Length];
        for (int i = 0; i < scorePerNote.Length; i++)
        {
            maxScore += scorePerNote[i];
            scoreSums[i] = maxScore;
        }
        int maxScoreLeftOver = maxScore;
        scoreLeftover = new int[scorePerNote.Length];
        for (int i = 0; i < scorePerNote.Length; i++)
        {
            maxScoreLeftOver -= scorePerNote[i];
            scoreLeftover[i] = maxScoreLeftOver;
        }
    }

    void Start()
    {
        foregroundText = transform.Find("Score").GetComponent<Text>();
        shadowText = GetComponent<Text>();

        foregroundText.text = 100.FormatDecimals() + "%";
        shadowText.text = 100.FormatDecimals() + "%";

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - 50f, rect.anchoredPosition.y - 25f);

        scoreChanged += OnScoreChanged;
    }

    void OnDestroy() => scoreChanged -= OnScoreChanged;

    internal void OnScoreChanged(int totalScore, int noteIndex)
    {
        float percent = GetPercent(totalScore, noteIndex);
        string percentText = percent.FormatDecimals() + "%";
        foregroundText.text = percentText;
        shadowText.text = percentText;

        if (percent > pb)
        {
            foregroundText.color = Color.green;
        }
        else if (percent < pb - 10)
        {
            foregroundText.color = Color.red;
        }
        else
        {
            foregroundText.color = Color.yellow;
        }
    }

    private float GetPercent(int totalScore, int noteIndex) =>
        Plugin.accType.Value switch
        {
            AccType.Increasing => (float)totalScore / maxScore * 100,
            AccType.Decreasing => (float)(totalScore + scoreLeftover[noteIndex]) / maxScore * 100,
            _ => (float)totalScore / scoreSums[noteIndex] * 100,
        };
}
