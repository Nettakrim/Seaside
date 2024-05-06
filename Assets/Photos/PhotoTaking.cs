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
    protected float fovVelocity;

    [SerializeField] protected float fovVelocitySpeed;
    [SerializeField] protected float fovChangeSpeed;
    [SerializeField] protected float maxFovVelocity;
    
    [SerializeField] protected float minFovScale = 0.25f;
    [SerializeField] protected float maxFovScale = 1.5f;

    protected void Start() {
        renderTexture = photoCamera.targetTexture;
        normalFov = mainCamera.fieldOfView;
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SetCameraMode(!inCameraMode);
        }

        if (inCameraMode) {
            if (Input.GetKey(KeyCode.W)) {
                if (fovVelocity > 0) fovVelocity = 0;
                fovVelocity -= Time.deltaTime*fovVelocitySpeed;
                UpdateFov(currentFovScale + fovVelocity*fovChangeSpeed*Time.deltaTime);
            } else if (fovVelocity < 0) {
                fovVelocity += Time.deltaTime*fovVelocitySpeed*4;
            }

            if (Input.GetKey(KeyCode.S)) {
                if (fovVelocity < 0) fovVelocity = 0;
                fovVelocity += Time.deltaTime*fovVelocitySpeed;
                UpdateFov(currentFovScale + fovVelocity*fovChangeSpeed*Time.deltaTime);
            } else if (fovVelocity > 0) {
                fovVelocity -= Time.deltaTime*fovVelocitySpeed*4;
            }

            fovVelocity = Mathf.Clamp(fovVelocity, -maxFovVelocity, maxFovVelocity);

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
    }

    protected void SetCameraMode(bool to) {
        inCameraMode = to;
        cameraOverlay.SetActive(inCameraMode);
        player.SetMovementLock(inCameraMode);
        gallery.SetGalleryActive(!inCameraMode);
        result.SetActive(false);
        UpdateFov(1);
    }

    protected void UpdateFov(float newScale) {
        currentFovScale = Mathf.Clamp(newScale, minFovScale, maxFovScale);
        mainCamera.fieldOfView = normalFov*currentFovScale;
        photoCamera.fieldOfView = normalFov*currentFovScale;
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

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, false);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = previous;

        gallery.SaveNewImage(tex, currentMetadata);
        currentMetadata = null;
        return true;
    }
}
