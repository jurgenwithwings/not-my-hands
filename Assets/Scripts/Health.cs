using UnityEngine;

public class Health : MonoBehaviour
{
    private Statboard statboard;
    public void SetStatboard(Statboard board) {
        statboard ??= board;
    }
}
