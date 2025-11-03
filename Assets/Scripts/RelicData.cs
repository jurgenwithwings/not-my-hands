using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "ScriptableObjects/Relic")]
public class RelicData : ScriptableObject {
    public string displayName = "Relic";
    [TextArea] public string description = "Kool Relic";
    public ClassReference<Relic> relicType;
    public Sprite icon;
    public Rarity rarity;
}