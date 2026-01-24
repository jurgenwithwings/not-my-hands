using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryInterface : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject itemPanelPrefab;
    
    [Header("Info")]
    [SerializeField] private InventoryInfoPanel infoPanel;
    
    [Header("Relics")]
    [SerializeField] private GameObject relicsPanel;
    [SerializeField] private List<InventoryItem> relics = new();
    
    [Header("Limbs")]
    [SerializeField] private List<InventoryItem> limbs = new();
    
    [Header("Organs")]
    [SerializeField] private List<InventoryItem> organs = new();

    private void Start() {
        PlayerHUDEvents.OnAddedRelic += AddRelic;
        PlayerHUDEvents.OnUpdateLimb += UpdateLimb;
        PlayerHUDEvents.OnUpdateOrgan += UpdateOrgan;
    }
    
    private void AddRelic(RelicData relicData) {
        InventoryItem iItem = relics.Find(i => i.itemData == relicData);

        if (iItem == null) {
            iItem = Instantiate(itemPanelPrefab, relicsPanel.transform).GetComponent<InventoryItem>();
            iItem.Set(relicData, InventoryItem.ItemType.Relic).onClick.AddListener(() => infoPanel.UpdateInfo(relicData));
            relics.Add(iItem);
        }
        else {
            iItem.Add();
        }
    }

    private void UpdateLimb(LimbData limbData, LimbSide limbSide) {
        int limbSideCount = Enum.GetNames(typeof(LimbSide)).Length;
        int index = (((int)limbData.limbType * limbSideCount) + (int)limbSide) - limbSideCount + 1; // Converts the Enum indexes to a unique number from 0-n grouped by type.

        Button button = limbs[index].Set(limbData, InventoryItem.ItemType.Limb);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => infoPanel.UpdateInfo(limbData));
    }

    private void UpdateOrgan(OrganData organData) {
        int type = (int)organData.type;

        Button button = organs[type].Set(organData, InventoryItem.ItemType.Organ);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => infoPanel.UpdateInfo(organData));
    }
}