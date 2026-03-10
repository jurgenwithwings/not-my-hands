using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;
using UnityEngine.VFX;

public class ColdStomp : Leg {
    private static readonly int Sweep = Animator.StringToHash("Sweep");

    [SerializeField] private Damage damage;
    [SerializeField] private float effectDistanceDown = 1.6f;
    [SerializeField] private float radius = 4.5f;
    [SerializeField] private VisualEffect visualEffect;
    
    private bool fired;
    private Modifier modifier;
    private string source = "ColdStomp";

    private void Start() {
        visualEffect.Stop();
    }
    
    public override void Initialise(LimbData data, LimbManager manager, Statboard statboard) {
        base.Initialise(data, manager, statboard);
        
        this.statboard.eventManager.OnCausedStatusEffect += OnCausedStatusEffect;
    }

    // Double Freeze Effects
    private void OnCausedStatusEffect(DamageInfo damageInfo, Statboard victim) {
        if (damageInfo.tags.Contains(source)) return;
        
        List<StatusEffect> effects = damageInfo.resultingAppliedEffects.FindAll(effect => effect.GetType() == typeof(Freeze));
        if (effects.Count == 0) return;

        DamageInfo info = DamageInfo.Empty(statboard, damageInfo.hitPoint);
        info.tags.Add(source);
        for (int i = 0; i < effects.Count; i++) {
            info.additionalStatusEffects.Add(GameConfig.Instance.freeze);        
        }
        victim.health.TakeDamage(info);
    }

    public override void Remove() {
        base.Remove();
        
        statboard.eventManager.OnCausedStatusEffect -= OnCausedStatusEffect;
    }

    private void Update() {
        if (input.Triggered && !manager.IsLegBusy) {
            Kick();
        }
    }

    private void Kick() {
        if (!statboard.mana.RemoveMana(manaCost)) return;
        animator.SetTrigger(Sweep);
        statboard.eventManager.OnLegAbilityUsed?.Invoke(manaCost);
    }
    
    public override void ToggleHitbox() {
        if (!fired) {
            StartCoroutine(KeepEffectSteady(manager.fpsCam.transform.position + (Vector3.down * effectDistanceDown)));
            visualEffect.Play();
            
            Collider[] hits = Physics.OverlapSphere(manager.fpsCam.transform.position, radius, GameConfig.Instance.pawnLayer);

            if (hits.Length > 0) {
                foreach (Collider hit in hits) {
                    if (hit.gameObject == manager.gameObject) continue;
                    if (hit.gameObject.TryGetComponent(out Statboard victim)) {
                        DamageInfo info = new(damage, statboard, hit.transform.position);
                        info.additionalStatusEffects.Add(GameConfig.Instance.freeze);
                        victim.health.TakeDamage(info);
                    }
                }
            }
            fired = true;
        }
        else {
            fired = false;
        }
    }

    private IEnumerator KeepEffectSteady(Vector3 position) {
        float timer = 0f;
        float duration = 0.51f;

        while (timer < duration) {
            visualEffect.transform.position = position;
            visualEffect.transform.rotation = Quaternion.identity;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}