using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    private List<AsyncOperation> sceneLoads = new List<AsyncOperation>();
    private SceneTransition activeTransition;

    [SerializeField] private Transform playerPrefab;
    private string currentAreaName;

    private void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetTransition(SceneTransition sceneTransition) {
        if (HasTransition()) {
            Destroy(activeTransition);
        }

        activeTransition = Instantiate(sceneTransition);
        DontDestroyOnLoad(activeTransition);
    }

    public bool HasTransition() {
        return activeTransition != null;
    }

    public void LoadArea(string area) {
        currentAreaName = area.ToLowerInvariant();
        LoadScene(area, LoadSceneMode.Single);
    }

    public void LoadScene(string scene, LoadSceneMode mode) {
        // this supports additive scenes because the player used to be a scene, but wouldnt load on the same frame
        // but while not synced it *does* work, so theres not much point getting rid of it in case a future update needs it, despite it not being used
        if (mode == LoadSceneMode.Single) {
            foreach (AsyncOperation operation in sceneLoads) {
                operation.allowSceneActivation = true;
            }
            sceneLoads.Clear();
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene, mode);
        asyncOperation.allowSceneActivation = false;
        sceneLoads.Add(asyncOperation);

        if (mode == LoadSceneMode.Single) {
            StartCoroutine(Transition());
        }
    }

    public IEnumerator Transition() {
        yield return StartCoroutine(activeTransition.Cover());

        bool continueWaiting = true;
        while (continueWaiting) {
            foreach (AsyncOperation asyncOperation in sceneLoads) {
                if (asyncOperation.progress < 0.9f) {
                    continueWaiting = true;
                    break;
                }
            }

            if (continueWaiting) {
                continueWaiting = false;
                yield return null;
            }
        }

        foreach (AsyncOperation asyncOperation in sceneLoads) {
            asyncOperation.allowSceneActivation = true;
        }
        yield return null;

        yield return StartCoroutine(activeTransition.Uncover());
        sceneLoads.Clear();
    }

    public bool IsLoadingScene() {
        return sceneLoads.Count > 0;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single && scene.name.ToLowerInvariant() == currentAreaName) {
            Instantiate(playerPrefab);
        }
    }

    public string GetCurrentAreaName() {
        return currentAreaName;
    }
}
