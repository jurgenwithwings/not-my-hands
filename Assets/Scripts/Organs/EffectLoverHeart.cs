using System;
using Stats;

[Serializable] public class EffectLoverHeart : Organ {
    public float perStatusEffectBonus = 0.4f;
    
    public override void Initialise(Statboard statboard, OrganData organData) {
        base.Initialise(statboard, organData);
        
        EffectLoverHeart config = organData.organClass as EffectLoverHeart;
        perStatusEffectBonus = config.perStatusEffectBonus;

        stats.eventManager.OnPreDealDamage += ModifyDamage;
    }

    private void ModifyDamage(ref DamageInfo damageInfo, Statboard victim) {
        if (victim.statusEffectManager.StatusEffects.Count > 0) {
            damageInfo.AddModifier(perStatusEffectBonus * victim.statusEffectManager.StatusEffects.Count, ModifierType.Additive);
        }
    }

    public override void Remove() {
        base.Remove();
        stats.eventManager.OnPreDealDamage -= ModifyDamage;
    }
}