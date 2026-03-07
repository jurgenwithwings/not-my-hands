using UnityEngine;

public class SetInt : StateMachineBehaviour {
    public string variable;
    public int value;
    
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger(variable, value);
    }
}
