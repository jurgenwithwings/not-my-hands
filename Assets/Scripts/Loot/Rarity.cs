using UnityEngine;

public enum Rarity {
    None,
    Rare,
    Epic,
    Legendary,
    Cursed,
}

public static class RarityExtensions {
    public static Color Colour(this Rarity rarity) => rarity switch {
        Rarity.None => "c5c5c5".ToHexColor(),
        Rarity.Rare => "4080d6".ToHexColor(),
        Rarity.Epic => "8f65e4".ToHexColor(),
        Rarity.Legendary => "ffaf52".ToHexColor(),
        Rarity.Cursed => "d83d65".ToHexColor(),
        _ => Color.magenta
    };
}