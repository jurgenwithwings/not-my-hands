using UnityEngine;

public class LightBeamArm : Arm {
    private static readonly int FingerGun = Animator.StringToHash("FingerGun");
    
    [SerializeField] private Damage damage;
    [SerializeField] private float fireRate = 0.7f;
    
    private float fireCooldown;
    
    
    private void Update() {
        if (input.Value > 0) {
            Shoot();
        }
        else {
            animator.SetInteger(FingerGun, ArmFingerGunState.None.ToInt());
        }
        
        fireCooldown -= Time.deltaTime;
    }
    
    private void Shoot() {
        if (fireCooldown > 0) return;
        fireCooldown = fireRate;
        animator.SetInteger(FingerGun, ArmFingerGunState.Channel.ToInt());
        
        Vector3 endPoint = transform.position + (Vector3.forward * 10f);
        RaycastHit[] hits = Physics.CapsuleCastAll(transform.position, endPoint, 0.7f, transform.forward);
        foreach (RaycastHit hit in hits) {
            if (hit.collider.gameObject == manager.gameObject) {
                continue;
            }
            else if (hit.collider.gameObject.TryGetComponent(out Statboard victim)) {
                DamageInfo info = new(damage, statboard, hit.point);
                victim.health?.TakeDamage(info);
            }
        }
    }
}
