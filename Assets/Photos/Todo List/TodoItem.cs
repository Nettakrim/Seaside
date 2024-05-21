using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TodoItem : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI text;

    [SerializeField] protected Image image;
    [SerializeField] protected Sprite incomplete;
    [SerializeField] protected Sprite complete;

    protected CameraTargetData cameraTargetData;
    protected bool isComplete = false;

    public void SetTargetData(CameraTargetData data) {
        cameraTargetData = data;
        text.text = data.displayName.Replace("#", cameraTargetData.requiredCount.ToString());
    }

    public void SetComplete(bool isComplete) {
        this.isComplete = isComplete;
        image.sprite = isComplete ? complete : incomplete;
    }

    public bool IsComplete() {
        return isComplete;
    }

    public bool UpdateComplete(List<GalleryPhoto> photos) {
        SetComplete(false);
        if (cameraTargetData == null) {
            return true;
        }
        foreach (GalleryPhoto photo in photos) {
            if (photo.ContainsTarget(cameraTargetData)) {
                SetComplete(true);
                return true;
            }
        }
        return false;
    }

    public bool IsType(CameraTargetData cameraTargetData) {
        return this.cameraTargetData == cameraTargetData;
    }
}
