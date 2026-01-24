using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Organ", menuName = "ScriptableObjects/Organ")]
public class OrganData : ItemData {
    [Button(nameof(SetDefaultStats), "Set Default Stats", ButtonAttribute.ButtonDisplay.DrawInline)] 
    public OrganType type;
    public OrganStat[] stats;
    public ClassReference<Organ> organClass;

    //Set defaults for type of organ selected.
    public void SetDefaultStats() {
        int i = 0;
        List<OrganStat> newStats = new List<OrganStat>();
        switch (type) {
            case OrganType.Heart:
                SetStatValue(ref newStats, ref i, Statboard.VariableType.MaxHealth, 100);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.DamageResistance, 0);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.HealingEffectiveness, 1);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.PassiveRegenRate, 0);
                break;
            case OrganType.Brain:
                SetStatValue(ref newStats, ref i, Statboard.VariableType.CriticalChanceMultiplier, 1);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.CriticalDamageMultiplier, 1);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.Luck, 1);
                break;
            case OrganType.Liver:
                SetStatValue(ref newStats, ref i, Statboard.VariableType.StatusChanceMultiplier, 1);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.BuffDurationMultiplier, 1);
                
                SetStatValue(ref newStats, ref i, Statboard.VariableType.BuffPotencyMultiplier, 1);
                break;
        }
        stats = newStats.ToArray();
    }
    
    private void SetStatValue(ref List<OrganStat> list, ref int index, Statboard.VariableType type, float value) {
        OrganStat stat = new() {
            statType = type,
            value = value
        };
        
        if (index >= list.Count) {
            list.Add(stat);
        }
        else {
            list[index] = stat;
        }
        
        index++;
    }
}