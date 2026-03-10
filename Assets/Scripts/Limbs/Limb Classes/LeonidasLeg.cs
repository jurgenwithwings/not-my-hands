using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;

public class LeonidasLeg : Leg {
    private static readonly int Front = Animator.StringToHash("Front");
    private static readonly int AnimSpeed = Animator.StringToHash("AnimSpeed");

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private float projectileSpeed = 10;
    [SerializeField] private Damage damage;
    [SerializeField] private float animHoldDuration = 0.2f;
    [Space]
    [SerializeField] private float critDamageIncrease = 1.25f;
    
    private bool fired;
    private Modifier modifier;
    private string source = "Leonidas";

    public override void Initialise(LimbData data, LimbManager manager, Statboard statboard) {
        base.Initialise(data, manager, statboard);
        
        modifier = new Modifier(critDamageIncrease, ModifierType.Final, source);
        this.statboard.criticalDamageMultiplier.AddModifier(modifier);
        
        this.statboard.eventManager.OnPreDealDamage += OnPreDealDamage;
    }

    // Guarantees Critical hits
    private void OnPreDealDamage(ref DamageInfo damageInfo, Statboard victim) {
        if (damageInfo.tags.Contains(source) && damageInfo.resultingCritLevel <= 0) {
            damageInfo.AddModifier(damageInfo.source.criticalDamageMultiplier.Value);
            damageInfo.resultingCritLevel++;
        }
    }

    public override void Remove() {
        base.Remove();
        statboard.criticalDamageMultiplier.RemoveModifier(modifier);
        
        statboard.eventManager.OnPreDealDamage -= OnPreDealDamage;
    }

    private void Update() {
        if (input.Triggered && !manager.IsLegBusy) {
            Kick();
        }
    }

    private void Kick() {
        if (!statboard.mana.RemoveMana(manaCost)) return;
        animator.SetTrigger(Front);
        animator.SetFloat(AnimSpeed, 0.2f);
        statboard.eventManager.OnLegAbilityUsed?.Invoke(manaCost);
    }
    
    public override void ToggleHitbox() {
        if (!fired) {
            animator.SetFloat(AnimSpeed, 50);
        
            Projectile proj = Instantiate(projectilePrefab, projectilePoint.position, manager.fpsCam.transform.rotation).GetComponent<Projectile>();
            proj.Initialise(statboard, damage, projectileSpeed, new List<object>{ source });
            
            fired = true;
        }
        else {
            StartCoroutine(Recovery());
        }
    }

    private IEnumerator Recovery() {
        animator.SetFloat(AnimSpeed, 0);
        
        yield return new WaitForSeconds(animHoldDuration);
        
        animator.SetFloat(AnimSpeed, 1);
        
        fired = false;
    }
}