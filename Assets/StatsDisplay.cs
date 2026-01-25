using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsDisplay : MonoBehaviour
{
    private Statboard statboard;
    
    [SerializeField] private TMP_Text text;
    
    private void Awake() {
        PlayerHUDEvents.OnRegisterStatboard += OnRegisterStatboard;
    }

    private void OnRegisterStatboard(Statboard statboard) {
        this.statboard = statboard;
        Set();
    }

    private void FixedUpdate() {
        if (statboard != null) {
            Set();
        }
    }

    private void Set() {
        List<string> result = new();
        
        result.Add("Heart:");
        result.Add($"   Max Health: {statboard.maxHealth.Value:0}HP");
        result.Add($"   Damage Resistance: {statboard.damageResistance * 100f:0.#}%");
        result.Add($"   Healing Effectiveness: {statboard.healingEffectiveness * 100f:0.#}%");
        result.Add($"   Passive Regen Rate: {statboard.passiveRegenRate.Value:0.#}HP/s");
        result.Add("");
        result.Add("Brain:");
        result.Add($"   Critical Chance: {statboard.criticalChanceMultiplier * 100f:0.#}%");
        result.Add($"   Critical Damage: {statboard.criticalDamageMultiplier * 100f:0.#}%");
        result.Add($"   Luck: {statboard.luck.Value:0}");
        result.Add("");
        result.Add("Liver:");
        result.Add($"   Status Chance: {statboard.statusChanceMultiplier * 100f:0.#}%");
        result.Add($"   Buff Duration: {statboard.buffDurationMultiplier * 100f:0.#}%");
        result.Add($"   Buff Potency: {statboard.buffPotencyMultiplier * 100f:0.#}%");
        result.Add("");
        result.Add("Movement:");
        result.Add($"   Move Speed: {statboard.moveSpeed.Value:0.#}m/s");
        result.Add($"   Jump Count: {statboard.jumpCount.Value:0}");
        result.Add("");
        result.Add("Mana:");
        result.Add($"   Max Mana: {statboard.maxMana.Value:0}Mana");
        result.Add($"   Mana Regen Rate: {statboard.manaRegenRate.Value:0.#}Mana/s");
        result.Add("");
        result.Add("Combat:");
        result.Add($"   All Damage: {statboard.damageMultiplier * 100f:0.#}%");
        result.Add($"   Melee Damage: {statboard.meleeDamageMultiplier * 100f:0.#}%");
        result.Add($"   Ranged Damage: {statboard.rangedDamageMultiplier * 100f:0.#}%");
        result.Add($"   Elemental Damage: {statboard.elementalDamageMultiplier * 100f:0.#}%");
        result.Add($"   Projectile Speed: {statboard.projectileSpeedMultiplier * 100f:0.#}%");
        result.Add("");
        result.Add("Elemental:");
        result.Add("   Damage:");
        result.Add($"       Physical: {statboard.damageMultipliers.physical * 100f:0.#}%");
        result.Add($"       Fire: {statboard.damageMultipliers.fire * 100f:0.#}%");
        result.Add($"       Ice: {statboard.damageMultipliers.ice * 100f:0.#}%");
        result.Add($"       Electric: {statboard.damageMultipliers.electric * 100f:0.#}%");
        result.Add($"       Poison: {statboard.damageMultipliers.poison * 100f:0.#}%");
        result.Add($"       Light: {statboard.damageMultipliers.light * 100f:0.#}%");
        result.Add("");
        result.Add("   Resistance:");
        result.Add($"       Physical: {statboard.damageResistances.physical * 100f:0.#}%");
        result.Add($"       Fire: {statboard.damageResistances.fire * 100f:0.#}%");
        result.Add($"       Ice: {statboard.damageResistances.ice * 100f:0.#}%");
        result.Add($"       Electric: {statboard.damageResistances.electric * 100f:0.#}%");
        result.Add($"       Poison: {statboard.damageResistances.poison * 100f:0.#}%");
        result.Add($"       Light: {statboard.damageResistances.light * 100f:0.#}%");
        result.Add("Misc:");
        result.Add($"   Currency Bonus{statboard.currencyMultiplier* 100f:0.#}%");
        
        text.text = string.Join("\n", result);
    }
}
