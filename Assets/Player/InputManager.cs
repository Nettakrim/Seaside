using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    
    [NonSerialized] public bool isController;
    [NonSerialized] public UnityEvent<bool> onIsControllerChange = new UnityEvent<bool>();

    public Axis turnX = new Axis("Mouse X");
    public Axis turnY = new Axis("Mouse Y");
    public Axis turnXController = new Axis("Controller X");
    public Axis turnYController = new Axis("Controller Y");

    public Axis moveX = new Axis("Horizontal");
    public Axis moveY = new Axis("Vertical");

    public Axis jump = new Axis("Jump");
    public Axis interact = new Axis("Interact");

    public Axis photo = new Axis("Photo");
    public Axis gallery = new Axis("Gallery");
    public Axis train = new Axis("Train");

    public Axis pause = new Axis("Pause");
    public Axis submit = new Axis("Submit");

    private GameObject lost;
    private bool isSettingLost;

    private bool lockCursor;

    public void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    public void Update() {
        UpdateAxes();

        UpdateController();
    }

    private void UpdateAxes() {
        turnX.Update();
        turnY.Update();
        turnXController.Update();
        turnYController.Update();

        moveX.Update();
        moveY.Update();

        jump.Update();
        interact.Update();

        photo.Update();
        gallery.Update();
        train.Update();

        pause.Update();
        submit.Update();
    }

    public class Axis {
        public float value;
        public float lastValue;

        public float rawValue;
        public float lastRawValue;

        public string name;
        
        public Axis(string name) {
            this.name = name;
        }

        public void Update() {
            lastValue = value;
            value = Input.GetAxis(name);

            lastRawValue = rawValue;
            rawValue = Input.GetAxisRaw(name);
        }

        public bool Get() {
            return value > 0f || value < 0f;
        }

        public bool GetDown() {
            return (rawValue > 0f || rawValue < 0f) && lastRawValue == 0f;
        }
    }

    private void UpdateController() {
        if (isController && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))) {
            SetIsController(false);
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton0) && !isController) {
            SetIsController(true);
        }

        if (submit.GetDown() && EventSystem.current.currentSelectedGameObject == null && lost != null) {
            EventSystem.current.SetSelectedGameObject(lost);
        }
    }

    public void SetIsController(bool to) {
        isController = to;
        onIsControllerChange.Invoke(to);
        UpdateCursorLock();
    }

    public void SetLost(GameObject lost) {
        // selects an object and sets it to be selected again if its somehow deselected
        this.lost = lost;
        isSettingLost = true;
        EventSystem.current.SetSelectedGameObject(lost);
        isSettingLost = false;
    }

    public bool IsSettingLost() {
        return isSettingLost;
    }

    public void SetCursorLock(bool locked) {
        lockCursor = locked;
        UpdateCursorLock();
    }

    private void UpdateCursorLock() {
        Cursor.lockState = (lockCursor || isController) ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
