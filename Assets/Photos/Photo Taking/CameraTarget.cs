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
        return visible;
    }

    public Bounds GetBounds() {
        if (alternateBounds != null) {
            return alternateBounds.bounds;
        }
        return r.bounds;
    }

    public CameraTargetData.Wrapper GetCameraTargetData(Camera photoCamera) {
        CameraTargetData.Wrapper wrapper = new CameraTargetData.Wrapper();
        wrapper.cameraTargetData = cameraTargetData;
        wrapper.viewProportion = Mathf.Atan((GetBounds().extents.magnitude*2)/Vector3.Distance(photoCamera.transform.position, transform.position))/(photoCamera.fieldOfView*Mathf.Deg2Rad);
        return wrapper;
    }
}
