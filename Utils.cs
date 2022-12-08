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

        public static string ScoreLetter(float num) =>
            num < 1f ? (num < 0.8f ? (num < 0.6f ? (num < 0.4f ? (num < 0.2f ? "F" : "D") : "C") : "B") : "A") : "S";

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

            gameMax = GetGameMax(noteData[1]);
        }

        public static int GetGameMax(float length) =>
            (int)Mathf.Floor(Mathf.Floor(length * 10f * 100f * 1.3f) * 10f);
    }
}
