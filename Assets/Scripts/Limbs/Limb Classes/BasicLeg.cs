using UnityEngine;

public class BasicLeg : Leg {
    [SerializeField] private Collider hitbox;

    private void Update() {
        if (input.Triggered && !manager.IsLegBusy) {
            Kick();
        }
    }

    private void Kick() {
        IsBusy = true;
        animator.SetTrigger("Sweep");
    }
}