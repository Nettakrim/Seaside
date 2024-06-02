using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    private void Awake() {
        Cursor.lockState = CursorLockMode.None;
    }

    public void Play() {
        if (SceneLoader.instance.IsLoadingScene()) {
            return;
        }
        LoadArea("Seaside");
    }

    public void LoadArea(string area) {
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
