using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using static PlayerControls;

public enum InputMap {
    Player,
    UI
}

public enum ControlType {
    Keyboard,
    Gamepad
}

public struct InputEvent<T>{
    public T Value;
    public T RawValue;
    public bool Triggered;
    public bool preformedLastFrame;
    
    public event Action<InputEvent<T>> Event;
    
    public InputAction.CallbackContext Context;
    
    public void Invoke() {
        Event?.Invoke(this);
    }

    public void Set(T value, T rawValue, InputAction.CallbackContext context) {
        Triggered = !preformedLastFrame && context.performed;
        preformedLastFrame = context.performed;
        Value = value;
        RawValue = rawValue;
        Context = context;
    }
    
    public void SetAndInvoke(T value, T rawValue, InputAction.CallbackContext context) {
        Set(value, rawValue, context);
        Invoke();
    }

    public void ResetState() {
        Triggered = false;
    }

    public static implicit operator T(InputEvent<T> inputEvent) {
        return inputEvent.Value;
    }
}

public class InputManager : MonoBehaviour, IPlayerActions, IUIActions {
    private PlayerControls controls;
    
    private List<InputEvent<Type>> inputFields;

    public static event Action<ControlType> OnControlTypeChanged;
    public static ControlType CurrentControlDevice { get; private set; }

    public void OnEnable() => EnablePlayerActions();
    
    public void EnablePlayerActions() {
        if (controls == null) {
            controls = new PlayerControls();
            
            controls.Player.SetCallbacks(this);
            controls.UI.SetCallbacks(this);
        }
        
        controls.Player.Enable();

        SetUIModuleAsset();
    }

    private void SetUIModuleAsset() {
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null) {
            return;
        }

        var uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (uiModule == null) {
            return;
        }

        if (uiModule.actionsAsset != controls.asset) {
            uiModule.actionsAsset = controls.asset;
        }
    }
    
    public void EnableActionMap(InputMap map) {
        controls.Disable();
        switch (map) {
            case InputMap.Player:
                controls.Player.Enable();
                break;
            case InputMap.UI:
                controls.UI.Enable();
                break;
            default:
                controls.Enable();
                break;
        }
    }

    private void GetCurrentInputDevice(InputAction.CallbackContext context) {
        ControlType controlType = CurrentControlDevice;
        switch (context.control.device) {
            case Gamepad:
                CurrentControlDevice = ControlType.Gamepad;
                break;
            case Mouse or Keyboard:
                CurrentControlDevice = ControlType.Keyboard;
                break;
        }

        if (controlType != CurrentControlDevice) {
            OnControlTypeChanged?.Invoke(CurrentControlDevice);
        }
    }

    // All Inputs Must be added to here to reset properly
    private void LateUpdate() {
        Move.ResetState();
        Look.ResetState();
        Jump.ResetState();
        Interact.ResetState();
        PrimaryInteract.ResetState();
        SecondaryInteract.ResetState();
        PrimaryFire.ResetState();
        SecondaryFire.ResetState();
        PrimaryKick.ResetState();
        SecondaryKick.ResetState();
        SwapArms.ResetState();
        SwapLegs.ResetState();
        Inventory.ResetState();
        Recycle.ResetState();
    }
    
    private void Update() {
        //PlayerHUDEvents.DebugText(CurrentControlDevice.ToString(), key: "0");
        //PlayerHUDEvents.DebugText(PrimaryFire.Triggered.ToString(), key: "1");
        //PlayerHUDEvents.DebugText(PrimaryFire.preformedLastFrame.ToString(), key: "2");
    }

    public void DefaultHandle(InputAction.CallbackContext context) {
        GetCurrentInputDevice(context);
    }

    // Player Actions
    public InputEvent<Vector2> Move;
    public void OnMove(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Move.SetAndInvoke(context.ReadValue<Vector2>(), context.ReadValue<Vector2>(), context);
        }
    }

    public InputEvent<Vector2> Look;
    public void OnLook(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Vector2 look = context.ReadValue<Vector2>() * (CurrentControlDevice == ControlType.Keyboard ? PlayerSettings.MouseSensitivity : PlayerSettings.ControllerSensitivity);
            if (PlayerSettings.IsInvertedMouseY) {
                look.y *= -1;
            }
            Look.SetAndInvoke(look, context.ReadValue<Vector2>(), context);
        }
    }

    public InputEvent<bool> Jump;
    public void OnJump(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Jump.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> Interact;
    public void OnInteract(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Interact.SetAndInvoke(context.performed, context.performed, context);
        }
    }

    public InputEvent<bool> PrimaryInteract;
    public void OnPrimaryInteract(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            PrimaryInteract.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> SecondaryInteract;
    public void OnSecondaryInteract(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            SecondaryInteract.SetAndInvoke(context.performed, context.performed, context);
        }
    }

    public InputEvent<bool> PrimaryFire;
    public void OnPrimaryFire(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            PrimaryFire.SetAndInvoke(context.performed, context.performed, context);
        }
    }

    public InputEvent<bool> SecondaryFire;
    public void OnSecondaryFire(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            SecondaryFire.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> PrimaryKick;
    public void OnPrimaryKick(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            PrimaryKick.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> SecondaryKick;
    public void OnSecondaryKick(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            SecondaryKick.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> SwapArms;
    public void OnSwapArms(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            SwapArms.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> SwapLegs;
    public void OnSwapLegs(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            SwapLegs.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> Inventory;
    public void OnInventory(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Inventory.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> Recycle;
    public void OnRecycle(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Recycle.SetAndInvoke(context.performed, context.performed, context);
        }
    }


    // UI Actions
    public void OnNavigate(InputAction.CallbackContext context) { }
    public void OnSubmit(InputAction.CallbackContext context) { }
    public void OnCancel(InputAction.CallbackContext context) { }
    public void OnPoint(InputAction.CallbackContext context) { }
    public void OnClick(InputAction.CallbackContext context) { }
    public void OnRightClick(InputAction.CallbackContext context) { }

    public void OnMiddleClick(InputAction.CallbackContext context) {
        if (context.performed) {
            EnableActionMap(InputMap.Player);
        }
    }
    public void OnScrollWheel(InputAction.CallbackContext context) { }
}
