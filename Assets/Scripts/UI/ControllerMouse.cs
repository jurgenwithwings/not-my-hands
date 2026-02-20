using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControllerMouse : MonoBehaviour {
    [SerializeField] private Image pointer;

    private bool active = false;
    
    private void Start() {
        InputManager.OnControlTypeChanged += OnControlTypeChanged;
        
        PlayerHUDEvents.OnDoTheInventory += OnDoTheInventory;

        //InputManager.Instance.UIPointer.Event += inputEvent => MoveCursor(inputEvent);
    }

    private void OnDoTheInventory(bool enable) {
        pointer.enabled = enable;
        active = enable;
    }

    private void OnControlTypeChanged(ControlType type) {
        if (type == ControlType.Controller) {
            pointer.enabled = true;
            active = true;
        }
        else {
            pointer.enabled = false;
            active = false;
        }
    }
    
    
    [Header("Movement")]
    public float speed = 1400f;
    public float acceleration = 10f;
    public float friction = 14f;
    public float deadzone = 0.05f;

    [Header("Scroll")]
    public float scrollSpeed = 35f;

    Vector2 velocity;

    void Update()
    {
        if (!active || InputManager.CurrentControlDevice != ControlType.Controller) return;
        
        if (Gamepad.current == null || Mouse.current == null) return;
        
        
        MoveCursor(InputManager.Instance.UIPointer.Value);
        
        
        //HandleButtons();
        //HandleScroll();
    }

    void MoveCursor(Vector2 input) {
        // Deadzone
        if (input.magnitude < deadzone) {
            input = Vector2.zero;            
        }

        Vector2 delta = input * speed * Time.unscaledDeltaTime;
        
        pointer.rectTransform.anchoredPosition += delta;
    }
}