using UnityEngine;

public class EntityEventManager : MonoBehaviour
{
    private Statboard statboard;
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }
}
