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
        iconImage.sprite = null;
        nameText.text = "";
        rarityText.text = "";
        flavourText.text = "";
        descriptionText.text = "";
    }
    
    public void UpdateInfo(ItemData itemData) {
        iconImage.sprite = itemData.itemIcon;
        nameText.text = itemData.name;
        rarityText.text = itemData.rarity.ToString();
        flavourText.text = itemData.itemFlavourText;
        descriptionText.text = itemData.itemDescription;
    }
}