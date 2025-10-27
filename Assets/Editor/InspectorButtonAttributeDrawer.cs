using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomPropertyDrawer(typeof(ButtonAttribute))]
public class ButtonAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ButtonAttribute buttonAttr = (ButtonAttribute)attribute;
        string methodName = buttonAttr.MethodName;
        string labelText = buttonAttr.Label;
        var target = property.serializedObject.targetObject;

        // Locate the method once per draw
        MethodInfo method = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (method == null)
        {
            EditorGUI.HelpBox(position, $"[Button] Method '{methodName}' not found on {target.GetType().Name}", MessageType.Warning);
            return;
        }

        Color previous = GUI.color;
        GUI.color = buttonAttr.Color;
        switch (buttonAttr.DisplayMode)
        {
            case ButtonAttribute.ButtonDisplay.DrawOnTop:
                DrawButtonOnTop(position, property, label, labelText, target, method);
                break;

            case ButtonAttribute.ButtonDisplay.Overwrite:
                DrawButtonOnly(position, labelText, target, method);
                break;

            case ButtonAttribute.ButtonDisplay.DrawInline:
                DrawButtonInline(position, property, label, labelText, target, method, previous);
                break;
        }
        GUI.color = previous;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ButtonAttribute buttonAttr = (ButtonAttribute)attribute;

        switch (buttonAttr.DisplayMode)
        {
            case ButtonAttribute.ButtonDisplay.DrawOnTop:
                return EditorGUI.GetPropertyHeight(property, label, true)
                     + EditorGUIUtility.singleLineHeight
                     + EditorGUIUtility.standardVerticalSpacing;

            case ButtonAttribute.ButtonDisplay.Overwrite:
                return EditorGUIUtility.singleLineHeight;

            case ButtonAttribute.ButtonDisplay.DrawInline:
            default:
                return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    // --- DRAWING HELPERS ---

    private void DrawButtonOnTop(Rect position, SerializedProperty property, GUIContent label,
                                 string buttonLabel, object target, MethodInfo method)
    {
        // Button rect
        Rect buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        // Field rect
        Rect fieldRect = new Rect(position.x, buttonRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                                  position.width, EditorGUI.GetPropertyHeight(property, label, true));

        if (GUI.Button(buttonRect, buttonLabel))
            method.Invoke(target, null);

        EditorGUI.PropertyField(fieldRect, property, label, true);
    }

    private void DrawButtonOnly(Rect position, string buttonLabel, object target, MethodInfo method)
    {
        if (GUI.Button(position, buttonLabel))
            method.Invoke(target, null);
    }

    private void DrawButtonInline(Rect position, SerializedProperty property, GUIContent label,
                                  string buttonLabel, object target, MethodInfo method, Color previousColor)
    {
        // Reserve space for the label
        float labelWidth = EditorGUIUtility.labelWidth;
        float buttonWidth = GUI.skin.button.CalcSize(new GUIContent(buttonLabel)).x + 10f;
        float spacing = -10f;

        // Label rect
        Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);

        // Button rect (right after the label)
        Rect buttonRect = new Rect(labelRect.xMax, position.y, buttonWidth, position.height);

        // Field rect (after the button)
        float fieldX = buttonRect.xMax + spacing;
        float fieldWidth = position.xMax - fieldX;
        Rect fieldRect = new Rect(fieldX, position.y, fieldWidth, position.height);

        // Draw label manually (no property field yet)
        EditorGUI.LabelField(labelRect, label);

        // Draw button
        if (GUI.Button(buttonRect, buttonLabel))
            method.Invoke(target, null);
        
        GUI.color = previousColor;

        // Draw the value field *without* its label, since we already drew it
        EditorGUI.PropertyField(fieldRect, property, GUIContent.none, true);
    }
}