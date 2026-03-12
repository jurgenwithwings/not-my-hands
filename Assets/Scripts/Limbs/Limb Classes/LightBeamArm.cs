using System.Collections;
using Stats;
using UnityEngine;
using UnityEngine.VFX;

public class LightBeamArm : Arm {
    private static readonly int FingerGun = Animator.StringToHash("FingerGun");

    [SerializeField] private VisualEffect beamEffect;
    [Space]
    [SerializeField] private Damage damage;
    [SerializeField] private float fireRate = 0.7f;
    [SerializeField] private float range = 25f;
    [Space]
    [Header("Passive")]
    [SerializeField] private float lightDamageIncrease = 1.12f;
    private Modifier modifier;
    
    private float fireCooldown;

    private void Start() {
        beamEffect.Stop();
    }

    public override void Initialise(LimbData data, LimbManager manager, Statboard statboard) {
        base.Initialise(data, manager, statboard);

        modifier = new Modifier(lightDamageIncrease, ModifierType.FinalMultiply, "LightBeamArm");
        this.statboard.damageMultipliers.light.AddModifier(modifier);
    }

    public override void Remove() {
        base.Remove();
        
        statboard.damageMultipliers.light.RemoveModifier(modifier);
    }

    private void Update() {
        if (input.Value > 0 && fireCooldown <= 0 && shootRoutine ==  null && statboard.mana.RemoveMana(manaCost)) {
            shootRoutine = StartCoroutine(Shoot());
        }
        
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
        
        RaycastHit[] hits = Physics.SphereCastAll(manager.fpsCam.transform.position, 0.7f, manager.fpsCam.transform.forward, range, mask);
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
