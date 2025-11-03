using UnityEngine;

public enum Rarity {
    Rare,
    Epic,
    Legendary,
    Cursed,
}

public static class RarityExtensions {
    public static Color Colour(this Rarity rarity) => rarity switch {
        Rarity.Rare => "4080d6".ToHexColor(),
        Rarity.Epic => "8f65e4".ToHexColor(),
        Rarity.Legendary => "ffaf52".ToHexColor(),
        Rarity.Cursed => "d83d65".ToHexColor(),
        _ => Color.magenta
    };
}