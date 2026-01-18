using System;
using System.Collections.Generic;
using UnityEngine;

public class LootDropHandler : MonoBehaviour
{
    [SerializeField] private LootTable lootTable;
    [Space]
    [SerializeField] private List<LootTable.LootItem> uniqueLoot;
    [SerializeField] private LootTable guaranteedLoot;
    [SerializeField] private Vector2Int guaranteedDropRange = Vector2Int.one;
    [SerializeField] private float enemyInfluence = 1.0f;
    [SerializeField] private Vector2Int dropRange = Vector2Int.one;

    private struct DroppedItem {
        public LootTable.LootItem item;
        public float adjustedWeight;
        
        public DroppedItem(LootTable.LootItem item, float adjustedWeight) {
            this.item = item;
            this.adjustedWeight = adjustedWeight;
        }
    }
    
    private void Start() {
        GetComponent<Health>().onDeath += HandleLootDrop;
    }

    private void HandleLootDrop(Statboard killer) {
        //Check for negative luck values
        bool negateLuck = killer.luck <= 0;
        
        if (guaranteedLoot != null) {
            List<LootTable.LootItem> guaranteedItemPool = new List<LootTable.LootItem>(guaranteedLoot.items);
            float guaranteedTotalWeight = guaranteedLoot.GetTotalWeight(killer.luck, enemyInfluence);
            
            int guaranteedDropsToSpawn = UnityEngine.Random.Range(dropRange.x, dropRange.y + 1);

            for (int i = 0; i < guaranteedDropsToSpawn; i++) {
                DroppedItem droppedItem = SpawnLoot(guaranteedItemPool, guaranteedTotalWeight, killer.luck, enemyInfluence);
                guaranteedTotalWeight -= droppedItem.adjustedWeight;
                guaranteedItemPool.Remove(droppedItem.item);
            }
        }
        
        List<LootTable.LootItem> itemPool = new List<LootTable.LootItem>();
        float totalWeight = 0f;
        
        if (uniqueLoot != null && uniqueLoot.Count > 0) {
            float uniqueWeight = LootTable.GetTotalWeightFromList(uniqueLoot, killer.luck, enemyInfluence);
            totalWeight += uniqueWeight;
            itemPool.AddRange(uniqueLoot);
        }
        
        totalWeight += lootTable.GetTotalWeight(killer.luck, enemyInfluence);
        itemPool.AddRange(lootTable.items);

        int dropsToSpawn = UnityEngine.Random.Range(dropRange.x, dropRange.y + 1);
        
        for (int i = 0; i < dropsToSpawn; i++) {
            DroppedItem droppedItem = SpawnLoot(itemPool, totalWeight, killer.luck, enemyInfluence);
            totalWeight -= droppedItem.adjustedWeight;
            itemPool.Remove(droppedItem.item);
        }
    }
    
    private DroppedItem SpawnLoot(List<LootTable.LootItem> itemPool, float totalWeight, float luck, float enemyInfluence) {
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
                Instantiate(item.data.prefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(Vector3.up * 3, ForceMode.Impulse);
                break;
            }
        }
        Debug.Log($"Luck: {luck} | Enemy Influence: {enemyInfluence} | TotalWeight: {totalWeight} | Roll: {roll} | DroppedItem: {droppedItem.Value.data.itemName} | Base Weight: {droppedItem.Value.dropWeight} Multiplier: {multiplier} | Item Weight: {adjustedWeight}");
        
        return new DroppedItem(droppedItem.Value, adjustedWeight);
    }
}
