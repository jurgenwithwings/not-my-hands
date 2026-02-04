using System;
using UnityEngine;

public class Leg : Limb {
    [SerializeField] private Collider hitBox;
    
    private static readonly int Idle = Animator.StringToHash("Idle");

    private void Update() {
        if (input.Triggered && animator.GetBool(Idle)) {
            Kick();
        }
    }

    private void Kick() {
        int random = UnityEngine.Random.Range(0, 2);
        switch (random) {
            case 0:
                animator.SetTrigger("Front");
                break;
            case 1:
                animator.SetTrigger("Sweep");
                break;
        }
    }
}