using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public Axis moveX = new Axis("Horizontal");
    public Axis moveY = new Axis("Vertical");

    public Axis turnX = new Axis("Mouse X");
    public Axis turnY = new Axis("Mouse Y");

    public Axis jump = new Axis("Jump");


    public void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    public void Update() {
        moveX.Update();
        moveY.Update();
        turnX.Update();
        turnY.Update();
        jump.Update();
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
            return value > 0f;
        }

        public bool GetDown() {
            return value > 0f && lastValue == 0f;
        }
    }
}
