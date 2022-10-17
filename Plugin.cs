using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
    [BepInPlugin("com.hypersonicsharkz.highscoreaccuracy", "Highscore Accuracy", "1.0.0")]
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
        private static void Postfix(LevelSelectController __instance, int ___songindex)
        {
            GetMaxScore(___songindex, out int gameMax, out int realMax);
            for (int k = 0; k < 5; k++)
            {
                try
                {
                    __instance.topscores[k].fontSize = 9;

                    float percent = 0;
                    if (accType.Value == AccType.Real)
                    {
                        percent = float.Parse(__instance.topscores[k].text) / realMax;
                    }
                    else
                    {
                        percent = float.Parse(__instance.topscores[k].text) / gameMax;
                    }

                    string letter = "";
                    if (showLetterRank.Value)
                    {
                        letter = Utils.ScoreLetter(float.Parse(__instance.topscores[k].text) / gameMax);
                    }

                    __instance.topscores[k].text = __instance.topscores[k].text + " " + (100 * percent).FormatDecimals() + "% " + letter;
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

            GetMaxScore(GlobalVariables.levelselect_index, out int gameMax, out int realMax);

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
                int highscore = int.Parse(GlobalVariables.localsave.data_trackscores[FindTrackIndex(GlobalVariables.chosen_track)][2]);

                if (highscore > 0)
                {
                    GameObject gameObject = GameObject.Find("ScoreShadow");
                    GameObject pb = UnityEngine.Object.Instantiate<GameObject>(gameObject, gameObject.transform.parent);

                    pb.transform.localScale = Vector3.one;

                    RectTransform rect = pb.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - 50f);

                    var foregroundText = pb.transform.Find("Score").GetComponent<Text>();
                    var shadowText = pb.GetComponent<Text>();

                    float percent = 0;

                    if (accType.Value == AccType.Real)
                    {
                        percent = ((float)highscore / (float)realMax) * 100;
                    }
                    else if (accType.Value == AccType.BaseGame)
                    {
                        percent = ((float)highscore / (float)realMax) * 100;
                    }

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

        private static int FindTrackIndex(string trackref)
        {
            int num = 0;
            for (int i = 0; i < 100; i++)
            {
                if (GlobalVariables.localsave.data_trackscores[i][0] == trackref)
                {
                    num = i;
                    break;
                }
            }
            return num;
        }

        [HarmonyPatch(typeof(GameController), "getScoreAverage")]
        private static void Postfix(int ___totalscore, int ___currentnoteindex)
        {
            if (showAccIngame.Value)
            {
                PercentCounter.scoreChanged(___totalscore, ___currentnoteindex);
            }
        }



        /*
        [HarmonyPatch(typeof(GameController), "Update")]
        private static void Postfix(Text ___ui_score, bool ___noteactive, float ___currentnoteend, float ___currentnotestart, float ___currentnotepshift, RectTransform ___pointerrect, float ___currentnotestarty, RectTransform ___noteholderr, float ___zeroxpos)
        {
            float num10 = ___noteholderr.anchoredPosition3D.x - ___zeroxpos;

            float num11 = (___currentnoteend - num10) / (___currentnoteend - ___currentnotestart);
            num11 = Mathf.Abs(1f - num11);
            float num12 = easeInOutVal(num11, 0f, ___currentnotepshift, 1f);
            float f = ___pointerrect.anchoredPosition.y - (___currentnotestarty + num12);
            float num13 = 100f - Mathf.Abs(f);

            Debug.Log(string.Concat(new object[]
            {
                "11: ",
                num11,
                " / 12: ",
                num12,
                " / f: ",
                f,
                " / 13: ",
                num13,
                " / 10?: ",
                num10
            }));

            ___ui_score.text = f.ToString();

        }


        private static float easeInOutVal(float t, float b, float c, float d)
        {
            t /= d / 2f;
            if (t < 1f)
            {
                return c / 2f * t * t + b;
            }
            t -= 1f;
            return -c / 2f * (t * (t - 2f) - 1f) + b;
        }*/

        private static void GetMaxScore(int levelId, out int gameMaxScore, out int realMaxScore)
        {
            string text = Application.streamingAssetsPath + "/leveldata/" + GlobalVariables.data_trackrefs[levelId] + ".tmb";
            List<float[]> levelData = new List<float[]>();

            if (!File.Exists(text))
            {
                //Must be custom level
                string trackReference = GlobalVariables.data_trackrefs[levelId];
                string customChartPath = Globals.GetCustomSongsPath() + trackReference + "/song.tmb";

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
