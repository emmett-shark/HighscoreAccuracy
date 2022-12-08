using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SimpleJSON;
using System.Runtime.Serialization.Formatters.Binary;
using TrombLoader.Data;
using TrombLoader.Helpers;
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

        public static int GetMaxScore(AccType accType, List<float[]> levelData) =>
            levelData.Select((noteData, i) => accType == AccType.Real 
                ? GetRealMax(noteData[1], i) 
                : GetGameMax(noteData[1])).Sum();

        public static int GetMaxScore(AccType accType, float length, int noteIndex) =>
            accType == AccType.Real ? GetRealMax(length, noteIndex) : GetGameMax(length);

        public static int GetRealMax(float length, int noteIndex)
        {
            float champbonus = noteIndex > 23 ? 1.5f : 0;
            float realCoefficient = (Math.Min(noteIndex, 10) + champbonus) * 0.1f + 1f;
            return (int)Mathf.Floor(Mathf.Floor(length * 10f * 100f * realCoefficient) * 10f);
        }

        public static int GetGameMax(float length) =>
            (int)Mathf.Floor(Mathf.Floor(length * 10f * 100f * 1.3f) * 10f);

        public static List<float[]> GetLevelData(string trackRef)
        {
            string baseTmb = Application.streamingAssetsPath + "/leveldata/" + trackRef + ".tmb";
            return !File.Exists(baseTmb)
                ? GetCustomLevelData(trackRef)
                : GetSavedLevel(baseTmb).savedleveldata;
        }

        private static List<float[]> GetCustomLevelData(string trackRef)
        {
            if (!Globals.ChartFolders.TryGetValue(trackRef, out string customChartPath))
            {
                Plugin.Log.LogWarning($"Could not find {trackRef}");
                return new List<float[]>();
            }
            using (var streamReader = new StreamReader(customChartPath + "/song.tmb"))
            {
                string baseChartName = Application.streamingAssetsPath + "/leveldata/ballgame.tmb";
                SavedLevel savedLevel = GetSavedLevel(baseChartName);
                CustomSavedLevel customLevel = new CustomSavedLevel(savedLevel);
                string jsonString = streamReader.ReadToEnd();
                var jsonObject = JSON.Parse(jsonString);
                customLevel.Deserialize(jsonObject);
                return customLevel.savedleveldata;
            }
        }

        private static SavedLevel GetSavedLevel(string baseTmb)
        {
            using (FileStream fileStream = File.Open(baseTmb, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (SavedLevel)binaryFormatter.Deserialize(fileStream);
            }
        }

    }
}
