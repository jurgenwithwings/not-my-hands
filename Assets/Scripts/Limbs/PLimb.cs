using UnityEngine;

public class PLimb : PhysicalLoot, IInteractable {
    public LimbData data; 
    
    public string InteractionName() => $"Pick Up {data.itemName}";
    public bool HasAltInteraction { get; } = true;
    
    public void Interact(Statboard interactor) {
        LimbManager lm = interactor.GetComponent<LimbManager>();
        if (lm != null) {
            lm.AddLimb(data, LimbSide.Left);
            OnPickUp?.Invoke();
            Destroy(gameObject);
        }
    }
    
    public void AltInteract(Statboard interactor) {
        LimbManager lm = interactor.GetComponent<LimbManager>();
        if (lm != null) {
            lm.AddLimb(data, LimbSide.Right);
            OnPickUp?.Invoke();
            Destroy(gameObject);
        }
    }
}
