using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlsText : TextMeshProUGUI
{
    protected static string keyboardSprite = "<sprite=\"Keyboard\"";
    protected static string controllerSprite = "<sprite=\"Controller\"";

    protected override void Start() {
        base.Start();
        if (InputManager.instance != null) {
            InputManager.instance.onIsControllerChange.AddListener(SetController);
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(TextChanged);
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        if (InputManager.instance != null) {
            SetController(InputManager.instance.isController);
        }
    }

    protected void TextChanged(Object obj) {
        if (obj == this) {
            SetController(InputManager.instance.isController);
        }
    }

    protected void SetController(bool to) {
        if (to) {
            SetText(text.Replace(keyboardSprite, controllerSprite));
        } else {
            SetText(text.Replace(controllerSprite, keyboardSprite));
        }
    }

    protected override void OnDestroy() {
        if (InputManager.instance != null) {
            InputManager.instance.onIsControllerChange.RemoveListener(SetController);
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(TextChanged);
        }
        base.OnDestroy();
    }
}
