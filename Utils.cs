using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using TrombLoader.CustomTracks;
using TrombLoader.Helpers;
using UnityEngine;

namespace HighscoreAccuracy;

public static class Utils
{
    public static string FormatDecimals<T>(this T _number, int digits) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        return string.Format(new NumberFormatInfo() { NumberDecimalDigits = (digits) }, "{0:F}", _number);
    }

    public static string ScoreLetter(float num) =>
        num < 1f ? (num < 0.8f ? (num < 0.6f ? (num < 0.4f ? (num < 0.2f ? "F" : "D") : "C") : "B") : "A") : "S";

    public static int GetMaxScore(AccType accType, List<float[]> levelData) => GetMaxScores(accType, levelData).Sum();

    public static IEnumerable<int> GetMaxScores(AccType accType, List<float[]> levelData) =>
        levelData.Select((noteData, i) => GetMaxScore(accType, noteData[1], i));

    public static int GetMaxScore(AccType accType, float length, int noteIndex) =>
        accType switch
        {
            AccType.BaseGame => GetGameMax(length),
            _ => GetRealMax(length, noteIndex),
        };

    public static int GetRealMax(float length, int noteIndex)
    {
        double champbonus = noteIndex > 23 ? 1.5 : 0;
        double realCoefficient = (Math.Min(noteIndex, 10) + champbonus) * 0.100000001490116 + 1.0;
        length = GetLength(length);
        return (int)(Mathf.Floor((float)((double)length * 10.0 * 100 * realCoefficient)) * 10f);
    }

    public static int GetGameMax(float length) => (int)Mathf.Floor(Mathf.Floor(GetLength(length) * 10f * 100f * 1.315f) * 10f);

    public static float GetLength(float length) => length <= 0f ? 0.015f : length;

    public static List<float[]> GetLevelData(string trackRef) =>
        TrackLookup.lookup(trackRef).LoadChart().savedleveldata;

    public static bool SkipHighscore(string trackRef)
    {
        return TrackLookup.lookup(trackRef) is CustomTrack ct
            && new FileInfo(Path.Combine(ct.folderPath, Globals.defaultChartName)).Length > 2_000_000;
    }
}
