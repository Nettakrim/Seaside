using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void OnInteract(Interactor interactor);

    public abstract bool CanInteract(Interactor interactor);

    public abstract string GetPrompt();

    public virtual bool InView(Interactor interactor) {
        return Vector3.Angle(interactor.facing.forward, transform.position-interactor.facing.position) < 60;
    }
}
