using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberButton : Interactable
{
    [SerializeField] private Animator animator;

    [SerializeField] private BoxCollider targetRange;
    [SerializeField] private GameObject target;
    private Transform player;

    [SerializeField] private Material active;
    [SerializeField] private Material inactive;
    [SerializeField] private Renderer buttonRenderer;

    bool running = false;

    protected void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected void Update() {
        if (running) {
            target.SetActive(targetRange.bounds.Contains(player.position));
            if (CanInteract(null)) {
                running = false;
                SetButtonMaterial(true);
            }
        }
    }

    public override bool CanInteract(Interactor interactor) {
        return animator.GetCurrentAnimatorClipInfoCount(0) == 0;
    }

    public override string GetPrompt() {
        return "Press Q/E to cut a log!";
    }

    public override void OnInteract(Interactor interactor) {
        animator.SetTrigger("Cut");
        running = true;
        SetButtonMaterial(false);
    }

    private void SetButtonMaterial(bool canInteract) {
        Material[] materials = buttonRenderer.sharedMaterials;
        materials[1] = canInteract ? active : inactive;
        buttonRenderer.sharedMaterials = materials;
    }
}
