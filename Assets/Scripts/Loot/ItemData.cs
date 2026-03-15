using UnityEngine;

public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    [TextArea(5, 30)] public string itemDescription = "New Description";
    [TextArea(5, 30)] public string itemFlavourText = "New Flavour Text";
    public Rarity rarity = Rarity.Rare;
    public int value = 10;
    public Sprite itemIcon;
    public GameObject prefab;
}
