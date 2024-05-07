using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhotoTaking : MonoBehaviour
{
    [SerializeField] protected PlayerController player;
    
    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected Camera photoCamera;
    protected RenderTexture renderTexture;

    [SerializeField] protected Gallery gallery;

    protected ImageMetadata currentMetadata;

    [SerializeField] protected GameObject cameraOverlay;
    protected bool inCameraMode;

    [SerializeField] protected GameObject result;

    protected float normalFov;
    protected float currentFovScale;
    protected float targetFovScale;
    [SerializeField] protected float fovChangeSpeed;
    [SerializeField] protected float fovLerpSpeed;

    [SerializeField] protected float minFovScale = 0.25f;
    [SerializeField] protected float maxFovScale = 1.5f;

    private bool canZoom;

    protected void Start() {
        renderTexture = photoCamera.targetTexture;
        normalFov = mainCamera.fieldOfView;
        targetFovScale = 1;
        currentFovScale = 1;
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SetCameraMode(!inCameraMode);
        }

        if (inCameraMode) {
            if (canZoom) {
                if (Input.GetKey(KeyCode.W)) {
                    targetFovScale -= fovChangeSpeed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.S)) {
                    targetFovScale += fovChangeSpeed * Time.deltaTime;
                }
                targetFovScale = Mathf.Clamp(targetFovScale, minFovScale, maxFovScale);
            } else {
                canZoom |= Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S);
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                TakePhoto();
                result.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.A)) {
                result.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.D)) {
                if (SaveLastPhoto()) {
                    SetCameraMode(false);
                }
            }
        }

        UpdateFov(Mathf.Lerp(currentFovScale, targetFovScale, Time.deltaTime * fovLerpSpeed));
    }

    protected void SetCameraMode(bool to) {
        inCameraMode = to;
        cameraOverlay.SetActive(inCameraMode);
        player.SetMovementLock(inCameraMode);
        if (inCameraMode) {
            gallery.SetGalleryActive(false);
        }
        result.SetActive(false);
        targetFovScale = 1;
        canZoom = false;
    }

    protected void UpdateFov(float newScale) {
        currentFovScale = newScale;
        mainCamera.fieldOfView = normalFov*currentFovScale;
        photoCamera.fieldOfView = normalFov*currentFovScale;
        player.SetRotationSpeed(currentFovScale);
    }

    protected void TakePhoto() {
        photoCamera.Render();
        currentMetadata = new ImageMetadata();
        currentMetadata.position = player.GetPosition();
        currentMetadata.rotation = player.GetRotation();
    }

    protected bool SaveLastPhoto() {
        if (currentMetadata == null) {
            Debug.LogWarning("SaveLastPhoto() was called, but there is currently no unsaved photo!");
            return false;
        }

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
        tex.filterMode = FilterMode.Point;

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = previous;

        gallery.SaveNewImage(tex, currentMetadata);
        currentMetadata = null;
        return true;
    }
}
