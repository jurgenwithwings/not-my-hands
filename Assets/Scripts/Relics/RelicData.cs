using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "ScriptableObjects/Relic")]
public class RelicData : ItemData {
    public ClassReference<Relic> relicType;
}