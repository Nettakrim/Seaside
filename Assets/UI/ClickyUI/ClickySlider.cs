using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClickyUI {
    public class ClickySlider : Slider
    {
        [SerializeField] protected bool clickSound = true;
        [SerializeField] protected bool hoverSound = true;
        [SerializeField] protected int clicks = 9;

        private float lastValue;
        private bool dragging;
        private bool hovered;

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            ClickyManager.PointerDown();
            dragging = true;
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
            ClickyManager.PointerUp();
            dragging = false;
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            ClickyManager.PointerEnter();
            hovered = true;
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            ClickyManager.PointerExit();
            hovered = false;
        }

        protected override void Set(float input, bool sendCallback = true) {
            float num = GetClickValue(input);
            if (num != lastValue && interactable && IsDragging() && clicks > 0) ClickyManager.PointerEnter();
            lastValue = num;
            base.Set(input, sendCallback);
        }

        protected float GetClickValue(float input) {
            if (wholeNumbers) {
                return Mathf.Round(Mathf.Clamp(input, minValue, maxValue));
            }
            return input == maxValue ? clicks : Mathf.Ceil((input-minValue)/(maxValue-minValue)*(clicks-1));
        }

        protected bool IsDragging() {
            return dragging;
        }

        public override void OnMove(AxisEventData eventData) {
            bool selected = EventSystem.current.currentSelectedGameObject == gameObject;
            base.OnMove(eventData);
            if (selected && EventSystem.current.currentSelectedGameObject == gameObject) {
                ClickyManager.PointerEnter();
            }
        }

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            if (!hovered || InputManager.instance.isController) {
                ClickyManager.PointerEnter();
            }
        }
    }
}