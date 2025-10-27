#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Generic editor that only changes drawing when SectionAttribute or SectionEndAttribute is used on the inspected type.
[CustomEditor(typeof(UnityEngine.Object), true)]
[CanEditMultipleObjects]
public class SectionedEditor : Editor
{
    // foldout state keyed by "TypeFullName:SectionName"
    static Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    // Bold foldout style for section headers
    private static GUIStyle sectionFoldoutStyle;

    public override void OnInspectorGUI()
    {
        // ensure style is initialized
        if (sectionFoldoutStyle == null)
        {
            sectionFoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
        }

        serializedObject.Update();

        // Quick check: if target type has no SectionAttribute or SectionEndAttribute on any field, fallback to default inspector.
        if (!TypeHasSectionMarkers(target.GetType()))
        {
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
            return;
        }

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        // Draw the script reference first
        if (prop.NextVisible(enterChildren))
        {
            EditorGUILayout.PropertyField(prop, true);
        }

        string currentSection = null;

        while (prop.NextVisible(false))
        {
            FieldInfo field = GetFieldInfo(prop.name, target.GetType());
            var sectionAttr = field != null ? field.GetCustomAttribute<SectionAttribute>(true) : null;
            var sectionEndAttr = field != null ? field.GetCustomAttribute<SectionEndAttribute>(true) : null;

            if (sectionAttr != null)
            {
                // new section starts here
                currentSection = sectionAttr.Name ?? "";
                string key = GetFoldoutKey(target.GetType(), currentSection);
                if (!foldouts.ContainsKey(key))
                    foldouts[key] = true;

                // Use the bold foldout style for section headers
                foldouts[key] = EditorGUILayout.Foldout(foldouts[key], currentSection, true, sectionFoldoutStyle);
                if (foldouts[key])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(prop, true);
                    EditorGUI.indentLevel--;
                }
                // if folded, skip drawing this property (it remains part of the section)
            }
            else if (sectionEndAttr != null)
            {
                // treat SectionEndAttribute as section break: close current section and draw spacing + this property normally
                currentSection = null;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(prop, true);
            }
            else
            {
                // No explicit marker: belongs to currentSection (if any) or drawn normally
                if (!string.IsNullOrEmpty(currentSection))
                {
                    string key = GetFoldoutKey(target.GetType(), currentSection);
                    if (!foldouts.ContainsKey(key))
                        foldouts[key] = true;

                    if (foldouts[key])
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(prop, true);
                        EditorGUI.indentLevel--;
                    }
                    // else skip drawing because the section is folded
                }
                else
                {
                    // Not in a section, draw normally
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static string GetFoldoutKey(Type t, string sectionName)
    {
        return $"{t.FullName}:{sectionName}";
    }

    // Checks if any field has SectionAttribute or SectionEndAttribute
    private static bool TypeHasSectionMarkers(Type t)
    {
        if (t == null) return false;
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (Type check = t; check != null && check != typeof(object); check = check.BaseType)
        {
            foreach (var f in check.GetFields(flags))
            {
                if (f.GetCustomAttribute<SectionAttribute>(true) != null || f.GetCustomAttribute<SectionEndAttribute>(true) != null)
                    return true;
            }
        }
        return false;
    }

    // Try to find a backing FieldInfo for the serialized property name (handles common cases)
    private static FieldInfo GetFieldInfo(string propertyName, Type targetType)
    {
        if (string.IsNullOrEmpty(propertyName) || targetType == null)
            return null;

        // Direct field match
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo fi = targetType.GetField(propertyName, flags);
        if (fi != null) return fi;

        // Handle common "m_" and backing field naming for auto-properties
        string[] candidates = new string[]
        {
            propertyName,
            "m_" + propertyName,
            "<" + propertyName + ">k__BackingField"
        };

        Type t = targetType;
        while (t != null && t != typeof(object))
        {
            foreach (var name in candidates)
            {
                fi = t.GetField(name, flags);
                if (fi != null) return fi;
            }
            t = t.BaseType;
        }

        return null;
    }
}
#endif