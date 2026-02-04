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

        for (int i = 0; i < limbAnchors.Length; i++) {
            limbs[i] = LimbHelper.CreateDefaultLimb((LimbType)((int)(i * 0.5f) + 1), limbAnchors[i]);
            limbs[i].Initialise(LimbHelper.LoadLimbData(limbs[i].data.limbType), statboard);
        }
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
        
        //LimbData old = limbs[index]?.data;
        //Destroy(limbs[index]?.gameObject);
        
        limbs[index] = Instantiate(limbData.limbPrefab, limbAnchors[index].position, limbAnchors[index].rotation, limbAnchors[index]).GetComponent<Limb>();
        limbs[index].Initialise(limbData, statboard);
        
        //Instantiate(old.prefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(transform.forward + (transform.up * 0.4f) * 2f);
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
    
    protected Animator animator;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public virtual void Initialise(LimbData data, Statboard statboard) {
        this.data = data;
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

    public bool PlayInteractionAnim() {
        if (animator.GetBool("Idle")) {
            animator.SetTrigger("Collect");
            return true;
        }
        return false;
    }
}