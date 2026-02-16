using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "Game Config", menuName = "Game Config")]
public class GameConfig : ScriptableObject
{
    private static GameConfig instance;
    public static GameConfig Instance {
        get {
            if (instance == null) {
                instance = Resources.Load<GameConfig>("Game Config");
            }
            return instance;
        }
    }

    private static bool dirtyDamageTypes = true;
    private static int damageTypes;
    public static int DamageTypesCount {
        get {
            switch (dirtyDamageTypes) {
                case true:
                    damageTypes = Enum.GetValues(typeof(DamageType)).Length;
                    dirtyDamageTypes = false;
                    break;
            }
            return damageTypes;
        }
    }
    
    [Header("Layers")]
    public LayerMask pawnLayer;
    public LayerMask ignoreRaycastLayer;
    public LayerMask levelCollisionLayer;
    

    [Header("Base Status Effects")]
    public StatusEffectData bleed;
    public StatusEffectData burn;
    public StatusEffectData freeze;
    public StatusEffectData charged;
    public StatusEffectData poison;
    public StatusEffectData judged;
    
    [Space]
    [Header("Empty GameObject")]
    public GameObject emptyGameObject;
}
