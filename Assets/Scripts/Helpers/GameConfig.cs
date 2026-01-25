using UnityEngine;

[CreateAssetMenu(fileName = "Game Config", menuName = "Game Config")]
public class GameConfig : ScriptableObject
{
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("Game Config");
            }
            return _instance;
        }
    }

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
