using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private GameObject playButton;

    [SerializeField] private GameObject webglHide;
    [SerializeField] private GameObject webglShow;

    [SerializeField] private GameObject persistentOn;
    [SerializeField] private GameObject persistentOff;

    private void Awake() {
        Cursor.lockState = CursorLockMode.None;
        InputManager.instance.SetLost(playButton);

        #if UNITY_WEBGL
            bool isWebgl = true;
        #else
            bool isWebgl = false;
        #endif

        webglHide.SetActive(!isWebgl);
        webglShow.SetActive(isWebgl);

        SetPersistentSave(PlayerPrefs.GetInt("PersistentSave", isWebgl ? 0 : 1) == 1);
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

    public void TogglePersistentSave() {
        SetPersistentSave(!SaveManager.instance.persistentSave);
    }

    private void SetPersistentSave(bool newPersistent) {
        SaveManager.instance.SetPersistentSave(newPersistent);

        persistentOn.SetActive(newPersistent);
        persistentOff.SetActive(!newPersistent);
        PlayerPrefs.SetInt("PersistentSave", newPersistent ? 1 : 0);
    }

    public void Quit() {
        Application.Quit();
    }
}
