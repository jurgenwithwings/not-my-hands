using System;

public class PRelic : PhysicalLoot, IInteractable {
    public RelicData data;

    //Interactable
    public string InteractionName() => data.displayName;
    public bool HasAltInteraction { get; }

    public void Interact(Statboard interactor) {
        interactor.relicManager.AddRelic(data.relicType, data);
        Destroy(gameObject);
    }
    //Interactable End

}