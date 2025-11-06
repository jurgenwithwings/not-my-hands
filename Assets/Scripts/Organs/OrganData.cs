using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Organ", menuName = "ScriptableObjects/Organ")]
public class OrganData : ScriptableObject {
    public string displayName;
    public Sprite icon;
    public OrganType type;
    public ClassReference<Organ> organClass;
    public Rarity rarity;
    [TextArea] public string description;
    [Space] 
    public OrganStat[] stats;

    //Set defaults for type
    public void OnValidate() {
        switch (type) {
            case OrganType.Heart:
                if (stats.Length == 0 || stats[0].statType != Statboard.VariableType.MaxHealth) {
                    OrganStat[] newStats = new OrganStat[5];
                    newStats[0].statType = Statboard.VariableType.MaxHealth;
                    newStats[0].value = stats.Length >= 1 ? stats[0].value : 100;
                    
                    newStats[1].statType = Statboard.VariableType.DamageResistance;
                    newStats[1].value = stats.Length >= 2 ? stats[1].value : 0;
                    
                    newStats[2].statType = Statboard.VariableType.Defense;
                    newStats[2].value = stats.Length >= 3 ? stats[2].value : 0;
                    
                    newStats[3].statType = Statboard.VariableType.HealingEffectiveness;
                    newStats[3].value = stats.Length >= 4 ? stats[3].value : 1;
                    
                    newStats[4].statType = Statboard.VariableType.PassiveRegenRate;
                    newStats[4].value = stats.Length >= 5 ? stats[4].value : 0;
                    
                    stats = newStats;
                }
                break;
            case OrganType.Lungs:
                if (stats.Length == 0 || stats[0].statType != Statboard.VariableType.MoveSpeed) {
                    OrganStat[] newStats = new OrganStat[3];
                    newStats[0].statType = Statboard.VariableType.MoveSpeed;
                    newStats[0].value = stats.Length >= 1 ? stats[0].value : 6;
                    
                    newStats[1].statType = Statboard.VariableType.AttackSpeedMultiplier;
                    newStats[1].value = stats.Length >= 2 ? stats[1].value : 1;

                    newStats[2].statType = Statboard.VariableType.ActiveAbilityCooldownRate;
                    newStats[2].value = stats.Length >= 3 ? stats[2].value : 1;
                    
                    stats = newStats;
                }
                break;
            case OrganType.Brain:
                if (stats.Length == 0 || stats[0].statType != Statboard.VariableType.CriticalChanceMultiplier) {
                    OrganStat[] newStats = new OrganStat[3];
                    newStats[0].statType = Statboard.VariableType.CriticalChanceMultiplier;
                    newStats[0].value = stats.Length >= 1 ? stats[0].value : 1;
                    
                    newStats[1].statType = Statboard.VariableType.CriticalDamageMultiplier;
                    newStats[1].value = stats.Length >= 2 ? stats[1].value : 1;
                    
                    newStats[2].statType = Statboard.VariableType.Luck;
                    newStats[2].value = stats.Length >= 3 ? stats[2].value : 1;
                    
                    stats = newStats;
                }
                break;
            case OrganType.Liver:
                if (stats.Length == 0 || stats[0].statType != Statboard.VariableType.StatusChanceMultiplier) {
                    OrganStat[] newStats = new OrganStat[4];
                    newStats[0].statType = Statboard.VariableType.StatusChanceMultiplier;
                    newStats[0].value = stats.Length >= 1 ? stats[0].value : 1;
                    
                    newStats[1].statType = Statboard.VariableType.FlaskCooldown;
                    newStats[1].value = stats.Length >= 2 ? stats[1].value : 30;
                    
                    newStats[2].statType = Statboard.VariableType.BuffDurationMultiplier;
                    newStats[2].value = stats.Length >= 3 ? stats[2].value : 1;
                    
                    newStats[3].statType = Statboard.VariableType.BuffPotencyMultiplier;
                    newStats[3].value = stats.Length >= 4 ? stats[3].value : 1;
                    
                    stats = newStats;
                }
                break;
        }
    }
}