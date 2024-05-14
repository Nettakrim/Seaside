using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Awake() {
        Cursor.lockState = CursorLockMode.None;
    }

    public void Play() {
        SceneManager.LoadScene("Seaside");
        SceneManager.LoadScene("Player", LoadSceneMode.Additive);
    }

    public void Quit() {
        Application.Quit();
    }
}
