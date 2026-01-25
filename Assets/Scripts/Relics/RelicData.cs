using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "ScriptableObjects/Relic")]
public class RelicData : ItemData {
    [SerializeReference, SubclassSelector] public Relic relicType;
}