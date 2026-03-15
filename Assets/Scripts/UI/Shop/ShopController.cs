using System.Collections.Generic;
using UnityEngine;

public class ShopController : Menu {
    [SerializeField] private RectTransform Background;
    [SerializeField] private List<ShopItem> itemPanels = new();

    private Statboard customer;
    
    public void RegisterItems(ItemData[] items, Statboard customer) {
        this.customer = customer;
        for (int i = 0; i < items.Length; i++) {
            if (i >= itemPanels.Count) {
                itemPanels[i].gameObject.SetActive(false);
            }
            
            itemPanels[i].gameObject.SetActive(true);
            itemPanels[i].UpdateInfo(items[i]);
        }
    }

    private void Start() {
        Background.anchoredPosition = Vector2.up * 10000f;
    }
    
    public override void OpenMenu() {
        Background.anchoredPosition = Vector2.zero;
    }
    public override bool CloseMenu() {
        customer = null;
        Background.anchoredPosition = Vector2.up * 10000f;
        return true;
    }
}