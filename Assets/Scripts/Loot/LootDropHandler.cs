using System;
using System.Collections.Generic;
using UnityEngine;

public class LootDropHandler : MonoBehaviour
{
    [Header("Common Drop Table (Required)")]
    [SerializeField] private LootTable lootTable;
    [Space]
    [Header("Optional Unique Loot (Added to the Common Drop Table)")]
    [SerializeField] private List<LootTable.LootItem> uniqueLoot;
    [Space]
    [SerializeField, Range(0, 100)] private float initialDropChance = 100;
    [SerializeField, Range(0, 100)] private float extraDropChance = 10;
    [SerializeField] private int maxDrops = 2;
    [Space]
    [Header("Optional additional Loot that is rolled separately to the Common Drops")]
    [SerializeField] private LootTable additionalLoot;
    [SerializeField, Range(0, 100)] private float additionalInitialDropChance = 100;
    [SerializeField, Range(0, 100)] private float additionalExtraDropChance = 10;
    [SerializeField] private int additionalMaxDrops = 1;
    [Space]
    [Header("Extra Settings")]
    [SerializeField] private float enemyInfluence = 1.0f;

    private struct DroppedItem {
        public LootTable.LootItem item;
        public float adjustedWeight;
        
        public DroppedItem(LootTable.LootItem item, float adjustedWeight) {
            this.item = item;
            this.adjustedWeight = adjustedWeight;
        }
    }
    
    private void Start() {
        GetComponent<Health>().OnDeath += HandleLootDrop;
    }

    private void HandleLootDrop(Statboard killer) {
        float luckMult = Luck.GetLuckCurve(killer.luck);
        
        // Handle additional loot drops
        if (additionalLoot != null) {
            List<LootTable.LootItem> additionalItemPool = new List<LootTable.LootItem>(additionalLoot.items);
            
            float additionalTotalWeight = additionalLoot.GetTotalWeight(killer.luck, enemyInfluence);
            
            int additionalDropsToSpawn = GetDropAmount(additionalInitialDropChance, additionalExtraDropChance, additionalMaxDrops, luckMult);

            for (int i = 0; i < additionalDropsToSpawn; i++) {
                var droppedItem = SpawnLoot(additionalItemPool, additionalTotalWeight, killer.luck, enemyInfluence);
                additionalTotalWeight -= droppedItem.adjustedWeight;
                additionalItemPool.Remove(droppedItem.item);
            }
        }
        
        List<LootTable.LootItem> itemPool = new List<LootTable.LootItem>();
        float totalWeight = 0f;
        
        // Add unique loot to the pool if available
        if (uniqueLoot != null && uniqueLoot.Count > 0) {
            float uniqueWeight = LootTable.GetTotalWeightFromList(uniqueLoot, killer.luck, enemyInfluence);
            totalWeight += uniqueWeight;
            itemPool.AddRange(uniqueLoot);
        }
        
        totalWeight += lootTable.GetTotalWeight(killer.luck, enemyInfluence);
        itemPool.AddRange(lootTable.items);

        int dropsToSpawn = GetDropAmount(initialDropChance, extraDropChance, maxDrops, luckMult);
        
        for (int i = 0; i < dropsToSpawn; i++) {
            var droppedItem = SpawnLoot(itemPool, totalWeight, killer.luck, enemyInfluence);
            totalWeight -= droppedItem.adjustedWeight;
            itemPool.Remove(droppedItem.item);
        }
    }
    
    private (LootTable.LootItem item, float adjustedWeight) SpawnLoot(List<LootTable.LootItem> itemPool, float totalWeight, float luck, float enemyInfluence) {
        float roll = UnityEngine.Random.Range(0f, totalWeight);
        
        float cumulativeWeight = 0f;
        float adjustedWeight = 0f;
        float multiplier = 0f;
        LootTable.LootItem? droppedItem = null;
        
        foreach (LootTable.LootItem item in itemPool) {
            droppedItem = item;
            
            multiplier = LootTable.GetWeightInfluenceMultiplier(item.data.rarity, luck, enemyInfluence);

            adjustedWeight = item.dropWeight * multiplier;
            
            cumulativeWeight += adjustedWeight;
            
            if (roll <= cumulativeWeight) {
                Vector2 randomDirection = UnityEngine.Random.insideUnitCircle;
                Vector3 randomDirectionVector = new(randomDirection.x, 0, randomDirection.y);
                Instantiate(item.data.prefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce((Vector3.up + randomDirectionVector) * 5, ForceMode.Impulse);
                break;
            }
        }
        //PlayerHUDEvents.DebugText($"Luck: {luck} | Enemy Influence: {enemyInfluence} | TotalWeight: {totalWeight} | Roll: {roll} | DroppedItem: {droppedItem.Value.data.itemName} | Base Weight: {droppedItem.Value.dropWeight} Multiplier: {multiplier} | Item Weight: {adjustedWeight}", 10);
        
        return (droppedItem.Value, adjustedWeight);
    }

    private int GetDropAmount(float initial, float extra, int max, float luckMult) {
        int dropCount = 0;
        
        float adjustedInitial = initial * luckMult;
        float adjustedExtra = extra * luckMult;
        
        // Increase max drops if luck significantly increases extra drop chance
        if (extra < 75 && adjustedExtra > 100) {
            max *= (int)adjustedExtra / 100;
        }

        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll <= adjustedInitial) {
            dropCount++;
            
            for (int i = dropCount; i < max; i++) {
                roll = UnityEngine.Random.Range(0f, 100f);
                
                if (roll <= adjustedExtra) {
                    dropCount++;
                } 
                else {
                    break;
                }
            }
        }

        return dropCount;
    }
}
