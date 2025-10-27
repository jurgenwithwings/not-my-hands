using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class ButtonAttribute : PropertyAttribute
{
    public enum ButtonDisplay
    {
        DrawOnTop,   // Button drawn above the field
        Overwrite,   // Replaces the field entirely
        DrawInline   // Draws beside the field
    }
    
    public string MethodName { get; }
    public string Label { get; }
    public Color Color { get; }
    public ButtonDisplay DisplayMode { get; }

    public ButtonAttribute(string methodName, string label = null, ButtonDisplay displayMode = ButtonDisplay.DrawOnTop, float r = 0.9f, float g = 0.9f, float b = 0.9f)
    {
        MethodName = methodName;
        Label = label ?? methodName;
        Color = new Color(r, g, b);
        DisplayMode = displayMode;
    }
}