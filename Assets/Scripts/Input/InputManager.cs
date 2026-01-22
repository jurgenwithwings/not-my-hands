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
    public static PlayerControls controls { get; private set; }
    
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

        EnableActionMap(InputMap.Player);

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
                CurrentControlDevice = ControlType.Controller;
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
        if (Interact.Context.performed) {
            return;
        }
        if (context.performed || context.canceled) {
            PrimaryFire.SetAndInvoke(context.performed, context.performed, context);
        }
    }

    public InputEvent<bool> SecondaryFire;
    public void OnSecondaryFire(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            return;
        }
        if (context.performed || context.canceled) {
            SecondaryFire.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> PrimaryKick;
    public void OnPrimaryKick(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            return;
        }
        if (context.performed || context.canceled) {
            PrimaryKick.SetAndInvoke(context.performed, context.performed, context);
        }
    }
    
    public InputEvent<bool> SecondaryKick;
    public void OnSecondaryKick(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (Interact.Context.performed) {
            return;
        }
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

    public InputEvent<bool> Pause;
    public void OnPause(InputAction.CallbackContext context) {
        DefaultHandle(context);
        if (context.performed || context.canceled) {
            Pause.SetAndInvoke(context.performed, context.performed, context);
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

public struct CachedGlyph {
    private readonly string actionName;
    private ControlType controlType;
    private OnScreenControllerUI controllerSetting;
    private string glyph;

    /// <param name="name">Provide this in the format "[ActionMap]/[ActionName]" e.g "Player/Interact"</param>
    public CachedGlyph(string name) {
        actionName = name;
        controlType = InputManager.CurrentControlDevice;
        controllerSetting = PlayerSettings.controllerUI;
        glyph = "";
    }
    
    public string Glyph(bool includeModifier = true) {
        if (string.IsNullOrEmpty(glyph)) {
            return Get(includeModifier);
        }
        if (InputManager.CurrentControlDevice == controlType && PlayerSettings.controllerUI == controllerSetting) {
            return glyph;
        }
        return Get(includeModifier);
    }
        
    private string Get(bool includeModifier) {
        controlType = InputManager.CurrentControlDevice;
        controllerSetting = PlayerSettings.controllerUI;
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
                string prefix = "XB";
                switch (PlayerSettings.controllerUI) {
                    case OnScreenControllerUI.Xbox:
                        prefix = "XB";
                        break;
                    case OnScreenControllerUI.PlayStation:
                        prefix = "PS";
                        break;
                    case OnScreenControllerUI.Switch:
                        prefix = "SW";
                        break;
                }
                control = prefix + controlPath; break;
        }

        //PlayerHUDEvents.DebugText($"Index: {index} | Action: {action} | Layout: {deviceLayoutName} | ControlPath: {controlPath} | Control: {control}", 10, key:$"q{index}");
        
        if (!String.IsNullOrEmpty(control)) {
            return $"<sprite name=\"{control}\">";
        }
        
        return action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions, InputManager.CurrentControlDevice.ToString());
    }
}
