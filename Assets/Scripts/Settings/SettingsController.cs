using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour {
    [Header("Settings")] 
    [SerializeField] private SerializedDictionary<SettingName, Setting> genericSettings = new();
    
    private AudioMixer mixer;

    public enum SettingType {
        Slider,
        Toggle,
        Dropdown,
        Enum
    }

    [Serializable] public struct Setting {
        [HideInInspector] public SettingName name;
        
        [HideInInspector] public SettingType type;

        [Header("====================")]
        [Header("Only ONE Required")]
        public BetterSlider slider;
        public Toggle toggle;
        public TMP_Dropdown dropdown;
        public UIEnum enumSetting;
        
        public object Value {
            get => GetValue();
            set => SetValue(value);
        }
        
        public void Init(SettingName name, object value) {
            this.name = name;
            
            if (slider) {
                type = SettingType.Slider;
            }
            else if (toggle) {
                type = SettingType.Toggle;
            }
            else if (dropdown) {
                type = SettingType.Dropdown;
            }
            else if (enumSetting) {
                type = SettingType.Enum;
            }
            else {
                Debug.LogError($"Setting {name} does not have a valid component.");
            }
            
            Value = value;
        }

        private object GetValue() {
            switch (type) {
                case SettingType.Slider:
                    return slider.value;
                case SettingType.Toggle:
                    return toggle.isOn;
                case SettingType.Dropdown:
                    return dropdown.value;
                case SettingType.Enum:
                    return enumSetting.value;
                default:
                    return float.NaN;
            }
        }

        private void SetValue(object value) {
            switch (type) {
                case SettingType.Slider:
                    slider.value = Convert.ToSingle(value);
                    break;
                case SettingType.Toggle:
                    toggle.isOn = Convert.ToBoolean(value);
                    break;
                case SettingType.Dropdown:
                    dropdown.value = Convert.ToInt32(value);
                    break;
                case SettingType.Enum:
                    enumSetting.Set(Convert.ToInt32(value));
                    break;
                default:
                    Debug.LogError($"Setting type {type} is not supported");
                    return;
            }
        }
        
        public void RegisterListener(Action<object> callback) {
            switch (type) {
                case SettingType.Slider:
                    slider.onValueChanged.AddListener(v => callback(v));
                    break;

                case SettingType.Toggle:
                    toggle.onValueChanged.AddListener(v => callback(v));
                    break;

                case SettingType.Dropdown:
                    dropdown.onValueChanged.AddListener(v => callback(v));
                    break;

                case SettingType.Enum:
                    enumSetting.OnIndexChanged.AddListener(v => callback(v));
                    break;

                default:
                    Debug.LogError($"Setting type {type} is not supported");
                    break;
            }
        }
        
        public void RemoveAllListeners() {
            switch (type) {
                case SettingType.Slider:
                    slider.onValueChanged?.RemoveAllListeners();
                    break;

                case SettingType.Toggle:
                    toggle.onValueChanged?.RemoveAllListeners();
                    break;

                case SettingType.Dropdown:
                    dropdown.onValueChanged?.RemoveAllListeners();
                    break;

                case SettingType.Enum:
                    enumSetting.OnIndexChanged?.RemoveAllListeners();
                    break;

                default:
                    Debug.LogError($"Setting type {type} is not supported");
                    break;
            }
        }
    }

    private void Start() {
        PlayerSettings.Load();
        
        LoadResolutions();
        
        InitialiseSettings(); // Must be called before Non-Generic Settings.

        Setting setting; // ~~Non-Generic Settings~~
        // Graphics
        genericSettings[SettingName.Resolution].RegisterListener(i => SetResolution((int)i));
        genericSettings[SettingName.DisplayMode].RegisterListener(i => SetDisplayMode((int)i));
        genericSettings[SettingName.FramerateLimit].RegisterListener(i => SetFramerateLimit((int)i));
        genericSettings[SettingName.VSync].RegisterListener(i => SetVSync((int)i));
        genericSettings[SettingName.QualityPreset].RegisterListener(i => SetQuality((int)i));
        // Audio
        genericSettings[SettingName.MasterVolume].RegisterListener(_ => SetVolume(SettingName.MasterVolume));
        genericSettings[SettingName.MusicVolume].RegisterListener(_ => SetVolume(SettingName.MusicVolume));
        genericSettings[SettingName.AmbientVolume].RegisterListener(_ => SetVolume(SettingName.AmbientVolume));
        genericSettings[SettingName.EffectsVolume].RegisterListener(_ => SetVolume(SettingName.EffectsVolume));
    }

    private void InitialiseSettings() {
        for (int i = 0; i < genericSettings.Count; i++) {
            SettingName key = genericSettings.Keys.ElementAt(i);
            Setting setting = genericSettings[key];

            setting.Init(key, PlayerSettings.GetSettingFromKey(key));
            
            setting.RegisterListener(_ => SettingChanged(key));
            
            genericSettings[key] = setting;
        }
    }

    private void OnDestroy() {
        for (int i = 0; i < genericSettings.Count; i++) {
            SettingName key = genericSettings.Keys.ElementAt(i);
            Setting setting = genericSettings[key];
            
            setting.RemoveAllListeners();
            
            genericSettings[key] = setting;
        }
    }

    private void SettingChanged(SettingName key) {
        Setting setting = genericSettings[key];
        PlayerSettings.SetSettingValue(setting.name, setting.Value);
        PlayerSettings.SaveAllSettings();
    }
    
#region Specific Settings

    // Quality Settings
    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex, false);
    }

    private Resolution[] resolutions;
    private void LoadResolutions() {
        if (!genericSettings.TryGetValue(SettingName.Resolution, out Setting setting)) return;

        setting.dropdown.ClearOptions();
        resolutions = Screen.resolutions;
        List<string> dropdownOptions = new();
        List<Resolution> newRes = new();

        for (int i = resolutions.Length - 1; i >= 0; i--) {
            string resolutionString = $"{resolutions[i].width}x{resolutions[i].height}";
            if (dropdownOptions.Contains(resolutionString)) continue;
            newRes.Add(resolutions[i]);
            dropdownOptions.Add(resolutionString);
        }

        resolutions = newRes.ToArray();

        setting.dropdown.AddOptions(dropdownOptions);

        setting.dropdown.value = FindDefaultResolution();

        setting.dropdown.RefreshShownValue();

        genericSettings[SettingName.Resolution] = setting;
    }

    private int FindDefaultResolution() {
        return Array.FindIndex(resolutions, r => r.width == Screen.width && r.height == Screen.height);
    }

    public void SetResolution(int resolutionIndex) {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void SetDisplayMode(int modeIndex) {
        switch (modeIndex) {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1: // Borderless
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2: // Windowed
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            default:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }

    public void SetFramerateLimit(int limit) {
        switch (limit) {
            case 0: // No limit
                Application.targetFrameRate = -1;
                break;
            case 1: // 30 FPS
                Application.targetFrameRate = 30;
                break;
            case 2: // 60 FPS
                Application.targetFrameRate = 60;
                break;
            case 3: // 120 FPS
                Application.targetFrameRate = 120;
                break;
            case 4: // 240 FPS
                Application.targetFrameRate = 240;
                break;
            case 5: // 360 FPS
                Application.targetFrameRate = 360;
                break;
            default:
                Application.targetFrameRate = -1;
                break;
        }
    }

    public void SetVSync(int vSync) {
        QualitySettings.vSyncCount = vSync;
    }

    //Audio Settings
    public void SetVolume(SettingName key) {
        float mappedVolume =
            Mathf.Lerp(-80f, 0f,
                genericSettings[key].slider.value /
                100f); // Assuming the slider value is between 0 and 100, map it to -80 to 0 dB

        mixer.SetFloat(key.ToString(), mappedVolume);
    }

#endregion
}
