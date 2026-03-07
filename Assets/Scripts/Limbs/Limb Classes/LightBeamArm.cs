using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class LightBeamArm : Arm {
    private static readonly int FingerGun = Animator.StringToHash("FingerGun");

    [SerializeField] private VisualEffect beamEffect;
    [Space]
    [SerializeField] private Damage damage;
    [SerializeField] private float fireRate = 0.7f;
    [SerializeField] private float range = 25f;
    
    private float fireCooldown;

    private void Start() {
        beamEffect.Stop();
    }
    
    private void Update() {
        if (input.Value > 0 && fireCooldown <= 0 && shootRoutine ==  null) {
            shootRoutine = StartCoroutine(Shoot());
        }
        /*else {
            animator.SetInteger(FingerGun, ArmFingerGunState.None.ToInt());
        }*/
        
        fireCooldown -= Time.deltaTime;
    }

    private Coroutine shootRoutine;
    private IEnumerator Shoot() {
        animator.SetInteger(FingerGun, ArmFingerGunState.Channel.Index());
        
        yield return new WaitForSeconds((fireRate * 0.7f) - 1f);
        
        beamEffect.Play();
        
        yield return new WaitForSeconds(1);
        
        LayerMask mask = GameConfig.Instance.pawnLayer;
        mask.AddLayerToMask(GameConfig.Instance.levelCollisionLayer);
        
        Vector3 endPoint = manager.fpsCam.transform.position + (Vector3.forward * range);
        RaycastHit[] hits = Physics.CapsuleCastAll(manager.fpsCam.transform.position, endPoint, 0.7f, manager.fpsCam.transform.forward, mask);
        foreach (RaycastHit hit in hits) {
            if (hit.collider.gameObject == manager.gameObject) {
                continue;
            }

            if (hit.collider.gameObject.layer == GameConfig.Instance.levelCollisionLayer) {
                break;
            }
            
            if (hit.collider.gameObject.TryGetComponent(out Statboard victim)) {
                DamageInfo info = new(damage, statboard, hit.point);
                victim.health?.TakeDamage(info);
            }
        }
        
        animator.SetInteger(FingerGun, ArmFingerGunState.None.Index());
        
        yield return new WaitForSeconds(fireRate * 0.3f);
        
        fireCooldown = fireRate;
        shootRoutine = null;
    }
}
