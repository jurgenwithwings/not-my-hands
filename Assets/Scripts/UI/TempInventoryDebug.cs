using UnityEngine;

public class TempInventoryDebug : MonoBehaviour {
    public RectTransform Background;

    public void Start() {
        PlayerHUDEvents.OnDoTheInventory += ToggleInventory;
        Background.anchoredPosition = Vector2.up * 10000f;
    }

    private void ToggleInventory(bool isOn) {
        Background.anchoredPosition = Vector2.up * (isOn ? 0f : 10000f);
        Time.timeScale = isOn ? 0f : 1f;
    }
}
