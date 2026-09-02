using System;
using System.Collections.Generic;
using System.Linq;
using BaboonAPI.Hooks.Tracks;
using static HighscoreAccuracy.Enums.ModSpecificHighscoreMode;

namespace HighscoreAccuracy;

public static class HighscoreRegistry
{
    // Dictionary structure: trackref -> mods -> gamespeed -> highscore
    private static Dictionary<string, Dictionary<string, Dictionary<int, int>>> _trackRefToHighscoreDict = new Dictionary<string, Dictionary<string, Dictionary<int, int>>>();

    public static void LoadHighscoresFromFile()
    {
        _trackRefToHighscoreDict = FileHelper.LoadFromTootTallyAppData<Dictionary<string, Dictionary<string, Dictionary<int, int>>>>(Plugin.FILE_HIGHSCORES_NAME);
        if (_trackRefToHighscoreDict == default)
        {
            Plugin.Log.LogInfo($"Couldn't find highscores file, creating new one!");
            _trackRefToHighscoreDict = new Dictionary<string, Dictionary<string, Dictionary<int, int>>>();
            FileHelper.SaveToTootTallyAppData(Plugin.FILE_HIGHSCORES_NAME, _trackRefToHighscoreDict, true);
        }
    }

    public static void CheckNewScore(string trackref, int score, string mods, float gamespeed)
    {
        if (gamespeed == 0) return;
        mods = SortModsString(mods);
        int gamespeedInt = (int)Math.Round(gamespeed * 100);

        if (!_trackRefToHighscoreDict.TryGetValue(trackref, out var trackScores))
        {
            trackScores = new Dictionary<string, Dictionary<int, int>>();
            _trackRefToHighscoreDict[trackref] = trackScores;
        }

        if (!trackScores.TryGetValue(mods, out var modScores))
        {
            modScores = new Dictionary<int, int>();
            trackScores[mods] = modScores;
        }

        if (!modScores.TryGetValue(gamespeedInt, out var existingScore) || score > existingScore)
        {
            modScores[gamespeedInt] = score;
            FileHelper.SaveToTootTallyAppData(Plugin.FILE_HIGHSCORES_NAME, _trackRefToHighscoreDict);
        }
    }

    public static HighscoreResult GetHighscore(string trackref, string mods, float gamespeed)
    {
        if (Plugin.useModSpecificHighscores.Value == Global)
            return new HighscoreResult(GetGlobalHighscore(), false);

        mods = SortModsString(mods);
        int gamespeedInt = (int)Math.Round(gamespeed * 100);

        if (_trackRefToHighscoreDict.TryGetValue(trackref, out var trackScores) &&
            trackScores.TryGetValue(mods, out var modScores) &&
            modScores.TryGetValue(gamespeedInt, out var highscore))
            return new HighscoreResult(highscore, false);

        if (Plugin.useModSpecificHighscores.Value == Hybrid)
            return new HighscoreResult(GetGlobalHighscore(), true);

        return new HighscoreResult(0, false);
    }

    private static int GetGlobalHighscore()
    {
        var score = TrackLookup.lookupScore(GlobalVariables.chosen_track);
        return score != null ? score.Value.highScores.FirstOrDefault() : 0;
    }

    public static string SortModsString(string mods)
    {
        return string.Join(",", mods.Split(',').Select(m => m.Trim()).OrderBy(m => m).ToArray());
    }

    public readonly struct HighscoreResult
    {
        public int Score { get; }
        public bool ShowAsterisk { get; }

        public HighscoreResult(int score, bool showAsterisk)
        {
            Score = score;
            ShowAsterisk = showAsterisk;
        }
    }
}