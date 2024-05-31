using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Visibility {
    public static VisibilityResult GetVisibleCameraTargets(Camera photoCamera, PhotoTaking photoTaking, List<CameraTarget> targets) {
        VisibilityResult visibilityResult = new VisibilityResult();
        Plane[] planes = new Plane[6];

        foreach (CameraTarget cameraTarget in targets) {
            if (cameraTarget.IsVisible()) {
                GeometryUtility.CalculateFrustumPlanes(photoCamera, planes);
                if (GeometryUtility.TestPlanesAABB(planes, cameraTarget.GetBounds())) {
                    CameraTargetData.Wrapper wrapper = cameraTarget.GetCameraTargetData();
                    CalculateVisibility(cameraTarget, wrapper, photoCamera, photoTaking);
                    if (wrapper.PassesVisibilityCheck()) {
                        visibilityResult.visible.Add(wrapper);
                    } else if (wrapper.PassesNearMissCheck()) {
                        visibilityResult.misses.Add(wrapper);
                    }
                }
            }
        }

        return visibilityResult;
    }

    protected static float Smooth(float t) {
        t = Mathf.Clamp01(t);
        return -2*t*t*t + 3*t*t;
    }

    protected static void CalculateVisibility(CameraTarget cameraTarget, CameraTargetData.Wrapper wrapper, Camera photoCamera, PhotoTaking photoTaking) {
        Bounds bounds = cameraTarget.GetBounds();
        float viewProportion = Mathf.Atan((bounds.extents.magnitude*2)/Vector3.Distance(photoCamera.transform.position, cameraTarget.transform.position))/(photoCamera.fieldOfView*Mathf.Deg2Rad);

        float v = Smooth(Smooth(Mathf.Sqrt(viewProportion)));
        wrapper.MultiplyVisibility(v);
        if (!wrapper.PassesVisibilityCheck()) {
            return;
        }

        // check if object is occluded by another by looking at the depth map
        float minX = 1;
        float maxX = 0;
        float minY = 1;
        float maxY = 0;
        float depth = 0;

        // find 8 corners of bounding cube, then figure out the 4 corners of a viewport square that encloses the 8 corners
        for (int i = 0; i < 8; i++) {
            Vector3 pos = photoCamera.WorldToViewportPoint(bounds.center + new Vector3(bounds.extents.x*((i&1)-0.5f)*2, bounds.extents.y*((i&2)-1), bounds.extents.z*((i&4)-2f)/2));
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
            depth += pos.z/8;
        }
        // averaged depth of the 8 points is along the middle of the object, however usually the visible surface will be a little infront
        bounds.IntersectRay(new Ray(bounds.center, photoCamera.transform.rotation*Vector3.forward), out float intersectDistance);
        depth += intersectDistance;

        // reduce visibility the more of the object is out of frame
        float outOfFrame = 1;
        if (minX < 0) outOfFrame *= 1-((- minX)/(maxX-minX));
        if (maxX > 1) outOfFrame *= 1-((maxX-1)/(maxX-minX));
        if (minY < 0) outOfFrame *= 1-((- minY)/(maxY-minY));
        if (maxY > 1) outOfFrame *= 1-((maxY-1)/(maxY-minY));

        wrapper.MultiplyVisibility(outOfFrame);
        if (!wrapper.PassesVisibilityCheck()) {
            return;
        }

        int minXi = minX < 0 ? 0   : Mathf.RoundToInt(minX*256);
        int maxXi = maxX > 1 ? 256 : Mathf.RoundToInt(maxX*256);
        int minYi = minY < 0 ? 0   : Mathf.RoundToInt(minY*256);
        int maxYi = maxY > 1 ? 256 : Mathf.RoundToInt(maxY*256);

        // only render depth now since it might not have been needed, if a previous object in the photo already rendered it doesnt rerender
        Texture2D depthTex = photoTaking.RenderDepth();

        // get the ratio of covered pixels to pixels that are on the object
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
    }

    public class VisibilityResult {
        public List<CameraTargetData.Wrapper> visible = new List<CameraTargetData.Wrapper>();
        public List<CameraTargetData.Wrapper> misses = new List<CameraTargetData.Wrapper>();
    }
}