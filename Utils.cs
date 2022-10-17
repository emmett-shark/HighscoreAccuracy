using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HighscoreAccuracy
{
    public static class Utils
    {
        public static string FormatDecimals<T>(this T _number) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            return String.Format(new NumberFormatInfo() { NumberDecimalDigits = Plugin.decimals.Value }, "{0:F}", _number);
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

            float num1 = Mathf.Floor(noteData[1] * 10f);

            float num5 = Mathf.Floor(num1 * 100f * (((float)Math.Min(noteIndex, 10) + champbonus) * 0.1f + 1f)) * 10f;
            realMax = Mathf.FloorToInt(num5);

            float f = Mathf.Floor(num1 * 100f * 1.3f) * 10f;
            gameMax = Mathf.FloorToInt(f);
        }
    }
}
