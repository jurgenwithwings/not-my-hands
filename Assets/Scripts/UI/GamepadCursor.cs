using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class GamepadCursor : MonoBehaviour {
    [SerializeField] private Image cursor;
    [SerializeField] private float speed = 1000f;
    [SerializeField] private float padding = 35f;
    
    private Mouse virtualMouse;
    private RectTransform canvasRect;
    
    private bool previousMouseState;

    private bool active;

    private bool menuOpen;

    private void Awake() {
        canvasRect = GetComponent<RectTransform>();
    }

    private void Start() {
        // Creating Virtual Mouse
        if (virtualMouse == null) {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouse.added) {
            InputSystem.AddDevice(virtualMouse);
        }

        if (cursor != null) {
            Vector2 position = cursor.rectTransform.anchoredPosition;
            InputState.Change(virtualMouse.position, position);
        }
        
        OnDoTheInventory(false);
    }

    private void OnEnable() {
        InputSystem.onAfterUpdate += UpdateMotion; // Pointer becomes stuck at bottom left of screen if using normal Update().
        
        InputManager.OnControlTypeChanged += InputManagerOnOnControlTypeChanged;
        
        PlayerHUDEvents.OnDoTheInventory += OnDoTheInventory;
    }

    private void OnDisable() {
        InputSystem.RemoveDevice(virtualMouse);
        
        InputSystem.onAfterUpdate -= UpdateMotion;
        InputManager.OnControlTypeChanged -= InputManagerOnOnControlTypeChanged;
        PlayerHUDEvents.OnDoTheInventory -= OnDoTheInventory;
    }

    private void InputManagerOnOnControlTypeChanged(ControlType type) {
        if (!menuOpen) return;
        switch (type) {
            case ControlType.Keyboard:
                ToggleGamepadCursor(false);
                break;
            
            case ControlType.Controller:
                ToggleGamepadCursor(true);
                break;
        }
    }

    private void OnDoTheInventory(bool active) {
        menuOpen = active;
        if (active && InputManager.CurrentControlDevice == ControlType.Controller) {
            ToggleGamepadCursor(true);
        }
        else {
            ToggleGamepadCursor(false);
        }
        
        if (active) {
            // Set position of Cursor to current mouse position on Inventory Open
            InputState.Change(virtualMouse.position, Mouse.current.position.ReadValue());
            AnchorCursor(Mouse.current.position.ReadValue());
        }
        else {
            // Reset Left Click on Inventory Close
            virtualMouse.CopyState<MouseState>(out var mouseState);
            mouseState.WithButton(MouseButton.Left, false);
            InputState.Change(virtualMouse, mouseState);
        }
    }

    private void ToggleGamepadCursor(bool active) {
        cursor.gameObject.SetActive(active);
        Cursor.visible = !active;
        
        if (active) {
            InputState.Change(virtualMouse.position, Mouse.current.position.ReadValue());
            AnchorCursor(Mouse.current.position.ReadValue());
        }
        else {
            Mouse.current.WarpCursorPosition(virtualMouse.position.ReadValue());
        }
        
        this.active = active;
    }
    
    private void UpdateMotion() {
        if (!active || !menuOpen) return;
        
        if (virtualMouse == null || Gamepad.current == null || InputManager.CurrentControlDevice != ControlType.Controller) return;
        
        Vector2 stickValue = InputManager.Instance.UIControllerCursorDriver.Value;
        
        stickValue *= speed * Time.unscaledDeltaTime;
        
        Vector2 position = virtualMouse.position.ReadValue();
        Vector2 newPosition = position + stickValue;
        
        newPosition.x = Mathf.Clamp(newPosition.x, padding, Screen.width - padding);
        newPosition.y = Mathf.Clamp(newPosition.y, padding, Screen.height - padding);
        
        // Cursor
        InputState.Change(virtualMouse.position, newPosition);
        InputState.Change(virtualMouse.delta, stickValue);
        
        // Scroll Wheel
        InputState.Change(virtualMouse.scroll.x, InputManager.Instance.UIControllerScrollDriver.Value.x);
        InputState.Change(virtualMouse.scroll.y, InputManager.Instance.UIControllerScrollDriver.Value.y);

        bool aButtonPressed = InputManager.Instance.UIClick.Value;
        if (previousMouseState != aButtonPressed) {
            virtualMouse.CopyState<MouseState>(out var mouseState);
            mouseState.WithButton(MouseButton.Left, aButtonPressed);
            InputState.Change(virtualMouse, mouseState);
            previousMouseState = aButtonPressed;
        }

        AnchorCursor(newPosition);
    }

    private void AnchorCursor(Vector2 position) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, position, null, out Vector2 anchoredPosition);
        
        cursor.rectTransform.anchoredPosition = anchoredPosition;
    }
}