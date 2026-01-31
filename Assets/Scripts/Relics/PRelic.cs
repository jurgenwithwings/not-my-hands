using System;

public class PRelic : PhysicalLoot, IInteractable {
    public RelicData data;

    //Interactable
    public string InteractionName() => data.itemName;
    public bool HasAltInteraction { get; }

    public void Interact(Statboard interactor) {
        interactor.relicManager.AddRelic(data);
        Destroy(gameObject);
    }
    //Interactable End

}