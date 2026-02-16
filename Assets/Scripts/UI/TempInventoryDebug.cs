using UnityEngine;

public class TempInventoryDebug : MonoBehaviour {
    public GameObject Background;

    public void Start() {
        PlayerHUDEvents.OnDoTheInventory += ToggleInventory;
        Background.SetActive(false);
    }

    private void ToggleInventory(bool isOn) {
        Background.SetActive(isOn);
    }
}
