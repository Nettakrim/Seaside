using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClickyUI {
    public class ClickyButton : Button
    {
        private bool hovered = false;

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);
            ClickyManager.PointerDown();
        }

        public override void OnPointerUp(PointerEventData eventData) {
            base.OnPointerUp(eventData);
            ClickyManager.PointerUp();
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

        public override void OnSelect(BaseEventData eventData) {
            base.OnSelect(eventData);
            if (!hovered || InputManager.instance.isController) {
                ClickyManager.PointerEnter();
            }
        }

        public override void OnSubmit(BaseEventData eventData) {
            base.OnSubmit(eventData);
            if (!hovered || InputManager.instance.isController) {
                ClickyManager.PointerDown();
            }
        }
    }
}