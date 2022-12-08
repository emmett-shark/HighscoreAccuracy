using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy;

[HarmonyPatch]
[BepInDependency("TrombSettings", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("TrombLoader")]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static Plugin Instance;
    internal static ManualLogSource Log;

    internal static ConfigEntry<AccType> accType;
    internal static ConfigEntry<bool> showLetterRank;
    internal static ConfigEntry<int> decimals;
    internal static ConfigEntry<bool> showAccIngame;
    internal static ConfigEntry<bool> showScoreIngame;
    internal static ConfigEntry<bool> showPBIngame;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        accType = Config.Bind("General", "Acc Type", AccType.BaseGame);
        showLetterRank = Config.Bind("General", "Show Letters", true);
        decimals = Config.Bind("General", "Decimal Places", 2);
        showAccIngame = Config.Bind("General", "Show acc in track", true);
        showScoreIngame = Config.Bind("General", "Show score in track", false);
        showPBIngame = Config.Bind("General", "Show PB in track", true);

        object settings = OptionalTrombSettings.GetConfigPage("Highscore Acc");
        if (settings != null)
        {
            OptionalTrombSettings.Add(settings, showLetterRank);
            OptionalTrombSettings.Add(settings, accType);
            OptionalTrombSettings.AddSlider(settings, 0, 4, 1, true, decimals);

            OptionalTrombSettings.Add(settings, showAccIngame);
            OptionalTrombSettings.Add(settings, showScoreIngame);
            OptionalTrombSettings.Add(settings, showPBIngame);
        }

        new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
    }

    [HarmonyPatch(typeof(LevelSelectController), "populateScores")]
    private static void Postfix(LevelSelectController __instance, int ___songindex, List<SingleTrackData> ___alltrackslist)
    {
        var levelData = Utils.GetLevelData(___alltrackslist[___songindex].trackref);
        for (int k = 0; k < 5; k++)
        {
            try
            {
                if (float.TryParse(__instance.topscores[k].text, out float topScore))
                {
                    __instance.topscores[k].fontSize = 9;

                    string letter = showLetterRank.Value ? Utils.ScoreLetter(topScore / Utils.GetMaxScore(AccType.BaseGame, levelData)) : "";

                    float percent = topScore / Utils.GetMaxScore(accType.Value, levelData);
                    __instance.topscores[k].text = __instance.topscores[k].text + " " + (100 * percent).FormatDecimals() + "% " + letter;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

        }
    }

    [HarmonyPatch(typeof(PointSceneController), "doCoins")]
    private static void Postfix(PointSceneController __instance)
    {
        string trackRef = GlobalVariables.chosen_track_data.trackref;
        List<float[]> levelData = Utils.GetLevelData(trackRef);
        int max = Utils.GetMaxScore(accType.Value, levelData);
        float percent = (float)GlobalVariables.gameplay_scoretotal / max * 100;
        float prevPrecent = float.Parse(__instance.txt_prevhigh.text) / max * 100;

        __instance.scorecountertext.text += " " + percent.FormatDecimals() + "%";
        __instance.txt_prevhigh.text += " " + prevPrecent.FormatDecimals() + "%";
    }

    [HarmonyPatch(typeof(GameController), "Start")]
    private static void Postfix(GameController __instance, List<float[]> ___leveldata)
    {
        if (__instance.freeplay) return;
        float pbValue = 0;
        GameObject gameObject = GameObject.Find("ScoreShadow");
        if (showPBIngame.Value)
        {
            int highscore = FindHighScore(GlobalVariables.chosen_track);
            if (highscore > 0)
            {
                GameObject pb = Instantiate(gameObject, gameObject.transform.parent);

                pb.transform.localScale = Vector3.one;

                RectTransform rect = pb.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 50f);

                var foregroundText = pb.transform.Find("Score").GetComponent<Text>();
                var shadowText = pb.GetComponent<Text>();

                float max = Utils.GetMaxScore(accType.Value, ___leveldata);
                float percent = highscore / max * 100;

                foregroundText.text = "PB: " + percent.FormatDecimals() + "%";
                shadowText.text = "PB: " + percent.FormatDecimals() + "%";

                pbValue = percent;
            }
        }

        if (showAccIngame.Value)
        {
            PercentCounter percentCounter = Instantiate(gameObject, gameObject.transform.parent).AddComponent<PercentCounter>();
            percentCounter.Init(___leveldata, pbValue);
        }

        if (showScoreIngame.Value)
        {
            ScoreCounter scoreCounter = Instantiate(gameObject, gameObject.transform.parent).AddComponent<ScoreCounter>();
            scoreCounter.Init(___leveldata);
        }
    }

    private static int FindHighScore(string trackRef)
    {
        string[] trackScores = GlobalVariables.localsave.data_trackscores
            .Where(i => i != null && i[0] == trackRef)
            .FirstOrDefault();
        return trackScores == null ? 0 : int.Parse(trackScores[2]);
    }

    [HarmonyPatch(typeof(GameController), "getScoreAverage")]
    private static void Postfix(int ___totalscore, int ___currentnoteindex)
    {
        if (showAccIngame.Value)
        {
            PercentCounter.scoreChanged(___totalscore, ___currentnoteindex);
        }
        if (showScoreIngame.Value)
        {
            ScoreCounter.scoreChanged(___totalscore, ___currentnoteindex);
        }
    }
}
