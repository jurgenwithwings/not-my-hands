using System.Collections.Generic;
using UnityEngine;

public class ShopController : Menu {
    [SerializeField] private RectTransform Background;
    [SerializeField] private List<ShopItem> itemPanels = new();

    private Shop shop;
    private Statboard customer;
    
    public void RegisterItems(ItemData[] items, bool[] soldStates, Shop shop, Statboard customer) {
        this.shop = shop;
        this.customer = customer;
        for (int i = 0; i < items.Length; i++) {
            itemPanels[i].gameObject.SetActive(true);
            itemPanels[i].UpdateInfo(items[i], soldStates[i]);
        }
    }

    private void Start() {
        Background.anchoredPosition = Vector2.up * 10000f;

        foreach (ShopItem item in itemPanels) {
            item.BuyButton.onClick.AddListener(() => OnBuyButtonClicked(item));
        }
    }

    private void OnBuyButtonClicked(ShopItem shopItem) {
        if (shopItem.isSold) return;
        if (!customer.TryGetComponent(out CurrencyManager currency)) return;

        if (currency.HasAmount(shopItem.item.value) && shop.SpawnItem(shopItem.item)) {
            currency.RemoveCurrency(shopItem.item.value);
            shopItem.SetSold(true);
        }
    }
    
    public override void OpenMenu() {
        Background.anchoredPosition = Vector2.zero;
    }
    public override bool CloseMenu() {
        shop = null;
        customer = null;
        Background.anchoredPosition = Vector2.up * 10000f;
        return true;
    }
}