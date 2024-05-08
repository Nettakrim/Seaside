using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoManager : MonoBehaviour
{
    [NonSerialized] public Mode currentMode;

    public Gallery gallery;
    public PhotoTaking photoTaking;
    public PlayerController player;

    public static PhotoManager instance;

    public void Awake() {
        instance = this;
    }

    public void SetMode(Mode mode) {
        if (currentMode == Mode.Gallery) {
            gallery.CloseGallery();
        }
        if (currentMode == Mode.PhotoTaking) {
            photoTaking.CloseCameraMode();
        }

        currentMode = mode;

        if (currentMode == Mode.Gallery) {
            gallery.OpenGallery();
        }
        if (currentMode == Mode.PhotoTaking) {
            photoTaking.OpenCameraMode();
        }
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            SetMode(currentMode == Mode.Gallery ? Mode.Walking : Mode.Gallery);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            SetMode(currentMode == Mode.PhotoTaking ? Mode.Walking : Mode.PhotoTaking);
        }
    }

    public enum Mode {
        Walking,
        Gallery,
        PhotoTaking
    }
}
