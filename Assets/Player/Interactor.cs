using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Interactor : MonoBehaviour
{
    private Interactable current;
    private bool canInteract = true;
    [SerializeField] private TextMeshProUGUI prompt;
    public Transform facing;

    private void Update() {
        if (current != null && canInteract && current.InView(this) && current.CanInteract(this)) {
            UpdateInteractable();
            prompt.gameObject.SetActive(true);
        } else {
            prompt.gameObject.SetActive(false);
        }
    }

    private void UpdateInteractable() {
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) {
            current.OnInteract(this);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Interactable")) {
            if (other.TryGetComponent(out current)) {
                prompt.text = current.GetPrompt();
            } else {
                Debug.Log(other.transform+" has Interactable tag but no interactable script");
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (current != null && other.transform == current.transform) {
            current = null;
            prompt.text = "";
        }
    }

    public void SetCanInteract(bool to) {
        canInteract = to;
    }
}
