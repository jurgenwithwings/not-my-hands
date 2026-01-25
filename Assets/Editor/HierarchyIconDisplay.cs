using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyIconDisplay
{
    private static bool hierarchyHasFocus;
    
    private static EditorWindow hierarchyEditorWindow;
    
    static HierarchyIconDisplay()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnEditorUpdate()
    {
        if (hierarchyEditorWindow == null)
        {
            hierarchyEditorWindow = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor"));
        }

        hierarchyHasFocus = EditorWindow.focusedWindow != null &&
                            EditorWindow.focusedWindow == hierarchyEditorWindow;
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;
        
        // Comment out to make prefabs have Icons
        //if (PrefabUtility.GetCorrespondingObjectFromSource(go) != null) return;
        
        Component[] components = go.GetComponents<Component>();
        if (components == null || components.Length == 0) return;
        
        Component component = components.Length > 1 ? components[1] : components[0];
        
        Type type = component.GetType();
        
        if (type == typeof(CanvasRenderer)) 
        {
            component = components[2]; // display tmp or image instead of canvas renderer
            type = component.GetType();
        }
        
        GUIContent content = EditorGUIUtility.ObjectContent(component , type);
        content.text = null;
        content.tooltip = type.Name;

        if (content.image == null) return;
        
        bool isSelected = Selection.instanceIDs.Contains(instanceID);
        bool isHovering = selectionRect.Contains(Event.current.mousePosition);
        
        Color color = GetBackgroundColor(isSelected, hierarchyHasFocus ,isHovering);
        Rect backgroundRect = selectionRect;
        backgroundRect.width = 18.5f;
        EditorGUI.DrawRect(backgroundRect, color);
        
        EditorGUI.LabelField(selectionRect, content);
    }
    
    #region EditorColor
    private static readonly Color DefaultColor = new Color(0.7843f, 0.7843f, 0.7843f);
    private static readonly Color DefaultProColor = new Color(0.2196f, 0.2196f, 0.2196f);
    
    private static readonly Color SelectedColor = new Color(0.22745f, 0.447f, 0.6902f);
    private static readonly Color SelectedProColor = new Color(0.1725f, 0.3647f, 0.5294f);
    
    private static readonly Color UnFocusedColor = new Color(0.68f, 0.68f, 0.68f);
    private static readonly Color UnFocusedProColor = new Color(0.3f, 0.3f, 0.3f);
    
    private static readonly Color HoveredColor = new Color(0.698f, 0.698f, 0.698f);
    private static readonly Color HoveredProColor = new Color(0.2706f, 0.2706f, 0.2706f);
    
    private static Color GetBackgroundColor(bool isSelected, bool isFocused, bool isHovered)
    {
        if (isSelected)
        {
            if (isFocused)
            {
                return EditorGUIUtility.isProSkin ? SelectedProColor : SelectedColor;
            }
            else
            {
                return EditorGUIUtility.isProSkin ? UnFocusedProColor : UnFocusedColor;
            }
        }
        if (isHovered)
        {
            return EditorGUIUtility.isProSkin ? HoveredProColor : HoveredColor;
        }
        else
        {
            return EditorGUIUtility.isProSkin ? DefaultProColor : DefaultColor;
        }
    }
    #endregion
}
