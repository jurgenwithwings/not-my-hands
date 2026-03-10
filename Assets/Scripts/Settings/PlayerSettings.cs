using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum OnScreenControllerUI {
    Xbox,
    PlayStation,
    Switch,
}

public enum SettingName {
    // Gameplay
    AbbreviateDamageNumbers = 0,
    DamageNumberScale = 17,
    ControllerUI = 1,
    
    // Graphics
    Resolution = 2,
    DisplayMode = 3,
    FramerateLimit = 4,
    VSync = 5,
    QualityPreset = 6,
    
    // Input
    MouseSensitivity = 7,
    ControllerSensitivity = 8,
    IsInvertedMouseY = 9,
    InnerDeadzone = 10,
    OuterDeadzone = 11,
    AimAssistStrength = 12,
    
    //Audio
    MasterVolume = 13,
    MusicVolume = 14,
    AmbientVolume = 15,
    EffectsVolume = 16,
    
}

public static class PlayerSettings
{
    private static Dictionary<string, FieldInfo> settings = new();
    private static string SettingsFilePath => Application.dataPath + "/UserSettings.json";
    
    // ~~ SETTINGS ~~
    // Gameplay
    public static bool AbbreviateDamageNumbers = false;
    public static float DamageNumberScale = 1f;
    public static OnScreenControllerUI ControllerUI = OnScreenControllerUI.Xbox;
    
    // Graphics
    public static int Resolution;
    public static int DisplayMode;
    public static int FramerateLimit;
    public static bool VSync;
    public static int QualityPreset;
    
    // Input
    public static float MouseSensitivity = 30f;
    public static float ControllerSensitivity = 150f;
    public static bool IsInvertedMouseY = false;
    public static float InnerDeadzone = 0.125f;
    public static float OuterDeadzone = 0.125f;
    public static float AimAssistStrength = 1f;

    // Audio
    public static float MasterVolume = 0.35f;
    public static float MusicVolume = 1f;
    public static float AmbientVolume = 1f;
    public static float EffectsVolume = 1f;


    private static bool loaded;
    public static void Load() {
        if (loaded) return;
        
        GetFields();
        LoadAllSettings();
        
        loaded = true;
    }

    private static void GetFields() {
        settings.Clear();
        FieldInfo[] fields = typeof(PlayerSettings).GetFields();
        foreach (FieldInfo f in fields) {
            Type type = f.FieldType;
            if ((type == typeof(float) || type == typeof(bool) || type == typeof(int) || type.IsEnum) && f.IsPublic) {
                settings.Add(f.Name, f);
            }
        }
    }
    
    private static void LoadAllSettings() {
        if (!File.Exists(SettingsFilePath)) {
            Debug.Log("Could not find settings file. Creating new one with default values.");
            SaveAllSettings();
        }

        JObject json = JObject.Parse(File.ReadAllText(SettingsFilePath));
        for (int i = 0; i < settings.Count; i++) {
            string key = settings.Keys.ElementAt(i);
            FieldInfo settingField = settings[key];

            if (json.TryGetValue(key, out JToken value)) {
                SetValueFromJson(settingField, value);
            }
            else {
                Debug.LogWarning($"Setting {key} not found in settings file. Using default value.");
            }

            settings[key] = settingField;
        }
    }

    private static void SetValueFromJson(FieldInfo field, JToken value) {
        object convertedValue;

        // Handle enums explicitly
        if (field.FieldType.IsEnum) {
            if (value.Type == JTokenType.String) {
                convertedValue = Enum.Parse(field.FieldType, value.ToString());
            }
            else {
                convertedValue = Enum.ToObject(field.FieldType, value.ToObject<int>());
            }
        }
        else {
            convertedValue = value.ToObject(field.FieldType);
        }
        
        field.SetValue(null, convertedValue);
    }

    private static void SetValue(FieldInfo field, object value) {
        object convertedValue;
        
        // Handle enums explicitly
        if (field.FieldType.IsEnum) {
            if (value is string) { 
                convertedValue = Enum.Parse(field.FieldType, value.ToString());
            }
            else { 
                convertedValue = Enum.ToObject(field.FieldType, value);
            }
        }
        else {
            convertedValue = Convert.ChangeType(value, field.FieldType);
        }
        
        field.SetValue(null, convertedValue);
    }
    
    
    public static void SaveAllSettings() {
        Dictionary<string, object> jsonFormat = new Dictionary<string, object>();
        for (int i = 0; i < settings.Count; i++) {
            string key = settings.Keys.ElementAt(i);
            FieldInfo setting = settings[key];
            //Debug.Log(setting.GetValue(null));
            jsonFormat.Add(key, setting.GetValue(null));
        }

        string json = JsonConvert.SerializeObject(jsonFormat, Formatting.Indented);
        File.WriteAllText(SettingsFilePath, json);
    }

    
    public static object GetSettingFromKey(string key) {
        return settings[key]?.GetValue(null);
    }

    public static object GetSettingFromKey(SettingName key) {
        return GetSettingFromKey(key.ToString());
    }

    public static void SetSettingValue(string key, object value) {
        SetValue(settings[key], value);
    }

    public static void SetSettingValue(SettingName key, object value) {
        SetSettingValue(key.ToString(), value);
    }
    
    
    
    // This is not working to I just call SaveAllSettings instead, there is no perceivable
    // performance difference at the moment.
    
    /*public static void SaveSetting(string key) {
        JObject json;
        if (File.Exists(SettingsFilePath)) {
            json = JObject.Parse(File.ReadAllText(SettingsFilePath));
        }
        else {
            SaveAllSettings();
            return;
        }

        json[key] = (JToken)settings[key].GetValue(null);
        Debug.Log($"Saved {key} as {json[key].ToObject(settings[key].FieldType)}");
        string formattedJson = JsonConvert.SerializeObject(json, Formatting.Indented);
        File.WriteAllText(SettingsFilePath, formattedJson);
    }

    public static void SaveSetting(SettingName key) {
        SaveSetting(key.ToString());
    }*/
}
