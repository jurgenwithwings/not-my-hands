using UnityEngine;

public class BasicArm : Arm {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float projectileForce = 5;
    [SerializeField] private Damage damage;
    [SerializeField, Range(0, 1)] private float statusChance = 0.2f;
    [SerializeField] private float fireRate = 0.17f;
    
    private float fireCooldown;

    private void Update() {
        if (input.Value > 0) {
            Shoot();
        }
        else {
            animator.SetBool("Attack", false);
        }
        
        fireCooldown -= Time.deltaTime;

        if (animator.GetBool(AnimIdle)) {
            IsBusy = false;
        }
    }
    
    private void Shoot() {
        if (fireCooldown > 0) return;
        fireCooldown = fireRate;
        IsBusy = true;
        animator.SetBool("Attack", true);
        Instantiate(projectilePrefab, projectilePoint.position, transform.rotation)
            .GetComponent<TestArmProjectile>().Initialise(statboard, damage, statusChance, projectileForce);
    }
}