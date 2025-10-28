#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

[CustomPropertyDrawer(typeof(ClassReference<>))]
public class ClassReferenceDrawer : PropertyDrawer
{
    private AdvancedDropdownState _dropdownState = new AdvancedDropdownState();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var baseType = fieldInfo.FieldType.GetGenericArguments()[0];
        var classNameProp = property.FindPropertyRelative("className");

        string currentClass = classNameProp.stringValue;
        Type currentType = string.IsNullOrEmpty(currentClass) ? null : Type.GetType(currentClass);
        string displayName = currentType != null ? currentType.Name : "None";

        EditorGUI.BeginProperty(position, label, property);

        // Draw label and dropdown button
        position = EditorGUI.PrefixLabel(position, label);
        if (EditorGUI.DropdownButton(position, new GUIContent(displayName), FocusType.Keyboard))
        {
            var dropdown = new ClassTypeDropdown(_dropdownState, baseType, (selectedType) =>
            {
                classNameProp.stringValue = selectedType?.AssemblyQualifiedName;
                property.serializedObject.ApplyModifiedProperties();
            });

            dropdown.Show(position);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => EditorGUIUtility.singleLineHeight;
}


public class ClassTypeDropdown : AdvancedDropdown
{
    private readonly Action<Type> _onSelect;
    private readonly Type _baseType;
    private readonly List<Type> _types;

    public ClassTypeDropdown(AdvancedDropdownState state, Type baseType, Action<Type> onSelect) : base(state)
    {
        _onSelect = onSelect;
        _baseType = baseType;

        // Cache and sort valid types
        _types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch { return Type.EmptyTypes; } // Safely skip bad assemblies
            })
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t))
            .OrderBy(t => string.IsNullOrEmpty(t.Namespace) ? "" : t.Namespace)
            .ThenBy(t => t.Name)
            .ToList();

        minimumSize = new Vector2(350, 400);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Select " + _baseType.Name);

        // Add "None" option first
        root.AddChild(new TypeDropdownItem("None", null));

        // Separate namespace-less and namespaced types
        var noNamespaceTypes = _types.Where(t => string.IsNullOrEmpty(t.Namespace)).ToList();
        var grouped = _types
            .Where(t => !string.IsNullOrEmpty(t.Namespace))
            .GroupBy(t => t.Namespace)
            .OrderBy(g => g.Key);

        // Add namespace-less types directly under the root
        foreach (var type in noNamespaceTypes)
            root.AddChild(new TypeDropdownItem(type.Name, type));

        // Then add namespace groups
        foreach (var group in grouped)
        {
            var groupItem = new AdvancedDropdownItem(group.Key);
            foreach (var type in group)
                groupItem.AddChild(new TypeDropdownItem(type.Name, type));

            root.AddChild(groupItem);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item is TypeDropdownItem typeItem)
            _onSelect?.Invoke(typeItem.Type);
    }

    private class TypeDropdownItem : AdvancedDropdownItem
    {
        public Type Type { get; }

        public TypeDropdownItem(string name, Type type) : base(name)
        {
            Type = type;
        }
    }
}
#endif