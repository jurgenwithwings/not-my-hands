using System;
using System.Collections;
using System.Collections.Generic;
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
    Controller
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
    public static InputManager Instance { get; private set; }
    
    public static PlayerControls controls { get; private set; }
    
    private List<InputEvent<Type>> inputFields;

    public static event Action<ControlType> OnControlTypeChanged;
    public static ControlType CurrentControlDevice { get; private set; }

    private void Awake() {
        Instance = this;
        
        if (controls == null) {
            controls = new PlayerControls();
            
            controls.Player.SetCallbacks(this);
            controls.UI.SetCallbacks(this);
        }
    }
    
    private void Start() => StartCoroutine(EnablePlayerActions());
    
    private IEnumerator EnablePlayerActions() {
        SetUIModuleAsset();
        
        yield return null;
        
        EnableActionMap(InputMap.Player);
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
        //print(context.control.device.displayName);
        switch (context.control.device.displayName) {
            case "Gamepad" or "Wireless Controller" or "VirtualMouse":
                CurrentControlDevice = ControlType.Controller; break;
            
            case "Mouse" or "Keyboard":
                CurrentControlDevice = ControlType.Keyboard; break;
            
            default: // Fallback to device types if name is not found.
                switch (context.control.device) {
                    case Gamepad:
                        CurrentControlDevice = ControlType.Controller; break;
                    
                    case Mouse:
                        CurrentControlDevice = ControlType.Keyboard; break;
                    
                    default:
                        CurrentControlDevice = ControlType.Keyboard; break;
                }
                break;
        }

        if (controlType != CurrentControlDevice) {
            OnControlTypeChanged?.Invoke(CurrentControlDevice);
        }
    }

    // All Inputs Must be added to here to reset properly
    private void LateUpdate() {
        // Player
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
        Pause.ResetState();
        
        // UI
        UINavigate.ResetState(); 
        UISubmit.ResetState();
        UICancel.ResetState();
        UIPointer.ResetState();
        UIClick.ResetState();
        UIRightClick.ResetState();
        UIMiddleClick.ResetState();
        UIScrollWheel.ResetState();
        UIExit.ResetState();
        
        // Controller UI Drivers
        UIControllerCursorDriver.ResetState();
        UIControllerScrollDriver.ResetState();
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

    public InputEvent<float> PrimaryFire;
    public void OnPrimaryFire(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            PrimaryFire.SetAndInvoke(0, 0, context);
            return;
        }
        float value = context.ReadValue<float>();
        PrimaryFire.SetAndInvoke(value, value, context);
    }

    public InputEvent<float> SecondaryFire;
    public void OnSecondaryFire(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            SecondaryFire.SetAndInvoke(0, 0, context);
            return;
        }
        float value = context.ReadValue<float>();
        if (context.performed || context.canceled) {
            SecondaryFire.SetAndInvoke(value, value, context);
        }
    }
    
    public InputEvent<float> PrimaryKick;
    public void OnPrimaryKick(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            PrimaryKick.SetAndInvoke(0, 0, context);
            return;
        }
        float value = context.ReadValue<float>();
        if (context.performed || context.canceled) {
            PrimaryKick.SetAndInvoke(value, value, context);
        }
    }
    
    public InputEvent<float> SecondaryKick;
    public void OnSecondaryKick(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            SecondaryKick.SetAndInvoke(0, 0, context);
            return;
        }
        float value = context.ReadValue<float>();
        if (context.performed || context.canceled) {
            SecondaryKick.SetAndInvoke(value, value, context);
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

    public InputEvent<bool> Pause;
    public void OnPause(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Pause.SetAndInvoke(context.performed, context.performed, context);
        }
    }


    // UI Actions
    public InputEvent<Vector2> UINavigate;
    public void OnNavigate(InputAction.CallbackContext context) {
        //print("UINavigate");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UINavigate.SetAndInvoke(context.ReadValue<Vector2>(), context.ReadValue<Vector2>(), context);
        }
    }
    
    public InputEvent<bool> UISubmit;
    public void OnSubmit(InputAction.CallbackContext context) {
        //print("UISubmit");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UISubmit.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> UICancel;
    public void OnCancel(InputAction.CallbackContext context) {
        //print("UICancel");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UICancel.SetAndInvoke(context.performed, context.performed, context);
        }
    }

    public InputEvent<Vector2> UIPointer;
    public void OnPoint(InputAction.CallbackContext context) {
        //print("UIPointer");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIPointer.SetAndInvoke(context.ReadValue<Vector2>(), context.ReadValue<Vector2>(), context);
        }
    }
    
    public InputEvent<bool> UIClick;
    public void OnClick(InputAction.CallbackContext context) {
        //print("UIClick");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIClick.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> UIRightClick;
    public void OnRightClick(InputAction.CallbackContext context) {
        //print("UIRightClick");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIRightClick.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> UIMiddleClick;
    public void OnMiddleClick(InputAction.CallbackContext context) {
        //print("UIMiddleClick");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIMiddleClick.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<Vector2> UIScrollWheel;
    public void OnScrollWheel(InputAction.CallbackContext context) {
        //print("UIScrollWheel");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIScrollWheel.SetAndInvoke(context.ReadValue<Vector2>(), context.ReadValue<Vector2>(), context);
        }
    }

    public InputEvent<bool> UIExit;
    public void OnExit(InputAction.CallbackContext context) {
        //print("UIExit");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIExit.SetAndInvoke(context.performed, context.performed, context);
            EnableActionMap(InputMap.Player);
            
            PlayerHUDEvents.OnDoTheInventory.Invoke(false);
        }
    }
    
    // Controller UI Drivers
    public InputEvent<Vector2> UIControllerCursorDriver;
    public void OnControllerCursorDriver(InputAction.CallbackContext context) {
        //print("UIControllerCursorDriver");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIControllerCursorDriver.SetAndInvoke(context.ReadValue<Vector2>(), context.ReadValue<Vector2>(), context);
        }
    }

    public InputEvent<Vector2> UIControllerScrollDriver;
    public void OnControllerScrollDriver(InputAction.CallbackContext context) {
        //print("UIControllerScrollDriver");
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            UIControllerScrollDriver.SetAndInvoke(context.ReadValue<Vector2>(), context.ReadValue<Vector2>(), context);
        }
    }
}

