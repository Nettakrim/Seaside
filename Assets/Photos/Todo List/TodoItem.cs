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

    public void SetTargetData(CameraTargetData data) {
        cameraTargetData = data;
        text.text = data.displayName;
    }

    public void SetComplete(bool isComplete) {
        image.sprite = isComplete ? complete : incomplete;
    }

    public bool CheckComplete(List<GalleryPhoto> photos) {
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
}
