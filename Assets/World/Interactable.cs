using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract void OnInteract();

    public abstract bool CanInteract();

    public abstract string GetPrompt();
}
