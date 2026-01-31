using UnityEngine;

public class IdleTracker : StateMachineBehaviour {
    private static readonly int Idle = Animator.StringToHash("Idle");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(Idle, true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(Idle, false);
    }
}
