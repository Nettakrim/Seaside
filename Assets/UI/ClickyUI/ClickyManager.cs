using UnityEngine;

namespace ClickyUI {
    public class ClickyManager : MonoBehaviour
    {
        public static ClickyManager instance;

        public RandomAudioSource pointerDown;
        public RandomAudioSource pointerUp;
        public RandomAudioSource pointerEnter;
        public RandomAudioSource pointerExit;

        private bool active;

        public void Start() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this);

            // webgl doesnt like playing the sounds properly so it sounds sorta bad
            // until a fix is figured out, its better to just not have clickyui
            #if UNITY_WEBGL
            active = false;
            #else
            active = true;
            #endif
        }

        public static void PointerDown() {
            if (instance.active) instance.pointerDown.PlayRandom();
        }

        public static void PointerUp() {
            if (instance.active) instance.pointerUp.PlayRandom();
        }

        public static void PointerEnter() {
            if (instance.active) instance.pointerEnter.PlayRandom();
        }

        public static void PointerExit() {
            if (instance.active) instance.pointerExit.PlayRandom();
        }
    }
}