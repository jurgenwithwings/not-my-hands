using UnityEngine;

public class EntityStatusEffectManager : MonoBehaviour
{
    private Statboard statboard;
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }
}
