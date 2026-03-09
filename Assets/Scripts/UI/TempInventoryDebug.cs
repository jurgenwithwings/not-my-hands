using UnityEngine;

public class TempInventoryDebug : MonoBehaviour {
    [SerializeField] private RectTransform Background;
    [SerializeField] private UIEventHandler eventHandler;

    public void Start() {
        PlayerHUDEvents.OnDoTheInventory += ToggleInventory;
        Background.anchoredPosition = Vector2.up * 10000f;
    }

    private void ToggleInventory(bool isOn) {
        float position = 10000f;
        float timeScale = 1f;
        InputMap activeInput = InputMap.Player;
        CursorLockMode cursorMode = CursorLockMode.Locked;
        
        if (isOn) {
            position = 0f;
            timeScale = 0f;
            activeInput = InputMap.UI;
            cursorMode = CursorLockMode.None;
        }
        
        Background.anchoredPosition = Vector2.up * position;
        Time.timeScale = timeScale;
        InputManager.Instance.EnableActionMap(activeInput);
        Cursor.lockState = cursorMode;
        Cursor.visible = isOn;
        
        eventHandler.OnUIToggled?.Invoke(isOn);
    }
}
