using UnityEngine;

namespace ClickyUI {
    public class ClickyManager : MonoBehaviour
    {
        public static ClickyManager instance;

        public RandomAudioSource pointerDown;
        public RandomAudioSource pointerUp;
        public RandomAudioSource pointerEnter;
        public RandomAudioSource pointerExit;

        public void Start() {
            if (instance != null) {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this);
        }

        public static void PointerDown() {
            instance.pointerDown.PlayRandom();
        }

        public static void PointerUp() {
            instance.pointerUp.PlayRandom();
        }

        public static void PointerEnter() {
            instance.pointerEnter.PlayRandom();
        }

        public static void PointerExit() {
            instance.pointerExit.PlayRandom();
        }
    }
}