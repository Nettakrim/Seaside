using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Interactor : MonoBehaviour
{
    private Interactable current;
    private bool canInteract;
    [SerializeField] private TextMeshProUGUI prompt;

    private void Update() {
        if (current != null && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) && current.CanInteract()) {
            current.OnInteract();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Interactable")) {
            current = other.GetComponent<Interactable>();
            prompt.text = current.GetPrompt();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.transform == current.transform) {
            current = null;
            prompt.text = "";
        }
    }

    public void SetCanInteract(bool to) {
        canInteract = to;
        prompt.gameObject.SetActive(to);
    }
}
