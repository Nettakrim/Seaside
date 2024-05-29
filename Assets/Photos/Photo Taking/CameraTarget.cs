using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    protected bool visible;
    protected Renderer r;

    [SerializeField] protected CameraTargetData cameraTargetData;

    [SerializeField] protected Collider alternateBounds;
    
    protected void Awake() {
        r = GetComponent<Renderer>();
        if (TargetManager.instance != null) {
            TargetManager.instance.targetsInWorld.Add(this);

            if (cameraTargetData == null) {
                Debug.LogWarning("Camera Target "+name+" has no CameraTargetData assigned");
            } else if (!TargetManager.instance.GetCameraTargetDatas().Contains(cameraTargetData)) {
                Debug.LogWarning("Target Manager does not inculde "+cameraTargetData+" but "+name+" uses it");
            }
        }
    }

    protected void OnDestroy() {
        if (TargetManager.instance != null) {
            TargetManager.instance.targetsInWorld.Remove(this);
        }
    }

    protected void OnBecameVisible() {
        visible = true;
    }

    protected void OnBecameInvisible() {
        visible = false;
    }

    public bool IsVisible() {
        return visible || (r == null && gameObject.activeInHierarchy);
    }

    public Bounds GetBounds() {
        if (alternateBounds != null) {
            return alternateBounds.bounds;
        }
        return r.bounds;
    }

    public CameraTargetData.Wrapper GetCameraTargetData() {
        CameraTargetData.Wrapper wrapper = new CameraTargetData.Wrapper();
        wrapper.cameraTargetData = cameraTargetData;
        wrapper.visibility = 1;
        return wrapper;
    }
}
