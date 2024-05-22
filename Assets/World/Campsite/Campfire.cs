using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Campfire : Interactable
{
    protected Texture texture;

    [SerializeField] protected GameObject image;
    protected Material material;

    [SerializeField] protected float burnTime;
    protected float burntAt;

    protected void Start() {
        material = image.GetComponent<Renderer>().material;
    }

    protected void Update() {
        if (burntAt > 0) {
            UpdateBurn();
        }
    }

    protected void UpdateBurn() {
        float t = (Time.time-burntAt)/burnTime;
        if (t >= 1) {
            FinishBurningImage();
            t = 0;
        }

        material.SetFloat("_Progress", t);
    }

    public override bool CanInteract(Interactor interactor) {
        return burntAt == 0;
    }

    public override string GetPrompt() {
        return PhotoManager.instance.gallery.PhotoCount() == 0 ? "Take a photo to use it as fuel!" : "Press Q/E to light the campfire using a random photo!";
    }

    public override void OnInteract(Interactor interactor) {
        if (PhotoManager.instance.gallery.PhotoCount() == 0) return;

        Gallery gallery = PhotoManager.instance.gallery;
        gallery.SelectRandom();
        StartBurningImage(gallery.DeleteSelected(false));
    }

    public void StartBurningImage(Texture texture) {
        this.texture = texture;
        
        material.SetTexture("_MainTex", texture);
        image.SetActive(true);
        burntAt = Time.time;
    }

    public void FinishBurningImage() {
        Destroy(texture);
        image.SetActive(false);
        burntAt = 0;
    }
}
