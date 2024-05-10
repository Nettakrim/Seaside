using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    protected void Start() {
        renderTexture = photoCamera.targetTexture;
        normalFov = mainCamera.fieldOfView;
        targetFovScale = 1;
        currentFovScale = 1;
        depthTex = new Texture2D(depthRenderTex.width, depthRenderTex.height);
        photoCamera.depthTextureMode = DepthTextureMode.Depth;
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
                result.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.D)) {
                if (SaveLastPhoto()) {
                    manager.gallery.SetCurrentPhoto(-1);
                    manager.SetMode(PhotoManager.Mode.Gallery);
                }
            }
        }

        if (manager.currentMode != PhotoManager.Mode.Gallery) {
            UpdateFov(Mathf.Lerp(currentFovScale, targetFovScale, Time.deltaTime * fovLerpSpeed));
        }
    }

    public void OpenCameraMode() {
        cameraOverlay.SetActive(true);
        manager.player.SetMovementLock(true);
        result.SetActive(false);
        info.text = "";
        canZoom = false;
    }

    public void CloseCameraMode() {
        cameraOverlay.SetActive(false);
        manager.player.SetMovementLock(false);
        targetFovScale = 1;
    }

    protected void UpdateFov(float newScale) {
        currentFovScale = newScale;
        mainCamera.fieldOfView = normalFov*currentFovScale;
        photoCamera.fieldOfView = normalFov*currentFovScale;
        manager.player.SetRotationSpeed(currentFovScale);
    }

    protected void TakePhoto() {
        photoCamera.Render();

        RenderDepth();

        currentMetadata = new ImageMetadata();
        currentMetadata.position = manager.player.GetPosition();
        currentMetadata.rotation = manager.player.GetRotation();
        currentMetadata.fov = photoCamera.fieldOfView;
        currentMetadata.targets = GetVisibleCameraTargets();
        info.text = currentMetadata.GetInfoText();
    }

    protected void RenderDepth() {
        depthMaterial.SetFloat("_AspectRatio", Screen.height / (float)Screen.width);
        Graphics.Blit(depthTex, depthRenderTex, depthMaterial);
        ReadPixels(depthRenderTex, depthTex);
    }
    
    protected bool SaveLastPhoto() {
        if (currentMetadata == null) {
            Debug.LogWarning("SaveLastPhoto() was called, but there is currently no unsaved photo!");
            return false;
        }

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
        tex.filterMode = FilterMode.Point;

        ReadPixels(renderTexture, tex);

        manager.gallery.SaveNewImage(tex, currentMetadata);
        currentMetadata = null;
        return true;
    }

    protected void ReadPixels(RenderTexture rt, Texture2D tex) {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = previous;
    }

    public void SetFov(float fov) {
        UpdateFov(fov/normalFov);
    }

    protected List<CameraTargetData.Wrapper> GetVisibleCameraTargets() {
        List<CameraTargetData.Wrapper> targets = new List<CameraTargetData.Wrapper>();
        Plane[] planes = new Plane[6];

        foreach (CameraTarget cameraTarget in TargetManager.instance.targetsInWorld) {
            if (cameraTarget.IsVisible()) {
                GeometryUtility.CalculateFrustumPlanes(photoCamera, planes);
                if (GeometryUtility.TestPlanesAABB(planes, cameraTarget.GetBounds())) {
                    CameraTargetData.Wrapper wrapper = cameraTarget.GetCameraTargetData(photoCamera);
                    if (IsVisible(cameraTarget, wrapper)) {
                        targets.Add(wrapper);
                    }
                }
            }
        }

        targets.Sort((x, y) => y.viewProportion.CompareTo(x.viewProportion));

        return targets;
    }

    protected bool IsVisible(CameraTarget cameraTarget, CameraTargetData.Wrapper wrapper) {
        if (wrapper.viewProportion < wrapper.cameraTargetData.viewProportionThreshold) {
            return false;
        }

        //check depth

        return true;
    }

    protected float LineariseDepth(float depth) {
        float near = photoCamera.nearClipPlane;
        float far = photoCamera.farClipPlane;
        return far * near / ((near - far) * depth + far);
    }

}