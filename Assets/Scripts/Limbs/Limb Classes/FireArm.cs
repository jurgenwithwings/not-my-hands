using UnityEngine;

public class FireArm : Arm {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float projectileForce = 10;
    [SerializeField] private float projectileSpread = 0.2f;
    [SerializeField] private Damage damage;
    [SerializeField] private float fireRate = 0.07f;
    
    private float fireCooldown;

    private void Update() {
        if (input.Value > 0 && statboard.mana.HasEnoughMana(manaCost)) {
            Shoot();
        }
        else {
            animator.SetBool("Attack", false);
        }
        
        fireCooldown -= Time.deltaTime;
    }
    
    private void Shoot() {
        if (fireCooldown > 0) return;
        if (!statboard.mana.RemoveMana(manaCost)) return;
        fireCooldown = fireRate;
        animator.SetBool("Attack", true);
        Vector3 force = manager.fpsCam.transform.forward * projectileForce;
        Vector3 spread = Random.insideUnitSphere * projectileSpread;
        spread.z = 0;
        force += spread;
        Instantiate(projectilePrefab, projectilePoint.position, manager.fpsCam.transform.rotation)
            .GetComponent<Projectile>().Initialise(statboard, damage, force);
    }
}