using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoTaking : MonoBehaviour
{
    [SerializeField] protected PhotoManager manager;

    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected Camera photoCamera;
    protected RenderTexture renderTexture;

    protected ImageMetadata currentMetadata;

    [SerializeField] protected GameObject cameraOverlay;

    [SerializeField] protected GameObject result;

    protected float normalFov;
    protected float currentFovScale;
    protected float targetFovScale;
    [SerializeField] protected float fovChangeSpeed;
    [SerializeField] protected float fovLerpSpeed;

    [SerializeField] protected float minFovScale = 0.25f;
    [SerializeField] protected float maxFovScale = 1.5f;

    protected bool canZoom;

    [SerializeField] protected TextMeshProUGUI info;

    [SerializeField] protected Material depthMaterial;
    protected Texture2D depthTex;
    [SerializeField] protected RenderTexture depthRenderTex;

    protected bool depthRendered;

    [SerializeField] protected GameObject prompt;

    protected bool showControls;

    protected void Start() {
        renderTexture = photoCamera.targetTexture;
        normalFov = mainCamera.fieldOfView;
        targetFovScale = 1;
        currentFovScale = 1;
        depthTex = new Texture2D(depthRenderTex.width, depthRenderTex.height, TextureFormat.RFloat, false);
        photoCamera.depthTextureMode = DepthTextureMode.Depth;
        showControls = true;
    }

    protected void Update() {
        if (manager.currentMode == PhotoManager.Mode.PhotoTaking) {
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
                ClearTakenPhoto();
            }

            if (Input.GetKeyDown(KeyCode.D)) {
                if (SaveLastPhoto()) {
                    manager.gallery.SetCurrentPhoto(-1);
                    manager.SetMode(PhotoManager.Mode.Gallery);
                }
            }

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) {
                showControls = !showControls;
                prompt.SetActive(showControls);
            }
        }

        if (manager.currentMode != PhotoManager.Mode.Gallery) {
            // NOTE: lerp smoothing with deltatime is slightly innacurate, this should be Exp instead
            UpdateFov(Mathf.Lerp(currentFovScale, targetFovScale, Time.deltaTime * fovLerpSpeed));
        }
    }

    public void OpenCameraMode() {
        cameraOverlay.SetActive(true);
        manager.player.SetMovementLock(true);
        manager.interactor.SetCanInteract(false);
        ClearTakenPhoto();
        canZoom = false;
        prompt.SetActive(showControls);
    }

    public void CloseCameraMode() {
        cameraOverlay.SetActive(false);
        manager.player.SetMovementLock(false);
        manager.interactor.SetCanInteract(true);
        targetFovScale = 1;
    }

    public void SetFov(float fov) {
        UpdateFov(fov/normalFov);
    }

    protected void UpdateFov(float newScale) {
        currentFovScale = newScale;
        mainCamera.fieldOfView = normalFov*currentFovScale;
        photoCamera.fieldOfView = normalFov*currentFovScale;
        // slow down camera rotation as you zoom in
        // this isnt a perfect scale to get the same visual distance, but its close enough
        manager.player.SetRotationSpeed(currentFovScale);
    }

    protected void TakePhoto() {
        photoCamera.Render();

        depthRendered = false;

        currentMetadata = new ImageMetadata();
        currentMetadata.position = manager.player.GetPosition();
        currentMetadata.rotation = manager.player.GetRotation();
        currentMetadata.fov = photoCamera.fieldOfView;
        currentMetadata.SetTargetVisibility(Visibility.GetVisibleCameraTargets(photoCamera, this, TargetManager.instance.targetsInWorld));
        info.text = currentMetadata.GetInfoText();
    }

    protected void ClearTakenPhoto() {
        result.SetActive(false);
        info.text = "";
        currentMetadata = null;
    }
    
    protected bool SaveLastPhoto() {
        if (currentMetadata == null) {
            Debug.LogWarning("SaveLastPhoto() was called, but there is currently no unsaved photo!");
            return false;
        }

        Texture2D tex = GetTextureFromHDR(renderTexture);

        manager.gallery.SaveNewImage(tex, currentMetadata);
        currentMetadata = null;
        return true;
    }

    protected Texture2D GetTextureFromHDR(RenderTexture rt) {
        // in order for bloom to work, the rendertexture needs to be HDR
        // which needs to be converted to be in standard colorspace

        Texture2D readTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false, false);

        ReadPixels(rt, readTex);

        Color[] colors = readTex.GetPixels();
        Destroy(readTex);
        for (int i = 0; i < colors.Length; i++) {
            Color color = colors[i];
            color.r = Mathf.LinearToGammaSpace(color.r);
            color.g = Mathf.LinearToGammaSpace(color.g);
            color.b = Mathf.LinearToGammaSpace(color.b);
            colors[i] = color;
        }

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    protected void ReadPixels(RenderTexture rt, Texture2D tex) {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = previous;
    }

    public Texture2D RenderDepth() {
        if (depthRendered) return depthTex;
        depthRendered = true;
        depthMaterial.SetFloat("_AspectRatio", Screen.height / (float)Screen.width);
        Graphics.Blit(depthTex, depthRenderTex, depthMaterial);
        ReadPixels(depthRenderTex, depthTex);
        return depthTex;
    }
}
