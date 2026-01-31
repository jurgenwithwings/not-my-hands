using UnityEngine;

public class Arm : Limb {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;

    private void Update() {
        if (input.Value > 0) {
            Shoot();
        }
        else {
            animator.SetBool("Attack", false);
        }
    }
    
    private void Shoot() {
        PlayerHUDEvents.DebugText("Pew");
        animator.SetBool("Attack", true);
        Instantiate(projectilePrefab, projectilePoint.position, projectilePoint.rotation).GetComponent<Rigidbody>().AddForce(transform.forward * 5, ForceMode.Impulse);
    }
}