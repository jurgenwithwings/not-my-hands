using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour, IInteractable {
    public bool HasAltInteraction { get; } = false;
    public string InteractionName() => "Trade with The Rat";

    [SerializeField] private LootTable lootTable;
    
    private ItemData[] items = new ItemData[3];

    private float luck = 3;

    private void Awake() {
        RollShopItems();
    }

    private void RollShopItems() {
        List<LootTable.LootItem> itemPool = new List<LootTable.LootItem>();
        float totalWeight = lootTable.GetTotalWeight(luck);
        itemPool.AddRange(lootTable.items);
        
        for (int i = 0; i < 3; i++) {
            var pickedItem = LootDropHandler.SpawnLoot(itemPool, totalWeight, luck, 1);
            totalWeight -= pickedItem.adjustedWeight;
            itemPool.Remove(pickedItem.item);
            items[i] = pickedItem.item.data;
        }
    }
    
    public void Interact(Statboard interactor) {
        ShopController shopController = CanvasManager.Instance.OpenMenu(MenuType.Shop) as ShopController;
        if (shopController == null) {
            CanvasManager.Instance.CanCloseMenu(MenuType.Shop);
            return;
        }
        
        shopController.RegisterItems(items, interactor);
    }
}
