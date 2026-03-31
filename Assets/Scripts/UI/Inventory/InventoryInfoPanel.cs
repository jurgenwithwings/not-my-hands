using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInfoPanel : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Image iconBorder;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button recycleButton;
    [SerializeField] private TMP_Text recycleText;

    private ItemData itemData;
    
    public LimbSide LastLimbSideClicked { get; private set; } = LimbSide.Left;
    
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
        recycleText.text = "Recycle";
        recycleButton.onClick.RemoveAllListeners();
        recycleButton.interactable = false;
    }
    
    public void UpdateInfo(ItemData itemData) {
        this.itemData = itemData;
        
        iconImage.sprite = itemData.itemIcon;
        iconImage.color = Color.white;
        iconBorder.color = itemData.rarity.Colour();
        nameText.text = $"{itemData.itemName}<size=40%>\n\n</size>" +
                        $"<size=80%><style={itemData.rarity.ToString()}>{itemData.rarity.ToString()}</size></style><size=40%>\n\n</size>" +
                        $"<style=Flavour>{itemData.itemFlavourText}</style>";
        descriptionText.text = itemData.itemDescription;
        recycleText.text = $"${itemData.RecycleValue:0} Recycle";
        recycleButton.interactable = true;
        recycleButton.onClick.RemoveAllListeners();
        recycleButton.onClick.AddListener(RecycleButtonClicked);
    }
    
    public void SetLastLimbPressed(LimbSide limbSide) {
        LastLimbSideClicked = limbSide;
    }

    private void RecycleButtonClicked() {
        if (itemData.GetType() == typeof(LimbData)) {
            PlayerHUDEvents.OnLimbRecycleRequest?.Invoke((LimbData)itemData, LastLimbSideClicked);
        }
        else {
            PlayerHUDEvents.OnRecycleRequest?.Invoke(itemData);
        }
        Clear();
    }
}