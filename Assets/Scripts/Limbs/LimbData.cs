using UnityEngine;

[CreateAssetMenu(fileName = "Limb", menuName = "ScriptableObjects/Limb")]
public class LimbData : ItemData {
    public LimbType limbType;
    public GameObject limbPrefab;
}