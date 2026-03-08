using UnityEngine;

public class BasicArm : Arm {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float projectileForce = 5;
    [SerializeField] private Damage damage;
    [SerializeField] private float fireRate = 0.17f;
    
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
        Instantiate(projectilePrefab, projectilePoint.position, transform.rotation)
            .GetComponent<Projectile>().Initialise(statboard, damage, transform.forward * projectileForce);
    }
}