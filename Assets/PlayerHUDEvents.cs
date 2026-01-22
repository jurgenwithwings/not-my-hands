using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerHUDEvents {
    
    /// <summary>
    /// Call it update the Interaction Prompt on the UI.
    /// <para><c>Text</c> - The text to be displayed.</para>
    /// <para><c>DualInteraction</c> - If there is a secondary Interaction.</para>
    /// </summary>
    public static Action<string, bool> OnSetInteractionText;
    
    /// <summary>
    /// Called when the player's health changes.
    /// <para><c>Current Health</c> - Current health of the player</para>
    /// <para><c>Maximum Health</c> - Maximum health of the player</para>
    /// </summary>
    public static Action<float, float> OnHealthChanged;

    /// <summary>
    /// Adds the given debug text to the player's HUD.
    /// </summary>
    /// <param name="text">The text to be shown.</param>
    /// <param name="time">The amount of time the text will be visible.</param>
    /// <param name="key">Overrides any existing text with the same key.</param>
    /// <param name="color">The colour of the text.</param>
    public static void DebugText(string text, float time = 3f, object key = null, Color? color = null) {
        OnDebug?.Invoke(text, time, key, color);
    }
    
    public static Action<string, float, object, Color?> OnDebug;
}