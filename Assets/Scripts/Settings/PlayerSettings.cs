using UnityEngine;

public enum OnScreenControllerUI {
    Xbox,
    PlayStation,
    Switch,
}
public static class PlayerSettings
{
    // Gameplay
    public static bool AbbreviateDamageNumbers = false;
    public static OnScreenControllerUI controllerUI = OnScreenControllerUI.Xbox;
    
    // Input
    public static float MouseSensitivity = 30f;
    public static float ControllerSensitivity = 150f;
    public static bool IsInvertedMouseY = false;
    public static float InnerDeadzone = 0.125f;
    public static float OuterDeadzone = 0.125f;

    // Audio
    public static float MasterVolume = 0.35f;
    public static float MusicVolume = 1f;
    public static float AmbientVolume = 1f;
    public static float EffectsVolume = 1f;
}
