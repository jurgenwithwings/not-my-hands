using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInfoPanel : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text flavourText;
    [SerializeField] private TMP_Text descriptionText;

    private void Start() {
        Clear();
    }

    private void OnDisable() {
        Clear();
    }

    private void Clear() {
        iconImage.color = Color.clear;
        nameText.text = "";
        rarityText.text = "";
        flavourText.text = "";
        descriptionText.text = "";
    }
    
    public void UpdateInfo(ItemData itemData) {
        iconImage.sprite = itemData.itemIcon;
        Color rarityColor = Color.white;
        switch (itemData.rarity) {
            case Rarity.None:
                rarityColor = Color.gray;
                break;
            case Rarity.Rare:
                rarityColor = new(0f, .5f, 1f);
                break;
            case Rarity.Epic:
                rarityColor = new(0.4f, 0f, 1f);
                break;
            case Rarity.Legendary:
                rarityColor = new(1f, 0.4f, 0.1f);
                break;
            case Rarity.Cursed:
                rarityColor = Color.red;
                break;
        }
        iconImage.color = rarityColor;
        nameText.text = itemData.itemName;
        rarityText.text = itemData.rarity.ToString();
        rarityText.color = rarityColor;
        flavourText.text = itemData.itemFlavourText;
        descriptionText.text = itemData.itemDescription;
    }
}