using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSceneTransition : SceneTransition
{
    public Animator animator;

    protected override void CoverUpdate(float time) {
        if (time == 0) {
            animator.SetBool("Cover", true);
        }
        animator.SetFloat("Time", time);
    }

    protected override void UncoverUpdate(float time) {
        if (time == 0) {
            animator.SetBool("Cover", false);
        }
        animator.SetFloat("Time", time);
    }
}