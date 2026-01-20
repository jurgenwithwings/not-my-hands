using System;

public static class PlayerHUDEvents {
    /// <summary>
    /// Should be called on Late Update.
    /// </summary>
    public static Action<string> OnSetInteractionText;
    
    /// <summary>
    /// Called when the player's health changes.
    /// <para><c>Current Health</c> - Current health of the player</para>
    /// <para><c>Maximum Health</c> - Maximum health of the player</para>
    /// </summary>
    public static Action<float, float> OnHealthChanged;
}