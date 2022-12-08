using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

internal class PercentCounter : MonoBehaviour
{
    public static Action<int, int> scoreChanged;

    private List<float[]> leveldata;
    private Text foregroundText;
    private Text shadowText;
    private float pb;
    private float maxScore = 0;

    public void Init(List<float[]> _leveldata, float _pb)
    {
        pb = _pb;
        leveldata = _leveldata;
        transform.localScale = Vector3.one;
    }

    void Start()
    {
        foregroundText = transform.Find("Score").GetComponent<Text>();
        shadowText = GetComponent<Text>();

        foregroundText.text = 100.FormatDecimals() + "%";
        shadowText.text = 100.FormatDecimals() + "%";

        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - 50f, rect.anchoredPosition.y - 25f);


        scoreChanged = (Action<int, int>)Delegate.Combine(scoreChanged, new Action<int, int>(OnScoreChanged));
    }

    void OnDestroy()
    {
        scoreChanged = (Action<int, int>)Delegate.Remove(scoreChanged, new Action<int, int>(OnScoreChanged));
    }

    internal void OnScoreChanged(int totalScore, int noteIndex)
    {
        maxScore += Utils.GetMaxScore(Plugin.accType.Value, leveldata[noteIndex][1], noteIndex);
        float percent = totalScore / maxScore * 100;
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
}
