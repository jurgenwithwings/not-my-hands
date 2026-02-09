using UnityEngine;

public class BasicArm : Arm {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;

    private void Update() {
        if (input.Value > 0) {
            Shoot();
        }
        else {
            animator.SetBool("Attack", false);
        }

        if (animator.GetBool(AnimIdle)) {
            IsBusy = false;
        }
    }
    
    private void Shoot() {
        IsBusy = true;
        PlayerHUDEvents.DebugText("Pew");
        animator.SetBool("Attack", true);
        Instantiate(projectilePrefab, projectilePoint.position, projectilePoint.rotation).GetComponent<Rigidbody>().AddForce(transform.forward * 5, ForceMode.Impulse);
    }
}