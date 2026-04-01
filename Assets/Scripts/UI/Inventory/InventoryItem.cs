using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Image border;
    [SerializeField] private TMP_Text countText;
    
    public ItemData itemData { get; private set; }
    public int Count { get; private set; } = 1;
    public Button Button => button;

    public Button Set(ItemData itemData) {
        this.itemData = itemData;
        icon.sprite = itemData.itemIcon;
        border.color = itemData.rarity.Colour();
        countText.text = "";
        return button;
    }

    public void Add(int count = 1) {
        Count += count;

        if (Count <= 0) {
            button.onClick.RemoveAllListeners();
            Destroy(gameObject);
        }
        
        countText.text = Count <= 1 ? "" : $"x{Count}";
    }
}