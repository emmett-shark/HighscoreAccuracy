using System;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;

namespace HighscoreAccuracy;

public class OptionalTootTallySettings
{
    public static object AddNewPage(string pageName, string headerText, float elementSpacing, Color bgColor)
    {
        try
        {
            Type settingsPage = null;
            settingsPage = Type.GetType("TootTallySettings.TootTallySettingsManager, TootTallySettings");
            if (settingsPage == null)
            {
                Plugin.Log.LogDebug("TootTallySettings not found.");
                return null;
            }
            var addPageFn = settingsPage.GetMethod("AddNewPage", new Type[] { typeof(string), typeof(string), typeof(float), typeof(Color) });
            return addPageFn.Invoke(settingsPage, new object[] { pageName, headerText, elementSpacing, bgColor });
        }
        catch (Exception e)
        {
            Plugin.Log.LogError("Exception trying to get config page. Reporting TootTallySettings as not found.");
            Plugin.Log.LogError(e.Message);
            Plugin.Log.LogError(e.StackTrace);
            return null;
        }
    }

    public static void AddToggle(object page, string name, ConfigEntry<bool> config)
    {
        var addFn = page.GetType().GetMethod("AddToggle", new Type[] { typeof(string), typeof(ConfigEntry<bool>), typeof(UnityAction<bool>) });
        if (addFn != null) addFn.Invoke(page, new object[] { name, config, null });
    }

    public static void AddSlider(object page, string name, float min, float max, ConfigEntry<float> config, bool integerOnly)
    {
        var addFn = page.GetType().GetMethod("AddSlider", new Type[] { typeof(string), typeof(float), typeof(float), typeof(ConfigEntry<float>), typeof(bool) });
        if (addFn != null) addFn.Invoke(page, new object[] { name, min, max, config, integerOnly });
    }

    public static void AddDropdown(object page, string name, ConfigEntryBase config)
    {
        var addFn = page.GetType().GetMethod("AddDropdown", new Type[] { typeof(string), typeof(ConfigEntryBase) });
        if (addFn != null) addFn.Invoke(page, new object[] { name, config });
    }

    public static void AddLabel(object page, string label, int fontSize, TMPro.TextAlignmentOptions textAlignment)
    {
        var addFn = page.GetType().GetMethod("AddLabel", new Type[] { typeof(string), typeof(string), typeof(int), typeof(TMPro.FontStyles), typeof(TMPro.TextAlignmentOptions) });
        if (addFn != null) addFn.Invoke(page, new object[] { label, label, fontSize, TMPro.FontStyles.Normal, textAlignment });
    }

}
