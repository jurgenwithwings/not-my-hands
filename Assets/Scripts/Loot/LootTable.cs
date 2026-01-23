using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot Table", menuName = "ScriptableObjects/Loot/Loot Table")]
public class LootTable : ScriptableObject {
    [Serializable] public struct LootItem : IEquatable<LootItem> {
        public ItemData data;
        public float dropWeight;
        
        public LootItem(ItemData data, float dropWeight) {
            this.data = data;
            this.dropWeight = dropWeight;
        }
        
        private void SetDefault() {
            if (dropWeight > 0f || IsNull()) { return; }
            Type type = data.GetType();
            switch (type.Name) {
                case "RelicData":
                    switch (data.rarity) {
                        case Rarity.Rare:
                            dropWeight = 100f;
                            break;
                        case Rarity.Epic:
                            dropWeight = 35f;
                            break;
                        case Rarity.Legendary:
                            dropWeight = 5f;
                            break;
                        case Rarity.Cursed:
                            dropWeight = 1f;
                            break;
                    }
                    break;
                case "OrganData" or "LimbData":
                    switch (data.rarity) {
                        case Rarity.Rare:
                            dropWeight = 20f;
                            break;
                        case Rarity.Epic:
                            dropWeight = 7f;
                            break;
                        case Rarity.Legendary:
                            dropWeight = 2f;
                            break;
                        case Rarity.Cursed:
                            dropWeight = 1f;
                            break;
                    }
                    break;
            }
        }

        #region Checks
        public bool IsNull() {
            return data == null;
        }
        
        public bool Equals(LootItem other) {
            return Equals(data, other.data) && dropWeight.Equals(other.dropWeight);
        }

        public override bool Equals(object obj) {
            return obj is LootItem other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(data, dropWeight);
        }
        #endregion
    }
    
    public List<LootItem> items;
    private float rareWeight;
    private float epicWeight;
    private float legendaryWeight;
    private float cursedWeight;

    public float BaseTotalWeight => rareWeight + epicWeight + legendaryWeight + cursedWeight;
    
    public float GetTotalWeight(float luck, float enemyInfluence = 1f) {
        float totalWeight = 0;

        if (BaseTotalWeight == 0) {
            CollectRarityWeights();
        }

        totalWeight += rareWeight * GetWeightInfluenceMultiplier(Rarity.Rare, luck, enemyInfluence);
        totalWeight += epicWeight * GetWeightInfluenceMultiplier(Rarity.Epic, luck, enemyInfluence);
        totalWeight += legendaryWeight * GetWeightInfluenceMultiplier(Rarity.Legendary, luck, enemyInfluence);
        totalWeight += cursedWeight * GetWeightInfluenceMultiplier(Rarity.Cursed, luck, enemyInfluence);
        
        return totalWeight;
    }
    
    public static float GetTotalWeightFromList(List<LootItem> lootItems, float luck, float enemyInfluence = 1f) {
        float rareWeight = 0f;
        float epicWeight = 0f;
        float legendaryWeight = 0f;
        float cursedWeight = 0f;

        foreach (LootItem item in lootItems) {
            if (item.IsNull()) continue;
            
            switch (item.data.rarity) {
                case Rarity.Rare:
                    rareWeight += item.dropWeight;
                    break;
                case Rarity.Epic:
                    epicWeight += item.dropWeight;
                    break;
                case Rarity.Legendary:
                    legendaryWeight += item.dropWeight;
                    break;
                case Rarity.Cursed:
                    cursedWeight += item.dropWeight;
                    break;
            }
        }

        float totalWeight = 0;

        totalWeight += rareWeight * GetWeightInfluenceMultiplier(Rarity.Rare, luck, enemyInfluence);
        totalWeight += epicWeight * GetWeightInfluenceMultiplier(Rarity.Epic, luck, enemyInfluence);
        totalWeight += legendaryWeight * GetWeightInfluenceMultiplier(Rarity.Legendary, luck, enemyInfluence);
        totalWeight += cursedWeight * GetWeightInfluenceMultiplier(Rarity.Cursed, luck, enemyInfluence);
        
        return totalWeight;
    }
    
    public static float GetWeightInfluenceMultiplier(Rarity rarity, float luck, float enemyInfluence) {
        float bonus = 0;
        float exp = 0.7f;
        
        switch (rarity) {
            case Rarity.Rare:
                bonus = 0;
                exp = 0.7f;
                break;
            case Rarity.Epic:
                bonus = 0.3f;
                exp = 0.7f;
                break;
            case Rarity.Legendary:
                bonus = 0.7f;
                exp = 0.8f;
                break;
            case Rarity.Cursed:
                bonus = 1f;
                exp = 0.9f;
                break;
        }
        
        return Luck.GetLuckCurve(luck, enemyInfluence, bonus, exp);
    }

    private void CollectRarityWeights() {
        rareWeight = 0f;
        epicWeight = 0f;
        legendaryWeight = 0f;
        cursedWeight = 0f;
        
        if (items.Count == 0 || items[0].IsNull()) return;
        
        for (int i = 0; i < items.Count; i++) {
            if (items[i].IsNull()) continue;
            
            switch (items[i].data.rarity) {
                case Rarity.Rare:
                    rareWeight += items[i].dropWeight;
                    break;
                case Rarity.Epic:
                    epicWeight += items[i].dropWeight;
                    break;
                case Rarity.Legendary:
                    legendaryWeight += items[i].dropWeight;
                    break;
                case Rarity.Cursed:
                    cursedWeight += items[i].dropWeight;
                    break;
                default:
                    rareWeight += items[i].dropWeight;
                    break;
            }

            if (items[i].dropWeight <= 0f) {
                Debug.LogWarning($"Loot Item '{items[i].data.itemName}' in Loot Table '{name}' has a drop weight of 0 or less.");
            }
        }
    }
}