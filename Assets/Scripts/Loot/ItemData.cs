using UnityEngine;

public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    [TextArea] public string itemDescription = "New Description";
    [TextArea] public string itemFlavourText = "New Flavour Text";
    public Rarity rarity = Rarity.Rare;
    public Sprite itemIcon;
    public GameObject prefab;
}
