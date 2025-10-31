using UnityEditor;
using UnityEngine;

public class SceneGameToggleWindow : EditorWindow
{
    private bool showScene = true;
    private bool showGame = true;

    [MenuItem("Window/Scene/Game View Toggle")]
    public static void ShowWindow()
    {
        GetWindow<SceneGameToggleWindow>("View Toggle");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene / Game View Toggle", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Show Scene + Game (Side by Side) [F1]"))
        {
            ShowBoth();
        }

        if (GUILayout.Button("Show Game View Only [F2]"))
        {
            ShowGameOnly();
        }

        if (GUILayout.Button("Show Scene View Only [F3]"))
        {
            ShowSceneOnly();
        }

        HandleShortcuts(Event.current);
    }

    private void HandleShortcuts(Event e)
    {
        if (e.type != EventType.KeyDown) return;

        switch (e.keyCode)
        {
            case KeyCode.F1: ShowBoth(); e.Use(); break;
            case KeyCode.F2: ShowGameOnly(); e.Use(); break;
            case KeyCode.F3: ShowSceneOnly(); e.Use(); break;
        }
    }

    private void ShowBoth()
    {
        showScene = true;
        showGame = true;
        ToggleViews();
    }

    private void ShowGameOnly()
    {
        showScene = false;
        showGame = true;
        ToggleViews();
    }

    private void ShowSceneOnly()
    {
        showScene = true;
        showGame = false;
        ToggleViews();
    }

    private void ToggleViews()
    {
        foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
        {
            if (window is SceneView sceneView)
                sceneView.Show(showScene);
            else if (window.GetType().ToString() == "UnityEditor.GameView")
                window.Show(showGame);
        }
    }
}