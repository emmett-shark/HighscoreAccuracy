using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TootTallyGameModifiers;
using TootTallyCore.Utils.TootTallyGlobals;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

[HarmonyPatch]
[BepInDependency("ch.offbeatwit.baboonapi.plugin")]
[BepInDependency("TrombLoader")]
[BepInDependency("TootTallyCore")]
[BepInDependency("TootTallyGameModifiers")]
[BepInDependency("TootTallySettings", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance;
    internal static ManualLogSource Log;

    public const string FILE_HIGHSCORES_NAME = "HighscoreAccuracyScores.json";

    internal static ConfigEntry<AccType> accType;
    internal static ConfigEntry<ColorBehavior> colorBehavior;
    internal static ConfigEntry<float> decimals;
    internal static ConfigEntry<bool> showAccIngame;
    internal static ConfigEntry<bool> showLetterIngame;
    internal static ConfigEntry<bool> showPBIngame;
    internal static ConfigEntry<bool> animateCounter;
    internal static ConfigEntry<ModSpecificHighscoreMode> useModSpecificHighscores;

    public enum ModSpecificHighscoreMode
    {
        Never,
        ExactMatchFound,
        TrackFound,
        Always
    }

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        accType = Config.Bind("General", "Acc Type", AccType.Real);
        colorBehavior = Config.Bind("General", "Color Behavior", ColorBehavior.Hybrid);
        decimals = Config.Bind("General", "Decimal Places", 2f);
        showAccIngame = Config.Bind("General", "Show acc in track", true);
        showLetterIngame = Config.Bind("General", "Show letter rank in track", false);
        showPBIngame = Config.Bind("General", "Show PB in track", true);
        animateCounter = Config.Bind("General", "Gradually increase the accuracy.", false);
        useModSpecificHighscores = Config.Bind("General", "Use Modifier and Game Speed Specific High Scores", ModSpecificHighscoreMode.Never);

        object ttSettings = OptionalTootTallySettings.AddNewPage("Highscore Accuracy", "Highscore Accuracy", 40, new Color(.1f, .1f, .1f, .1f));
        if (ttSettings != null)
        {
            OptionalTootTallySettings.AddLabel(ttSettings, "Accuracy Type", 24, TMPro.TextAlignmentOptions.BottomLeft);
            OptionalTootTallySettings.AddDropdown(ttSettings, "Accuracy Type", accType);
            OptionalTootTallySettings.AddLabel(ttSettings, @"- Base Game: uses the internal calculations for the letter where >100% = S.

- Real: calculates the actual maximum score for a track.

- Decreasing: Uses real accuracy, but your % will always decrease or stay the same.
For example, ignoring multipliers, completely missing the first note of a 100 note song will give you 99%.

- Increasing: Uses real accuracy, but your % will always increase or stay the same.
For example, ignoring multipliers, perfectly hitting the first note of a 100 note song will give you 1%."
                , 24, TMPro.TextAlignmentOptions.TopLeft);

            OptionalTootTallySettings.AddLabel(ttSettings, "Color Behavior", 24, TMPro.TextAlignmentOptions.BottomLeft);
            OptionalTootTallySettings.AddDropdown(ttSettings, "Color Behavior", colorBehavior);
            OptionalTootTallySettings.AddLabel(ttSettings, @"- Closeness: Color depends on how close you are to your PB (old Highscore Accuracy behavior)
    - Green: Above PB
    - Yellow: Up to 10% below PB
    - Red: More than 10% below PB

- PB Possibility: Color depends on whether a PB is possible or not
    - Dark green: Above PB and cannot avoid setting a new PB
    - Green: Above PB
    - Yellow: PB is still possible with the remaining notes
    - Red: PB is impossible

- Hybrid: A combination of the above two (adding orange if below 10% of your PB, but a PB is still possible)
    - Dark green: Above PB and cannot avoid setting a new PB
    - Green: Above PB
    - Yellow: PB is still possible with the remaining notes
    - Orange: PB is still possible with the remaining notes, but you're more than 10% below PB
    - Red: PB is impossible"
                , 24, TMPro.TextAlignmentOptions.TopLeft);

            OptionalTootTallySettings.AddSlider(ttSettings, "Decimal Places", 0, 4, decimals, true);
            OptionalTootTallySettings.AddToggle(ttSettings, "Show Acc Ingame", showAccIngame);
            OptionalTootTallySettings.AddToggle(ttSettings, "Show Letter Rank Ingame", showLetterIngame);
            OptionalTootTallySettings.AddToggle(ttSettings, "Show PB Ingame", showPBIngame);
            OptionalTootTallySettings.AddToggle(ttSettings, "Animate Counter", animateCounter);
            OptionalTootTallySettings.AddLabel(ttSettings, "Use Game Speed and Modifier Specific High Scores", 24, TMPro.TextAlignmentOptions.BottomLeft);
            OptionalTootTallySettings.AddDropdown(ttSettings, "Use Modifier and Game Speed Specific High Scores", useModSpecificHighscores);
            OptionalTootTallySettings.AddLabel(ttSettings, @"Modifier and game speed specific high scores set without this mod installed are not saved and
cannot be shown. While this setting is not set to Never, an asterisk will be shown next to the high
score if the high score was set without this mod installed.
            
- Never: High scores are not specific to modifiers or game speed. The highest score is shown
regardless of which modifiers or game speed were used.
            
- ExactMatchFound: High scores are specific to modifiers and game speed. If a high score has not
been set with the exact same modifiers and game speed, the highest score across all modifiers and
game speeds is shown instead.

- TrackFound: High scores are specific to modifiers and game speed. If the song has never been
played with this mod installed, the highest score across all modifiers and game speeds set without
this mod is shown instead. If the song has been played with this mod installed, but not with the
exact same modifiers and game speed, no high score is shown.

- Always: High scores are specific to modifiers and game speed.
A high score is only shown if the song has been played with the exact same modifiers and game speed
with this mod installed. Otherwise, no high score is shown."
                , 24, TMPro.TextAlignmentOptions.TopLeft);
            OptionalTootTallySettings.AddLabel(ttSettings, @"If the dropdowns aren't showing up, update TootTally.
You can still update accuracy type through the config file, as usual."
                , 24, TMPro.TextAlignmentOptions.TopLeft);
        }

        HighscoreRegistry.LoadHighscoresFromFile();

        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    [HarmonyPatch(typeof(LevelSelectController), "populateScores")]
    private static void Postfix(LevelSelectController __instance, int ___songindex, List<SingleTrackData> ___alltrackslist)
    {
        string trackRef = ___alltrackslist[___songindex].trackref;
        if (Utils.SkipHighscore(trackRef)) return;
        try
        {
            var levelData = Utils.GetLevelData(trackRef);
            int gameMax = Utils.GetMaxScore(AccType.BaseGame, levelData);
            int max = Utils.GetMaxScore(accType.Value, levelData);
            for (int k = 0; k < 5; k++)
            {
                if (float.TryParse(__instance.topscores[k].text, out float topScore))
                {
                    __instance.topscores[k].fontSize = 19;
                    string letter = Utils.ScoreLetter(topScore / gameMax);
                    float percent = topScore / max;
                    __instance.topscores[k].text = __instance.topscores[k].text + " " + (100 * percent).FormatDecimals() + "% " + letter;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    [HarmonyPatch(typeof(GameController), nameof(GameController.tallyScore))]
    private static void Prefix(ref int ___previous_high_score)
    {
        ___previous_high_score = int.MaxValue;
    }

    [HarmonyPatch(typeof(PointSceneController), "doCoins")]
    [HarmonyPriority(Priority.High)]
    private static void Postfix(PointSceneController __instance)
    {
        string trackRef = GlobalVariables.chosen_track_data.trackref;
        if (Utils.SkipHighscore(trackRef)) return;
        List<float[]> levelData = Utils.GetLevelData(trackRef);
        int max = Utils.GetMaxScore(accType.Value, levelData);
        float percent = (float)GlobalVariables.gameplay_scoretotal / max * 100;
        float prevPrecent = float.Parse(__instance.txt_prevhigh.text) / max * 100;

        __instance.txt_score.text += " " + percent.FormatDecimals() + "%";
        __instance.txt_score.horizontalOverflow = HorizontalWrapMode.Overflow;
        __instance.txt_prevhigh.text += " " + prevPrecent.FormatDecimals() + "%";
        __instance.txt_prevhigh.horizontalOverflow = HorizontalWrapMode.Overflow;
    }

    [HarmonyPatch(typeof(GameController), "Start")]
    private static void Postfix(GameController __instance, List<float[]> ___leveldata)
    {
        if (__instance.freeplay) return;
        float pbPercent = 0;
        int pbScore = 0;
        GameObject gameObject = GameObject.Find("ScoreShadow");
        var scoreSums = Utils.GetScoreSums(accType.Value, ___leveldata);
        int maxScore = scoreSums[___leveldata.Count - 1];
        if (showPBIngame.Value)
        {
            var highscoreResult = HighscoreRegistry.GetHighscore(
                GlobalVariables.chosen_track,
                GameModifierManager.GetModifiersString(),
                TootTallyGlobalVariables.gameSpeedMultiplier
            );
            if (highscoreResult.Score > 0)
            {
                GameObject pb = Instantiate(gameObject, gameObject.transform.parent);

                pb.transform.localScale = Vector3.one;

                RectTransform rect = pb.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 50f);

                var foregroundText = pb.transform.Find("Score").GetComponent<Text>();
                var shadowText = pb.GetComponent<Text>();

                //Log.LogDebug($"{GlobalVariables.chosen_track} max score: {maxScore}");
                float percent = (float)highscoreResult.Score / maxScore * 100;

                foregroundText.text = "PB: " + percent.FormatDecimals() + "%" + (highscoreResult.IsGlobal ? "*" : "");
                shadowText.text = "PB: " + percent.FormatDecimals() + "%" + (highscoreResult.IsGlobal ? "*" : "");

                pbPercent = percent;
                pbScore = highscoreResult.Score;
            }
        }

        var scoreLeftover = new int[scoreSums.Length];
        for (int i = 0; i < scoreSums.Length; i++)
        {
            scoreLeftover[i] = maxScore - scoreSums[i];
        }

        if (showAccIngame.Value)
        {
            PercentCounter percentCounter = Instantiate(gameObject, gameObject.transform.parent).AddComponent<PercentCounter>();
            percentCounter.Init(maxScore, scoreLeftover, scoreSums, pbScore, pbPercent);
        }

        if (showLetterIngame.Value)
        {
            ScoreCounter scoreCounter = Instantiate(gameObject, gameObject.transform.parent).AddComponent<ScoreCounter>();
            scoreCounter.Init(Utils.GetScoreSums(AccType.BaseGame, ___leveldata));
        }
    }

    [HarmonyPatch(typeof(GameController), "getScoreAverage")]
    private static void Postfix(int ___totalscore, int ___currentnoteindex)
    {
        if (showAccIngame.Value)
        {
            PercentCounter.scoreChanged(___totalscore, ___currentnoteindex);
        }
        if (showLetterIngame.Value)
        {
            ScoreCounter.scoreChanged(___totalscore, ___currentnoteindex);
        }
    }

    [HarmonyPatch(typeof(SaverLoader), "checkForUpdatedScore")]
    private static void Prefix(SingleTrackData played_track, int newscore, string newletterscore)
    {
        HighscoreRegistry.CheckNewScore(
            played_track.trackref,
            newscore,
            GameModifierManager.GetModifiersString(),
            TootTallyGlobalVariables.gameSpeedMultiplier
        );
    }
}
