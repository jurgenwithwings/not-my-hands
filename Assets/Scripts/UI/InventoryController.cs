using UnityEngine;

public class InventoryController : Menu {
    [SerializeField] private RectTransform Background;

    public void Start() {
        Background.anchoredPosition = Vector2.up * 10000f;
    }

    public override void OpenMenu() {
        Background.anchoredPosition = Vector2.zero;
        
    }
    public override bool CloseMenu() {
        Background.anchoredPosition = Vector2.up * 10000f;
        return true;
    }
}
