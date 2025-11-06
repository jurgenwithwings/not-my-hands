public interface IInteractable {
    string InteractionName();
    bool HasAltInteraction { get; }
    
    
    void Interact(Statboard interactor); 
    void AltInteract(Statboard interactor) { return; }
}