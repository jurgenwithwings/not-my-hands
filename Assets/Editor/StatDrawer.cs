#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Stats.Stat))]
public class StatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var baseValueProp = property.FindPropertyRelative("BaseValue");
        if (baseValueProp != null)
        {
            EditorGUI.PropertyField(position, baseValueProp, label);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Missing BaseValue");
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var baseValueProp = property.FindPropertyRelative("BaseValue");
        return baseValueProp != null
            ? EditorGUI.GetPropertyHeight(baseValueProp, label)
            : base.GetPropertyHeight(property, label);
    }
}
#endif