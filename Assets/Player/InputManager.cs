using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public bool isController;

    public Axis turnX = new Axis("Mouse X");
    public Axis turnY = new Axis("Mouse Y");

    public Axis moveX = new Axis("Horizontal");
    public Axis moveY = new Axis("Vertical");

    public Axis jump = new Axis("Jump");
    public Axis interact = new Axis("Interact");

    public Axis photo = new Axis("Photo");
    public Axis gallery = new Axis("Gallery");
    public Axis train = new Axis("Train");

    public Axis pause = new Axis("Pause");

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
        // update isController
    }

    private void UpdateAxes() {
        turnX.Update();
        turnY.Update();

        moveX.Update();
        moveY.Update();

        jump.Update();
        interact.Update();

        photo.Update();
        gallery.Update();
        train.Update();

        pause.Update();
    }

    public class Axis {
        public float value;
        public float lastValue;

        public string name;
        
        public Axis(string name) {
            this.name = name;
        }

        public void Update() {
            lastValue = value;
            value = Input.GetAxis(name);
        }

        public bool Get() {
            return value > 0f || value < 0f;
        }

        public bool GetDown() {
            return (value > 0f || value < 0f) && lastValue == 0f;
        }

        public float GetRaw() {
            return Input.GetAxisRaw(name);
        }
    }
}
