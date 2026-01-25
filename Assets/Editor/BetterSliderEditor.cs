using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(BetterSlider))]
public class BetterSliderEditor : Editor
{
    private Editor sliderEditor;

    private void OnEnable()
    {
        // Create an editor for the base Slider
        var slider = (Slider)target;
        sliderEditor = CreateEditor(slider, typeof(UnityEditor.UI.SliderEditor));
    }

    public override void OnInspectorGUI()
    {
        // Draw Unity's built-in Slider inspector
        if (sliderEditor != null)
            sliderEditor.OnInspectorGUI();

        // Draw BetterSlider-specific fields
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("BetterSlider Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("text"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("format"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("snapInterval"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonIncrement"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("leftButton"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rightButton"));

        serializedObject.ApplyModifiedProperties();
    }
}