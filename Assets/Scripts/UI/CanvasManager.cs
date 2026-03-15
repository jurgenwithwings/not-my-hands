using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public enum MenuType {
    None,
    Inventory,
    Shop,
}

public abstract class Menu : MonoBehaviour {
    public abstract void OpenMenu();
    public abstract bool CloseMenu();
}

public class CanvasManager : MonoBehaviour {
    public static CanvasManager Instance { get; private set; }
    public Canvas MainCanvas { get; private set; }
    [SerializeField] private GamepadCursor gamepadCursor;
    
    [SerializeField] private SerializedDictionary<MenuType, Menu> Menus = new();
    
    private MenuType currentMenu = MenuType.None;
    
    public static Action<MenuType> OnMenuOpened;
    public static Action<MenuType> OnMenuClosed;

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
            return;
        }
        
        MainCanvas = GetComponent<Canvas>();
        if (MainCanvas != null) {
            Instance = this;
        }
    }

    private void Start() {
        InputManager.Instance.UIExit.Event += CloseCurrentMenu;
    }

    private void CloseCurrentMenu(InputEvent<bool> input) {
        if (input.Triggered && CanCloseMenu(currentMenu)) {
            InputManager.Instance.EnableActionMap(InputMap.Player);
        }
    }
    

    public bool CanOpenMenu(MenuType menu) {
        if (currentMenu == menu) {
            return false;
        }

        return currentMenu == MenuType.None;
    }

    public Menu OpenMenu(MenuType menu) {
        if (!CanOpenMenu(menu)) return null;
        InputManager.Instance.EnableActionMap(InputMap.UI);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        gamepadCursor.ToggleCursor(true);
        Menus[menu].OpenMenu();
        currentMenu = menu;
        OnMenuOpened?.Invoke(menu);
        return Menus[menu];
    }

    public bool CanCloseMenu(MenuType menu) {
        if (Menus[menu].CloseMenu()) {
            InputManager.Instance.EnableActionMap(InputMap.Player);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
            currentMenu = MenuType.None;
            gamepadCursor.ToggleCursor(false);
            OnMenuClosed?.Invoke(menu);
            return true;
        }
        return false;
    }
}
