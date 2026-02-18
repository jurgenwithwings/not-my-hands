using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Image border;
    [SerializeField] private TMP_Text countText;
    
    public ItemData itemData { get; private set; }

    public enum ItemType {
        Relic,
        Limb,
        Organ
    }
    
    private ItemType itemType;
    private int itemAmount;

    public Button Set(ItemData itemData, ItemType itemType) {
        this.itemData = itemData;
        icon.sprite = itemData.itemIcon;
        border.color = itemData.rarity.Colour();
        this.itemType = itemType;
        return button;
    }

    public void Add(int count = 1) {
        itemAmount += count;

        if (itemAmount <= 0) {
            button.onClick.RemoveAllListeners();
            Destroy(gameObject);
        }
        
        //countText.text = $"x{itemAmount}";
    }
}