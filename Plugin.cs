using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using SimpleJSON;
using TrombLoader.Data;
using TrombLoader.Helpers;
using TrombSettings;
using UnityEngine;
using UnityEngine.UI;

namespace HighscoreAccuracy
{
    [HarmonyPatch]
    [BepInDependency("com.steven.trombone.accuracycounter", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.hypersonicsharkz.trombsettings")]
    [BepInPlugin("com.hypersonicsharkz.highscoreaccuracy", "Highscore Accuracy", "1.1.2")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;

        public enum AccType
        {
            BaseGame,
            Real
        }

        internal static ConfigEntry<AccType> accType;
        internal static ConfigEntry<bool> showLetterRank;
        internal static ConfigEntry<int> decimals;
        internal static ConfigEntry<bool> showAccIngame;
        internal static ConfigEntry<bool> showPBIngame;

        private void Awake()
        {
            Instance = this;

            accType = Config.Bind("General", "Acc Type", AccType.BaseGame);
            showLetterRank = Config.Bind("General", "Show Letters", true);
            decimals = Config.Bind("General", "Decimal Places", 2);
            showAccIngame = Config.Bind("General", "Show acc in track", true);
            showPBIngame = Config.Bind("General", "Show PB in track", true);

            TrombEntryList settings = TrombConfig.TrombSettings["Highscore Acc"];

            settings.Add(showLetterRank);
            settings.Add(accType);
            settings.Add(new StepSliderConfig(0, 4, 1, true, decimals));

            settings.Add(showAccIngame);
            settings.Add(showPBIngame);

            new Harmony("com.hypersonicsharkz.highscoreaccuracy").PatchAll();
        }

        [HarmonyPatch(typeof(LevelSelectController), "populateScores")]
        private static void Postfix(LevelSelectController __instance, int ___songindex, List<SingleTrackData> ___alltrackslist)
        {
            GetMaxScore(___alltrackslist[___songindex].trackref, out int gameMax, out int realMax);
            for (int k = 0; k < 5; k++)
            {
                try
                {
                    float max = accType.Value == AccType.Real ? realMax : gameMax;

                    if (float.TryParse(__instance.topscores[k].text, out float percent))
                    {
                        __instance.topscores[k].fontSize = 9;
                        percent /= max;

                        string letter = "";
                        if (showLetterRank.Value)
                        {
                            letter = Utils.ScoreLetter(float.Parse(__instance.topscores[k].text) / gameMax);
                        }

                        __instance.topscores[k].text = __instance.topscores[k].text + " " + (100 * percent).FormatDecimals() + "% " + letter;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

            }
        }

        [HarmonyPatch(typeof(PointSceneController), "doneWithCountUp")]
        private static void Postfix(PointSceneController __instance, int ___totalscore)
        {
            Instance.StartCoroutine(SetTextLate(__instance, ___totalscore));
        }

        static IEnumerator SetTextLate(PointSceneController __instance, int ___totalscore)
        {
            yield return new WaitForSeconds(0.2f);

            float percent;
            float prevPrecent;

            string trackRef = GlobalVariables.chosen_track_data.trackref;
            GetMaxScore(trackRef, out int gameMax, out int realMax);

            if (accType.Value == AccType.Real)
            {
                percent = ((float)___totalscore / (float)realMax) * 100;
                prevPrecent = (float.Parse(__instance.txt_prevhigh.text) / (float)realMax) * 100;
            }
            else
            {
                percent = (GlobalVariables.gameplay_scoreperc * 100);
                prevPrecent = (float.Parse(__instance.txt_prevhigh.text) / (float)gameMax) * 100;
            }

            __instance.scorecountertext.text += " " + percent.FormatDecimals() + "%";
            __instance.txt_prevhigh.text += " " + prevPrecent.FormatDecimals() + "%";
        }

        [HarmonyPatch(typeof(GameController), "Start")]
        private static void Postfix(GameController __instance, List<float[]> ___leveldata)
        {
            float pbValue = 0;
            if (showPBIngame.Value)
            {
                Utils.GetMaxScore(___leveldata, out int gameMax, out int realMax);
                int highscore = FindHighScore(GlobalVariables.chosen_track);

                if (highscore > 0)
                {
                    GameObject gameObject = GameObject.Find("ScoreShadow");
                    GameObject pb = UnityEngine.Object.Instantiate<GameObject>(gameObject, gameObject.transform.parent);

                    pb.transform.localScale = Vector3.one;

                    RectTransform rect = pb.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 50f);

                    var foregroundText = pb.transform.Find("Score").GetComponent<Text>();
                    var shadowText = pb.GetComponent<Text>();

                    float max = accType.Value == AccType.Real ? realMax : gameMax;
                    float percent = highscore / max * 100;

                    foregroundText.text = "PB: " + percent.FormatDecimals() + "%";
                    shadowText.text = "PB: " + percent.FormatDecimals() + "%";

                    pbValue = percent;
                }
            }
           
            if (showAccIngame.Value)
            {
                if (__instance.freeplay)
                {
                    return;
                }
                GameObject gameObject = GameObject.Find("ScoreShadow");
                PercentCounter counter = UnityEngine.Object.Instantiate<GameObject>(gameObject, gameObject.transform.parent).AddComponent<PercentCounter>();

                counter.Init(___leveldata, pbValue);
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
        }

        private static void GetMaxScore(string trackRef, out int gameMaxScore, out int realMaxScore)
        {
            string text = Application.streamingAssetsPath + "/leveldata/" + trackRef + ".tmb";
            List<float[]> levelData = new List<float[]>();

            if (!File.Exists(text))
            {
                //Must be custom level
                string customChartPath = Globals.GetCustomSongsPath() + trackRef + "/song.tmb";

                string baseChartName = Application.streamingAssetsPath + "/leveldata/ballgame.tmb";

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = File.Open(baseChartName, FileMode.Open);
                SavedLevel savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();

                CustomSavedLevel customLevel = new CustomSavedLevel(savedLevel);

                string jsonString = File.ReadAllText(customChartPath);
                var jsonObject = JSON.Parse(jsonString);
                customLevel.Deserialize(jsonObject);

                levelData = customLevel.savedleveldata;
            }
            else
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = File.Open(text, FileMode.Open);
                SavedLevel savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();

                levelData = savedLevel.savedleveldata;
            }
            Utils.GetMaxScore(levelData, out gameMaxScore, out realMaxScore);
        }
    }
}
