using System;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;

namespace HighscoreAccuracy;

public class OptionalTootTally
{
    public static Type TootTallyGlobalVariables = GetType("TootTallyCore.Utils.TootTallyGlobals.TootTallyGlobalVariables", "TootTallyCore");

    private static Type GetType(string fullName, string assembly)
    {
        try
        {
            return Type.GetType($"{fullName}, {assembly}");
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"Exception trying to get {fullName}. Reporting {assembly} as not found.");
            Plugin.Log.LogError(e.Message);
            Plugin.Log.LogError(e.StackTrace);
            return null;
        }
    }

    public static string GetModifiersString()
    {
        Type gameModifierManager = GetType("TootTallyGameModifiers.GameModifierManager", "TootTallyGameModifiers");
        if (gameModifierManager == null) return "";
        var getModifiersFn = gameModifierManager.GetMethod("GetModifiersString");
        return getModifiersFn == null ? "" : (string)getModifiersFn.Invoke(gameModifierManager, []);
    }

    public static float GameSpeedMultiplier()
    {
        if (TootTallyGlobalVariables == null) return GlobalVariables.turbomode ? 2f : 1f;
        var speedField = TootTallyGlobalVariables.GetField("gameSpeedMultiplier", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        return speedField == null ? 1f : (float)speedField.GetValue(speedField);
    }

    public static object AddNewPage(string pageName, string headerText, float elementSpacing, Color bgColor)
    {
        Type settingsManager = GetType("TootTallySettings.TootTallySettingsManager", "TootTallySettings");
        if (settingsManager == null)
        {
            Plugin.Log.LogDebug($"TootTallySettings not found");
            return null;
        }
        var addPageFn = settingsManager.GetMethod("AddNewPage", [typeof(string), typeof(string), typeof(float), typeof(Color)]);
        return addPageFn == null ? null : addPageFn.Invoke(settingsManager, [pageName, headerText, elementSpacing, bgColor]);
    }

    public static void AddToggle(object page, string name, ConfigEntry<bool> config)
    {
        var addFn = page.GetType().GetMethod("AddToggle", [typeof(string), typeof(ConfigEntry<bool>), typeof(UnityAction<bool>)]);
        if (addFn != null) addFn.Invoke(page, [name, config, null]);
    }

    public static void AddSlider(object page, string name, float min, float max, ConfigEntry<float> config, bool integerOnly)
    {
        var addFn = page.GetType().GetMethod("AddSlider", [typeof(string), typeof(float), typeof(float), typeof(ConfigEntry<float>), typeof(bool)]);
        if (addFn != null) addFn.Invoke(page, [name, min, max, config, integerOnly]);
    }

    public static void AddDropdown(object page, string name, ConfigEntryBase config)
    {
        var addFn = page.GetType().GetMethod("AddDropdown", [typeof(string), typeof(ConfigEntryBase)]);
        if (addFn != null) addFn.Invoke(page, [name, config]);
    }

    public static void AddLabel(object page, string label, int fontSize, TMPro.TextAlignmentOptions textAlignment)
    {
        var addFn = page.GetType().GetMethod("AddLabel", [typeof(string), typeof(string), typeof(int), typeof(TMPro.FontStyles), typeof(TMPro.TextAlignmentOptions)]);
        if (addFn != null) addFn.Invoke(page, [label, label, fontSize, TMPro.FontStyles.Normal, textAlignment]);
    }
}
