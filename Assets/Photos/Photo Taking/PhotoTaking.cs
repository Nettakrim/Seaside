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

    protected void Start() {
        renderTexture = photoCamera.targetTexture;
        normalFov = mainCamera.fieldOfView;
        targetFovScale = 1;
        currentFovScale = 1;
        depthTex = new Texture2D(depthRenderTex.width, depthRenderTex.height, TextureFormat.RFloat, false);
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
                ClearTakenPhoto();
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
        manager.interactor.SetCanInteract(false);
        ClearTakenPhoto();
        canZoom = false;
        prompt.SetActive(manager.gallery.PhotoCount() <= 1);
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
        manager.player.SetRotationSpeed(currentFovScale);
    }

    protected void TakePhoto() {
        photoCamera.Render();

        depthRendered = false;

        currentMetadata = new ImageMetadata();
        currentMetadata.position = manager.player.GetPosition();
        currentMetadata.rotation = manager.player.GetRotation();
        currentMetadata.fov = photoCamera.fieldOfView;
        currentMetadata.SetTargets(GetVisibleCameraTargets());
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

    protected List<CameraTargetData.Wrapper> GetVisibleCameraTargets() {
        List<CameraTargetData.Wrapper> targets = new List<CameraTargetData.Wrapper>();
        Plane[] planes = new Plane[6];

        foreach (CameraTarget cameraTarget in TargetManager.instance.targetsInWorld) {
            if (cameraTarget.IsVisible()) {
                GeometryUtility.CalculateFrustumPlanes(photoCamera, planes);
                if (GeometryUtility.TestPlanesAABB(planes, cameraTarget.GetBounds())) {
                    CameraTargetData.Wrapper wrapper = cameraTarget.GetCameraTargetData();
                    if (CalculateVisibility(cameraTarget, wrapper)) {
                        targets.Add(wrapper);
                    }
                }
            }
        }

        return targets;
    }

    protected float Smooth(float t) {
        t = Mathf.Clamp01(t);
        return -2*t*t*t + 3*t*t;
    }

    protected bool CalculateVisibility(CameraTarget cameraTarget, CameraTargetData.Wrapper wrapper) {
        Bounds bounds = cameraTarget.GetBounds();
        float viewProportion = Mathf.Atan((bounds.extents.magnitude*2)/Vector3.Distance(photoCamera.transform.position, cameraTarget.transform.position))/(photoCamera.fieldOfView*Mathf.Deg2Rad);

        float v = Smooth(Smooth(Mathf.Sqrt(viewProportion)));
        wrapper.MultiplyVisibility(v);
        if (!wrapper.PassesVisibilityCheck()) {
            return false;
        }

        //check if object is occluded by another by looking at the depth map
        float minX = 1;
        float maxX = 0;
        float minY = 1;
        float maxY = 0;
        float depth = 0;

        //find 8 corners of bounding cube, then figure out the 4 corners of a viewport square that encloses the 8 corners
        for (int i = 0; i < 8; i++) {
            Vector3 pos = photoCamera.WorldToViewportPoint(bounds.center + new Vector3(bounds.extents.x*((i&1)-0.5f)*2, bounds.extents.y*((i&2)-1), bounds.extents.z*((i&4)-2f)/2));
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
            depth += pos.z/8;
        }
        //averaged depth of the 8 points is along the middle of the object, however usually the visible surface will be a little infront
        bounds.IntersectRay(new Ray(bounds.center, photoCamera.transform.rotation*Vector3.forward), out float intersectDistance);
        depth += intersectDistance;

        //reduce visibility the more of the object is out of frame
        float outOfFrame = 1;
        if (minX < 0) outOfFrame *= 1-((- minX)/(maxX-minX));
        if (maxX > 1) outOfFrame *= 1-((maxX-1)/(maxX-minX));
        if (minY < 0) outOfFrame *= 1-((- minY)/(maxY-minY));
        if (maxY > 1) outOfFrame *= 1-((maxY-1)/(maxY-minY));

        wrapper.MultiplyVisibility(outOfFrame);
        if (!wrapper.PassesVisibilityCheck()) {
            return false;
        }

        int minXi = minX < 0 ? 0   : Mathf.RoundToInt(minX*256);
        int maxXi = maxX > 1 ? 256 : Mathf.RoundToInt(maxX*256);
        int minYi = minY < 0 ? 0   : Mathf.RoundToInt(minY*256);
        int maxYi = maxY > 1 ? 256 : Mathf.RoundToInt(maxY*256);

        //only render depth now since it might not have been needed, if a previous object in the photo already rendered it dont rerender
        if (!depthRendered) {
            RenderDepth();
            depthRendered = true;
        }

        //get the ratio of covered pixels to pixels that are on the object
        int onObject = 0;
        int covered = 0;

        for (int x = minXi; x <= maxXi; x++) {
            for (int y = minYi; y <= maxYi; y++) {
                float depthAtPos = depthTex.GetPixel(x, y).r*photoCamera.farClipPlane;
                
                Vector3 worldPos = photoCamera.ViewportToWorldPoint(new Vector3(x/256f, y/256f, depthAtPos));
                float distance = Vector3.Distance(bounds.ClosestPoint(worldPos), worldPos);

                if (distance <= 0.5f) {
                    onObject++;
                } else if (depthAtPos < depth) {
                    covered++;
                }
            }
        }

        if (covered > 0) {
            float covering = Mathf.Clamp01(onObject/(float)covered);
            wrapper.MultiplyVisibility(covering*covering);
        }
        
        return wrapper.PassesVisibilityCheck();
    }

    protected void RenderDepth() {
        depthMaterial.SetFloat("_AspectRatio", Screen.height / (float)Screen.width);
        Graphics.Blit(depthTex, depthRenderTex, depthMaterial);
        ReadPixels(depthRenderTex, depthTex);
    }
}
