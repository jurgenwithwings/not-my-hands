using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour {
    [SerializeField] private Button buyButton;
    public Button BuyButton => buyButton;
    [Space]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBorder;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;

    public ItemData item { get; private set; }
    public bool isSold { get; private set; }

    private void OnDestroy() {
        buyButton.onClick.RemoveAllListeners();
    }
    
    public void UpdateInfo(ItemData itemData, bool isSold) {
        item = itemData;
        iconImage.sprite = itemData.itemIcon;
        iconImage.color = Color.white;
        iconBorder.color = itemData.rarity.Colour();
        nameText.text = $"{itemData.itemName}<size=40%>\n\n</size>";
        rarityText.text = $"<style={itemData.rarity.ToString()}>{itemData.rarity.ToString()}</style>";
        descriptionText.text = itemData.itemDescription;
        SetSold(isSold);
    }

    public void SetSold(bool sold) {
        isSold = sold;
        buyButton.interactable = !sold;
        priceText.text = sold ? "SOLD" : $"${item.value.ToString()}";
        priceText.fontStyle = sold ? FontStyles.Bold : FontStyles.Normal;
        priceText.color = sold ? Color.red : Color.white;
    }
}