using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private GameObject playButton;

    private void Awake() {
        Cursor.lockState = CursorLockMode.None;
        InputManager.instance.SetLost(playButton);
    }

    public void LoadArea(string area) {
        if (SceneLoader.instance.IsLoadingScene()) {
            return;
        }

        SceneLoader sceneLoader = SceneLoader.instance;
        if (!sceneLoader.HasTransition()) {
            sceneLoader.SetTransition(sceneTransition);
        }

        sceneLoader.LoadArea(area);
    }

    public void Quit() {
        Application.Quit();
    }
}
