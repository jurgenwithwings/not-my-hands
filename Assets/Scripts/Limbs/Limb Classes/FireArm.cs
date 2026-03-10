using UnityEngine;

public class FireArm : Arm {
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float projectileSpeed = 25;
    [SerializeField] private float projectileSpread = 4f;
    [SerializeField] private Damage damage;
    [SerializeField] private float fireRate = 0.06f;
    
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
        
        Vector3 spread = Random.insideUnitCircle * projectileSpread;
        Instantiate(projectilePrefab, projectilePoint.position, Quaternion.Euler(manager.fpsCam.transform.rotation.eulerAngles + spread))
            .GetComponent<Projectile>().Initialise(statboard, damage, projectileSpeed);
    }
}