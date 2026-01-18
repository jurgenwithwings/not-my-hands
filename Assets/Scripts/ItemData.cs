using UnityEngine;

public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    [TextArea] public string itemDescription = "New Description";
    public Rarity rarity = Rarity.Rare;
    public Sprite icon;
    public GameObject prefab;
}
