using UnityEngine;

public enum OnScreenControllerUI {
    Xbox,
    PlayStation,
    Switch,
}
public static class PlayerSettings
{
    public static bool IsInvertedMouseY = false;
    public static float MouseSensitivity = 30f;
    public static float ControllerSensitivity = 150f;
    public static OnScreenControllerUI controllerUI = OnScreenControllerUI.PlayStation;
}
