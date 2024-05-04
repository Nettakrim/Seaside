using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play() {
        SceneManager.LoadScene("Seaside");
        SceneManager.LoadScene("Player", LoadSceneMode.Additive);
    }

    public void Quit() {
        Application.Quit();
    }
}
