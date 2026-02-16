using System;
using UnityEngine;
using Object = UnityEngine.Object;

public enum LimbType {
    Arm = 1,
    Leg = 2,
}

public enum LimbSide {
    Left = 1,
    Right = 2,
}

public class LimbManager : MonoBehaviour {
    [SerializeField] private Limb[] limbs;
    [SerializeField] private Transform[] limbAnchors;
    
    private Statboard statboard;
    
    public bool IsArmBusy => limbs[0].IsBusy || limbs[1].IsBusy;
    public bool IsLegBusy => limbs[2].IsBusy || limbs[3].IsBusy;
    
    // Input
    private InputManager inputManager;
    private Action<InputEvent<float>> primaryFireEvent;
    private Action<InputEvent<float>> secondaryFireEvent;
    private Action<InputEvent<float>> primaryKickEvent;
    private Action<InputEvent<float>> secondaryKickEvent;
    
    // Interact Anims
    private Action<InputEvent<bool>> interactEvent;

    private void Awake() {
        inputManager = GetComponent<InputManager>();
        
        primaryFireEvent = input => InputEvent(1, input);
        inputManager.PrimaryFire.Event += primaryFireEvent;
        
        secondaryFireEvent = input => InputEvent(0, input);
        inputManager.SecondaryFire.Event += secondaryFireEvent;
        
        primaryKickEvent = input => InputEvent(3, input);
        inputManager.PrimaryKick.Event += primaryKickEvent;
        
        secondaryKickEvent = input => InputEvent(2, input);
        inputManager.SecondaryKick.Event += secondaryKickEvent;

        statboard = GetComponent<Statboard>();
        
        for (int i = 0; i < limbAnchors.Length; i++) {
            limbs[i] = LimbHelper.CreateDefaultLimb((LimbType)((int)(i * 0.5f) + 1), limbAnchors[i]);
            limbs[i].Initialise(LimbHelper.LoadLimbData(limbs[i].data.limbType), this, statboard);
        }
        
        statboard.moveSpeed.BaseValue = ((Leg)limbs[2]).MoveSpeed + ((Leg)limbs[3]).MoveSpeed;
    }

    public void HandleInteractAnim() {
        if (!limbs[1].PlayInteractionAnim()) {
            limbs[0].PlayInteractionAnim();
        }
    }

    private void OnDestroy() {
        inputManager.PrimaryFire.Event -= primaryFireEvent;
        inputManager.PrimaryKick.Event -= primaryKickEvent;
        inputManager.SecondaryFire.Event -= secondaryFireEvent;
        inputManager.SecondaryKick.Event -= secondaryKickEvent;
    }

    private void InputEvent(int targetLimb, InputEvent<float> input) {
        //PlayerHUDEvents.DebugText($"Input Sent To: {targetLimb}");
        limbs[targetLimb].ReceiveInput(input);
    }

    public void AddLimb(LimbData limbData, LimbSide limbSide) {
        int index = limbData.limbType == LimbType.Arm ? 0 : 1;
        if (limbSide == LimbSide.Right) {
            index++;
        }
        
        Limb old = limbs[index];
        if (old != null) {
            //Drop the last limb held
            Instantiate(old.data.prefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(transform.forward + (transform.up * 0.4f) * 2f);
            
            //Destroy the physical limb from the player
            Destroy(limbs[index]?.gameObject);
        }
        
        //Create the new physical limb for the player
        limbs[index] = Instantiate(limbData.limbPrefab, limbAnchors[index].position, limbAnchors[index].rotation, limbAnchors[index]).GetComponent<Limb>();
        limbs[index].Initialise(limbData, this, statboard);

        if (limbData.limbType == LimbType.Leg) {
            statboard.moveSpeed.BaseValue -= ((Leg)old).MoveSpeed;
            statboard.moveSpeed.BaseValue += ((Leg)limbs[index]).MoveSpeed;
        }
    }
}




public static class LimbHelper {
    private static string LimbDataPath = "DefaultLimbs/";
    
    public static LimbData LoadLimbData(LimbType type) {
        return Resources.Load<LimbData>(LimbDataPath + type.ToString());
    }
    
    public static Limb CreateDefaultLimb(LimbType limbType, Transform limbAnchor) {
        Limb newLimb = Object.Instantiate(LoadLimbData(limbType).limbPrefab, limbAnchor.position, limbAnchor.rotation, limbAnchor).GetComponent<Limb>();
        return newLimb;
    }
}




public abstract class Limb : MonoBehaviour {
    public LimbData data;
    protected Statboard statboard;
    protected LimbManager manager;
    
    protected Animator animator;
    protected static readonly int AnimIdle = Animator.StringToHash("Idle");
    protected static readonly int AnimCollect = Animator.StringToHash("Collect");

    public bool IsBusy { get; protected set; }
    
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    public virtual void Initialise(LimbData data, LimbManager manager, Statboard statboard) {
        this.data = data;
        this.manager = manager;
        this.statboard = statboard;
    }

    protected InputEvent<float> input;
    public virtual void ReceiveInput(InputEvent<float> inputEvent) {
        input = inputEvent;
    }

    private void LateUpdate() {
        input.ResetState();
    }

    public virtual void Remove() { }

    
    // Arm Specific
    public bool PlayInteractionAnim() {
        if (animator.GetBool(AnimIdle)) {
            animator.SetTrigger(AnimCollect);
            return true;
        }
        return false;
    }
}



public abstract class Arm : Limb {
    
}



public abstract class Leg : Limb {
    [SerializeField] private float moveSpeed = 2.5f;
    public float MoveSpeed => moveSpeed;

    // Used by the Idle Anim on the Animator.
    public void ResetIdle() {
        IsBusy = false;
        PlayerHUDEvents.DebugText("Reset Idle");
    }
}
