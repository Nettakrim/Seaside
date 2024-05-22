using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Campfire : Interactable
{
    public override bool CanInteract(Interactor interactor) {
        return true;
    }

    public override string GetPrompt() {
        return "Press Q/E to light";
    }

    public override void OnInteract(Interactor interactor) {
        Debug.Log("hi!");
    }
}
