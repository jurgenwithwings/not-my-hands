using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicLeg : Leg {
    private static readonly int Sweep = Animator.StringToHash("Sweep");
    
    [SerializeField] private Collider hitbox;
    [Space]
    [SerializeField] private Damage damage;
    
    private List<GameObject> hitObjects = new();

    private void Update() {
        if (input.Triggered && !manager.IsLegBusy) {
            Kick();
        }
    }

    private void Kick() {
        if (!statboard.mana.RemoveMana(manaCost)) return;
        animator.SetTrigger(Sweep);
        IsAnimBusy = true;
        statboard.eventManager.OnLegAbilityUsed?.Invoke(manaCost);
    }

    public override void ToggleHitbox() {
        base.ToggleHitbox();
        
        hitObjects.Clear();
        hitbox.enabled = !hitbox.enabled;
    }

    public void OnTriggerEnter(Collider other) {
        if (hitObjects.Contains(other.gameObject)) return;
        if (other.gameObject == manager.gameObject) return;
        
        if (other.gameObject.TryGetComponent(out Statboard victim)) {
            DamageInfo info = new(damage, statboard, other.gameObject.transform.position);
            victim.health?.TakeDamage(info);
            hitObjects.Add(other.gameObject);
        }
    }
}