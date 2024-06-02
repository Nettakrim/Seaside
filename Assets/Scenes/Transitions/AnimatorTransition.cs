using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTransition : TransitionType
{
    public Animator animator;

    public override void CoverUpdate(float time) {
        if (time == 0) {
            animator.SetBool("Cover", true);
        }
        animator.SetFloat("Time", time);
    }

    public override void UncoverUpdate(float time) {
        if (time == 0) {
            animator.SetBool("Cover", false);
        }
        animator.SetFloat("Time", time);
    }
}