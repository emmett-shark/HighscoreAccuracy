using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace HighscoreAccuracy
{
    public static class Utils
    {
        public static string FormatDecimals<T>(this T _number) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            return string.Format(new NumberFormatInfo() { NumberDecimalDigits = Plugin.decimals.Value }, "{0:F}", _number);
        }

        public static string ScoreLetter(float percentage)
        {
            string text = "F";
            if (percentage > 1f)
            {
                text = "S";
            }
            else if (percentage > 0.8f)
            {
                text = "A";
            }
            else if (percentage > 0.6f)
            {
                text = "B";
            }
            else if (percentage > 0.4f)
            {
                text = "C";
            }
            else if (percentage > 0.2f)
            {
                text = "D";
            }
            return text;
        }

        public static void GetMaxScore(List<float[]> levelData, out int gameMaxScore, out int realMaxScore)
        {
            realMaxScore = 0;
            gameMaxScore = 0;

            for (int i = 0; i < levelData.Count; i++)
            {
                GetMaxScoreFromNote(levelData[i], i, out int gameNoteMax, out int realNoteMax);
                realMaxScore += realNoteMax;
                gameMaxScore += gameNoteMax;
            }
        }

        public static void GetMaxScoreFromNote(float[] noteData, int noteIndex, out int gameMax, out int realMax)
        {
            float champbonus = 0;
            if (noteIndex > 23)
                champbonus = 1.5f;

            float realCoefficient = (Math.Min(noteIndex, 10) + champbonus) * 0.1f + 1f;

            realMax = (int)Mathf.Floor(Mathf.Floor(noteData[1] * 10f * 100f * realCoefficient) * 10f);

            gameMax = (int)Mathf.Floor(Mathf.Floor(noteData[1] * 10f * 100f * 1.3f) * 10f);
        }
    }
}
