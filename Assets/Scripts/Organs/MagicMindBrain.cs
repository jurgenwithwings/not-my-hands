using System;
using UnityEngine;

[Serializable] public class MagicMindBrain : Organ {
    [SerializeField] private float manaRefillPercent = 0.15f;
    [SerializeField] private float judgedBonus = 2f;
    
    public override void Initialise(Statboard statboard, OrganData organData) {
        base.Initialise(statboard, organData);
        
        MagicMindBrain config = organData.organClass as MagicMindBrain;
        manaRefillPercent = config.manaRefillPercent;
        
        statboard.eventManager.OnKilledTarget += OnKilledTarget;
    }

    private void OnKilledTarget(Statboard target) {
        float bonus = 1;
        if (target.statusEffectManager.GetEffectFromList(GameConfig.Instance.judged)) {
            bonus = judgedBonus;
        }
        stats.mana.AddMana(stats.maxMana.Value * manaRefillPercent * bonus);
    }
}