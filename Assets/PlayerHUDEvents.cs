using System;

public static class PlayerHUDEvents {
    /// <summary>
    /// Should be called on Late Update.
    /// </summary>
    public static Action<string> OnSetInteractionText;
    
    public static Action<float> OnSetHealth;
}