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
    private decimal pb;

    private int maxScore;
    private int[] scoreLeftover;
    private int[] scoreSums;

    private decimal targetAcc;
    private decimal currentAcc;
    private float updateTimer;
    private float timeSinceLastScore;

    public void Init(List<float[]> _leveldata, decimal _pb)
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

        foregroundText.text = 100.FormatDecimals(50) + "%";
        shadowText.text = 100.FormatDecimals(50) + "%";

        foregroundText.supportRichText = true;
        shadowText.supportRichText = true;

        targetAcc = currentAcc = 100;
        updateTimer = timeSinceLastScore = 0;

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - 50f, rect.anchoredPosition.y - 25f);

        scoreChanged += OnScoreChanged;
    }

    void Update()
    {
        if (Plugin.animateCounter.Value)
        {
            updateTimer += Time.deltaTime;
            timeSinceLastScore += Time.deltaTime;
            if (updateTimer > .02f && currentAcc != targetAcc)
            {
                UpdateText(currentAcc);
                updateTimer = 0;
            }
        }
    }

    void OnDestroy() => scoreChanged -= OnScoreChanged;

    internal void OnScoreChanged(int totalScore, int noteIndex)
    {
        var percent = GetPercent(totalScore, noteIndex);

        if (Plugin.animateCounter.Value)
        {
            timeSinceLastScore = 0;
            targetAcc = percent;
        }
        else
            UpdateText(percent);
    }

    internal void UpdateText(decimal percent)
    {
        string percentText = percent.FormatDecimals(50) + "%";
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

    private decimal GetPercent(int totalScore, int noteIndex) =>
        Plugin.accType.Value switch
        {
            AccType.Increasing => (decimal)totalScore / maxScore * 100,
            AccType.Decreasing => (decimal)(totalScore + scoreLeftover[noteIndex]) / maxScore * 100,
            _ => (decimal)totalScore / scoreSums[noteIndex] * 100,
        };
}
