using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class PhotoManager : MonoBehaviour
{
    [NonSerialized] public Mode currentMode;

    public Gallery gallery;
    public PhotoTaking photoTaking;
    public PlayerController player;
    public Interactor interactor;

    public static PhotoManager instance;

    [SerializeField] protected TextMeshProUGUI tutorial;
    protected int tutorialStep;
    protected Vector3 startPosition;
    [SerializeField] protected float tutorialWalkDistance;

    public bool isComplete;

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

        if (tutorialStep > 0) {
            tutorial.gameObject.SetActive(currentMode == Mode.Walking);
        }
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !MouseOnUI()) {
            SetMode(currentMode == Mode.Walking ? Mode.PhotoTaking : Mode.Walking);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            SetMode(currentMode == Mode.Walking ? Mode.Gallery : Mode.Walking);
        }

        if (!isComplete) {
            if (gallery.AllGoalsComplete()) {
                isComplete = true;
                tutorialStep = 3;
                player.SetCanFly(true);
                NextTutorialStep();
            }
        } else {
            if (gallery.PhotoCount() == 0) {
                isComplete = false;
                player.SetCanFly(false);
            }
        }

        UpdateTutorial();
    }

    public bool MouseOnUI() {
        return EventSystem.current.IsPointerOverGameObject() && MouseOnUI(GetEventSystemRaycastResults());
    }

    // https://forum.unity.com/threads/how-to-detect-if-mouse-is-over-ui.1025533/
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

    protected void UpdateTutorial() {
        if (tutorialStep == 0) {
            if (gallery.PhotoCount() == 0) {
                tutorialStep = 1;
                isComplete = false;
                player.SetCanFly(false);
                NextTutorialStep();
            }
        } else {
            if (currentMode == Mode.Walking) {
                tutorial.gameObject.SetActive(!interactor.HasInteractionPrompt());
            }

            if (tutorialStep == 1) {
                if (Vector3.Distance(startPosition, player.transform.position) > tutorialWalkDistance) {
                    NextTutorialStep();
                }
            } else if (tutorialStep == 2) {
                if (gallery.PhotoCount() > 0) {
                    NextTutorialStep();
                }
            } else if (tutorialStep == 3) {
                if (Input.GetKeyDown(KeyCode.Mouse1) && currentMode == Mode.Gallery) {
                    EndTutorial();
                }
            } else {
                if (player.IsFlying()) {
                    tutorialStep |= 8;
                }
                if (Input.GetKey(KeyCode.T)) {
                    tutorialStep |= 16;
                }
                if (tutorialStep >= 28) {
                    EndTutorial();
                }
            }
        }
    }

    public void NextTutorialStep() {
        tutorialStep++;
        tutorial.gameObject.SetActive(currentMode == Mode.Walking);
        
        if (tutorialStep == 1) {
            tutorial.text = "Use the Mouse to look around and WASD/Space to move!";
            startPosition = player.transform.position;
        }

        if (tutorialStep == 2) {
            tutorial.text = "Press the Left Mouse Button to start taking pictures";
        }

        if (tutorialStep == 3) {
            tutorial.text = "Press the Right Mouse Button to see your photos and to-do list";
        }

        if (tutorialStep >= 4) {
            tutorial.text = "Game Complete! Press Space while in the air to fly, Press T to spawn a train, or delete all your photos to start over!";
        }
    }

    public void EndTutorial() {
        tutorialStep = 0;
        tutorial.gameObject.SetActive(false);
    }
}
