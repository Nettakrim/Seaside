using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhotoManager : MonoBehaviour
{
    [NonSerialized] public Mode currentMode;

    public Gallery gallery;
    public PhotoTaking photoTaking;
    public PlayerController player;

    public static PhotoManager instance;

    public enum Mode {
        Walking,
        Gallery,
        PhotoTaking
    }

    public void Awake() {
        instance = this;
        Cursor.lockState = CursorLockMode.Locked;
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
            Cursor.lockState = CursorLockMode.None;
            gallery.OpenGallery();
        } else {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (currentMode == Mode.PhotoTaking) {
            photoTaking.OpenCameraMode();
        }
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !MouseOnUI()) {
            SetMode(currentMode == Mode.Walking ? Mode.PhotoTaking : Mode.Walking);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            SetMode(currentMode == Mode.Walking ? Mode.Gallery : Mode.Walking);
        }
    }

    public bool MouseOnUI() {
        return EventSystem.current.IsPointerOverGameObject() && MouseOnUI(GetEventSystemRaycastResults());
    }

    //https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
    private static bool MouseOnUI(List<RaycastResult> eventSystemRaysastResults)
    {
        foreach (RaycastResult curRaycastResult in eventSystemRaysastResults) {
            if (!curRaycastResult.gameObject.CompareTag("IgnoreUI")) return true;
        }
        return false;
    }

    private static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
