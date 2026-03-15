using System;
using System.Collections.Generic;
using Stats;
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
        result.Add($"   Max Health: {DisplayStat(statboard.maxHealth, 1, "0")}HP");
        result.Add($"   Damage Resistance: {DisplayStat(statboard.damageResistance, 100, "0.#")}%");
        result.Add($"   Healing Effectiveness: {DisplayStat(statboard.healingEffectiveness, 100f, "0.#")}%");
        result.Add($"   Passive Regen Rate: {DisplayStat(statboard.passiveRegenRate, 1, "0.#")}HP/s");
        result.Add("");
        result.Add("Brain:");
        result.Add($"   Critical Chance: {DisplayStat(statboard.criticalChanceMultiplier, 100, "0.#")}%");
        result.Add($"   Critical Damage: {DisplayStat(statboard.criticalDamageMultiplier, 100, "0.#")}%");
        result.Add($"   Luck: {DisplayStat(statboard.luck, 1, "0")}");
        result.Add("");
        result.Add("Liver:");
        result.Add($"   Status Chance: {DisplayStat(statboard.statusChanceMultiplier, 100, "0.#")}%");
        result.Add($"   Buff Duration: {DisplayStat(statboard.buffDurationMultiplier, 100, "0.#")}%");
        result.Add($"   Buff Potency: {DisplayStat(statboard.buffPotencyMultiplier, 100, "0.#")}%");
        result.Add("");
        result.Add("Movement:");
        result.Add($"   Move Speed: {DisplayStat(statboard.moveSpeed, 1, "0.#")}m/s");
        result.Add($"   Jump Count: {DisplayStat(statboard.jumpCount, 1, "0")}");
        result.Add("");
        result.Add("Mana:");
        result.Add($"   Max Mana: {DisplayStat(statboard.maxMana, 1, "0")}Mana");
        result.Add($"   Mana Regen Rate: {DisplayStat(statboard.manaRegenRate, 1, "0.#")}Mana/s");
        result.Add("");
        result.Add("Combat:");
        result.Add($"   All Damage: {DisplayStat(statboard.damageMultiplier, 100, "0.#")}%");
        result.Add($"   Melee Damage: {DisplayStat(statboard.meleeDamageMultiplier, 100, "0.#")}%");
        result.Add($"   Ranged Damage: {DisplayStat(statboard.rangedDamageMultiplier, 100, "0.#")}%");
        result.Add($"   Elemental Damage: {DisplayStat(statboard.elementalDamageMultiplier, 100, "0.#")}%");
        result.Add($"   Projectile Speed: {DisplayStat(statboard.projectileSpeedMultiplier, 100, "0.#")}%");
        result.Add("");
        result.Add("Elemental:");
        result.Add("   Damage:");
        result.Add($"       Physical: {DisplayStat(statboard.damageMultipliers.physical, 100, "0.#")}%");
        result.Add($"       Fire: {DisplayStat(statboard.damageMultipliers.fire, 100, "0.#")}%");
        result.Add($"       Ice: {DisplayStat(statboard.damageMultipliers.ice, 100, "0.#")}%");
        result.Add($"       Electric: {DisplayStat(statboard.damageMultipliers.electric, 100, "0.#")}%");
        result.Add($"       Poison: {DisplayStat(statboard.damageMultipliers.poison, 100, "0.#")}%");
        result.Add($"       Light: {DisplayStat(statboard.damageMultipliers.light, 100, "0.#")}%");
        result.Add("");
        result.Add("   Resistance:");
        result.Add($"       Physical: {DisplayStat(statboard.damageResistances.physical, 100, "0.#")}%");
        result.Add($"       Fire: {DisplayStat(statboard.damageResistances.fire, 100, "0.#")}%");
        result.Add($"       Ice: {DisplayStat(statboard.damageResistances.ice, 100, "0.#")}%");
        result.Add($"       Electric: {DisplayStat(statboard.damageResistances.electric, 100, "0.#")}%");
        result.Add($"       Poison: {DisplayStat(statboard.damageResistances.poison, 100, "0.#")}%");
        result.Add($"       Light: {DisplayStat(statboard.damageResistances.light, 100, "0.#")}%");
        result.Add("Misc:");
        result.Add($"   Currency Bonus{DisplayStat(statboard.currencyMultiplier, 100, "0.#")}%");
        
        text.text = string.Join("\n", result);
    }

    private string DisplayStat(Stat stat, float mult, string format) {
        string result = GetModifiedColour(stat);
        bool diff = !String.IsNullOrEmpty(result);

        result += (stat.Value * mult).ToString(format);

        if (diff) {
            result += "</color>";
        }
        
        return result;
    }

    private string GetModifiedColour(Stat stat) {
        string result = "<color=#";
        
        if (stat.Value < stat.BaseValue) {
            result += "ff0000>";
        }
        else if (stat.Value > stat.BaseValue) {
            result += "00ff00>";
        }
        else {
            result = "";
        }

        return result;
    }
}
