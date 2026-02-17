using UnityEngine;

public class FireArm : Arm {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float projectileForce = 10;
    [SerializeField] private float projectileSpread = 0.2f;
    [SerializeField] private Damage damage;
    [SerializeField, Range(0, 1)] private float statusChance = 0.25f;
    [SerializeField] private float fireRate = 0.07f;
    
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
        Vector3 force = transform.forward * projectileForce;
        Vector3 spread = Random.insideUnitSphere * projectileSpread;
        spread.z = 0;
        force += spread;
        Instantiate(projectilePrefab, projectilePoint.position, transform.rotation)
            .GetComponent<Projectile>().Initialise(statboard, damage, statusChance, force);
    }
}