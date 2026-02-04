using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInfoPanel : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBorder;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    private void Start() {
        Clear();
    }

    private void OnDisable() {
        Clear();
    }

    private void Clear() {
        iconImage.color = Color.clear;
        iconBorder.color = Color.clear;
        nameText.text = "";
        descriptionText.text = "";
    }
    
    public void UpdateInfo(ItemData itemData) {
        iconImage.sprite = itemData.itemIcon;
        iconImage.color = Color.white;
        iconBorder.color = itemData.rarity.Colour();
        nameText.text = $"{itemData.itemName}<size=40%>\n\n</size>" +
                        $"<size=80%><style={itemData.rarity.ToString()}>{itemData.rarity.ToString()}</size></style><size=40%>\n\n</size>" +
                        $"<style=Flavour>{itemData.itemFlavourText}</style>";
        descriptionText.text = itemData.itemDescription;
    }
}