public struct CachedGlyph {
    private readonly string actionName;
    private ControlType controlType;
    private OnScreenControllerUI controllerSetting;
    private string glyph;

    /// <param name="name">Provide this in the format "[ActionMap]/[ActionName]" e.g "Player/Interact"</param>
    public CachedGlyph(string name) {
        actionName = name;
        controlType = InputManager.CurrentControlDevice;
        controllerSetting = PlayerSettings.ControllerUI;
        glyph = "";
    }
    
    public string Glyph(bool includeModifier = true) {
        if (string.IsNullOrEmpty(glyph)) {
            return Get(includeModifier);
        }
        if (InputManager.CurrentControlDevice == controlType && PlayerSettings.ControllerUI == controllerSetting) {
            return glyph;
        }
        return Get(includeModifier);
    }
        
    private string Get(bool includeModifier) {
        controlType = InputManager.CurrentControlDevice;
        controllerSetting = PlayerSettings.ControllerUI;
        glyph = InputManager.controls.FindAction(actionName, true).Glyph(includeModifier);
        return glyph;
    }

    public void Refresh() {
        Get(true);
    }

    public static implicit operator string(CachedGlyph glyph) {
        return glyph.glyph;
    }
}

public static class InputManagerExtensions {
    /// <summary>
    /// Looks up the current Binding for the given action, in glyph form.
    /// If calling frequently, like on <c>Update</c>, use the <c>CashedGlyph</c> to save on string lookups.
    /// </summary>
    /// <param name="action">The action to find the glyph for.</param>
    /// <param name="includeModifier">If action binding has a modifier, whether to include that in the output.</param>
    /// <returns>The inline text glyph fot the InputAction</returns>
    public static string Glyph(this InputAction action, bool includeModifier = true) {
        if (action.bindings[0].isComposite) {
            var parts = new List<string>();

            for (int i = 1; i < action.bindings.Count; i++) {
                if (string.IsNullOrEmpty(action.bindings[i].groups)) {
                    continue;
                }
                if (!action.bindings[i].groups.Contains(InputManager.CurrentControlDevice.ToString())) {
                     continue;   
                }
                
                parts.Add(action.BindingToGlyph(i));

                parts[^1] = parts.Count switch {
                    1 => parts[^1] + " + ",
                    > 2 => " or " + parts[^1],
                    _ => parts[^1]
                };
            }

            if (parts.Count > 2) {
                parts[1] = "(" + parts[1];
                parts[^1] = $"{parts[^1]})";
            }
            if (parts.Count > 0 && !includeModifier) {
                parts.RemoveAt(0);
            }
            
            return string.Join("", parts);
        }
        else {
            return action.BindingToGlyph();
        }
    }

    private static string BindingToGlyph(this InputAction action, int? index = null) {
        index ??= action.GetBindingIndex(InputManager.CurrentControlDevice.ToString());
        
        action.GetBindingDisplayString(index.Value, out var deviceLayoutName, out var controlPath);
        
        string control = "";
            
        switch (InputManager.CurrentControlDevice) {
            case ControlType.Keyboard:
                control = controlPath; break;
            case ControlType.Controller:
                string prefix = PlayerSettings.ControllerUI switch {
                    OnScreenControllerUI.Xbox => "XB",
                    OnScreenControllerUI.PlayStation => "PS",
                    OnScreenControllerUI.Switch => "SW",
                    _ => "XB"
                };
                control = prefix + controlPath; break;
        }

        //PlayerHUDEvents.DebugText($"Index: {index} | Action: {action} | Layout: {deviceLayoutName} | ControlPath: {controlPath} | Control: {control}", 10, key:$"q{index}");
        
        if (!String.IsNullOrEmpty(control)) {
            return $"<sprite name=\"{control}\">";
        }
        
        return action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions, InputManager.CurrentControlDevice.ToString());
    }
}
