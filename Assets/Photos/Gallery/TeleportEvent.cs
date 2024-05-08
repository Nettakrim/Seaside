using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEvent : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        PhotoManager.instance.gallery.Teleport();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        PhotoManager.instance.gallery.TeleportEnd();
    }
}
