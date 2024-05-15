using System;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

public class PercentCounter : MonoBehaviour
{
    public static Action<int, int> scoreChanged;

    private Text foregroundText;
    private Text shadowText;

    /// <summary>
    /// Personal best score
    /// </summary>
    private int pbScore;

    /// <summary>
    /// Personal best percentage, on a scale of 0 - 100
    /// </summary>
    private float pbPercent;

    private int maxScore;
    private int[] scoreLeftover;
    private int[] scoreSums;

    private float targetAcc;
    private float currentAcc;
    private float updateTimer;
    private float timeSinceLastScore;

    public void Init(int maxScore, int[] scoreLeftover, int[] scoreSums, int pbScore, float pbPercent)
    {
        this.maxScore = maxScore;
        this.scoreLeftover = scoreLeftover;
        this.scoreSums = scoreSums;
        this.pbScore = pbScore;
        this.pbPercent = pbPercent;
        transform.localScale = Vector3.one;
    }

    void Start()
    {
        foregroundText = transform.Find("Score").GetComponent<Text>();
        shadowText = GetComponent<Text>();

        foregroundText.text = 100.FormatDecimals() + "%";
        shadowText.text = 100.FormatDecimals() + "%";

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
                currentAcc = EaseValue(currentAcc, targetAcc - currentAcc, timeSinceLastScore, .6f);
                UpdateText(currentAcc);
                updateTimer = 0;
            }
        }
    }

    void OnDestroy() => scoreChanged -= OnScoreChanged;

    internal void OnScoreChanged(int totalScore, int noteIndex)
    {
        if (scoreLeftover == null || scoreSums == null || maxScore == 0) return;
        float percent = GetPercent(totalScore, noteIndex);

        if (Plugin.animateCounter.Value)
        {
            timeSinceLastScore = 0;
            targetAcc = percent;
        }
        else
        {
            UpdateText(percent);
        }

        // If using `PbPossibility` or `Hybrid` color behavior, we update the color here
        // otherwise we update it in UpdateText (`PbPossibility` doesn't support coloring with an animated counter)
        //
        // PB Possibility and Hybrid color behaviors are pretty similar, hybrid only adds an orange color
        // so we handle them both here
        if (
            Plugin.colorBehavior.Value == ColorBehavior.PbPossibility ||
            Plugin.colorBehavior.Value == ColorBehavior.Hybrid
            )
        {
            if (totalScore > pbScore)
            {
                // A PB is inevitable, dark green
                foregroundText.color = new Color(0f, 0.7f, 0f);
            }
            else if (percent > pbPercent)
            {
                // You are currently above your PB, green
                foregroundText.color = Color.green;
            }
            else
            {
                float possibleRemainingScore = scoreLeftover[noteIndex];
                if (totalScore + possibleRemainingScore > pbScore)
                {
                    // You are below your PB but can still make a new PB
                    if (Plugin.colorBehavior.Value == ColorBehavior.Hybrid && percent < pbPercent - 10)
                    {
                        // We're in hybrid-mode and you're more than 10% off your PB, orange
                        foregroundText.color = new Color(1f, 0.5f, 0f);
                    }
                    else
                    {
                        // You're less than 10% off your PB, or you're not in hybrid mode, yellow
                        foregroundText.color = Color.yellow;
                    }
                }
                else
                {
                    // A PB is impossible, red
                    foregroundText.color = Color.red;
                }
            }
        }
    }

    internal void UpdateText(float percent)
    {
        string percentText = percent.FormatDecimals() + "%";
        foregroundText.text = percentText;
        shadowText.text = percentText;

        // If using `Closeness` color behavior, we update the color here
        // otherwise we update it in OnScoreChanged (`PbPossibility` doesn't support coloring with an animated counter)
        if (Plugin.colorBehavior.Value == ColorBehavior.Closeness)
        {
            if (percent > pbPercent)
            {
                foregroundText.color = Color.green;
            }
            else if (percent < pbPercent - 10)
            {
                foregroundText.color = Color.red;
            }
            else
            {
                foregroundText.color = Color.yellow;
            }
        }
    }

    /// <summary>
    /// Get the display percentage for a score, on a scale of 0 - 100
    /// </summary>
    private float GetPercent(int totalScore, int noteIndex) => Plugin.accType.Value switch
        {
            AccType.Increasing => (float)totalScore / maxScore * 100,
            AccType.Decreasing => (float)(totalScore + scoreLeftover[noteIndex]) / maxScore * 100,
            _ => (float)totalScore / scoreSums[noteIndex] * 100,
        };

    private float EaseValue(float current, float diff, float timeSum, float duration) =>
            Mathf.Max(diff * (-Mathf.Pow(2f, -10f * timeSum / duration) + 1f) * 1024f / 1023f + current, 0f);
}
