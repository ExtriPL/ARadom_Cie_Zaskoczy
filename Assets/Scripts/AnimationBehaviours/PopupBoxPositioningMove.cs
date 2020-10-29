using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBoxPositioningMove : StateMachineBehaviour
{
    [Tooltip("Numer pozycji, którą reprezentuje dany klip animacji")]
    public int movePosition;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("currentPosition", movePosition);
    }
}
