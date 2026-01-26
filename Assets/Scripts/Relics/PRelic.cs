using System;

public class PRelic : PhysicalLoot, IInteractable {
    public RelicData data;

    //Interactable
    public string InteractionName() => data.itemName;
    public bool HasAltInteraction { get; } = true;

    public void Interact(Statboard interactor) {
        interactor.relicManager.AddRelic(data);
        Destroy(gameObject);
    }

    public void AltInteract(Statboard interactor) {
        Interact(interactor);
        PlayerHUDEvents.DebugText($"Picked Up {data.itemName}");
    }
    //Interactable End

}