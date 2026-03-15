using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBorder;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;

    private ItemData item;
    
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
        item = itemData;
        iconImage.sprite = itemData.itemIcon;
        iconImage.color = Color.white;
        iconBorder.color = itemData.rarity.Colour();
        nameText.text = $"{itemData.itemName}<size=40%>\n\n</size>";
        rarityText.text = $"<style={itemData.rarity.ToString()}>{itemData.rarity.ToString()}</style>";
        descriptionText.text = itemData.itemDescription;
        priceText.text = $"${itemData.value.ToString()}";
    }
}