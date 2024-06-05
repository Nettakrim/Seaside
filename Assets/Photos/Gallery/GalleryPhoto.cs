using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    protected SaveData saveData;
    [SerializeField] protected RawImage image;

    [SerializeField] protected GameObject goalMarker;

    public void Initialise(SaveData saveData) {
        this.saveData = saveData;
        image.texture = saveData.tex;

        // puts a little checkmark if the photo has any targets that complete a todo
        foreach (int id in saveData.metadata.targets.Keys) {
            if (saveData.metadata.PassesCountRequirement(id)) {
                goalMarker.SetActive(true);
                break;
            }
        }
    }

    public void Teleport(PlayerController playerController) {
        playerController.SetPositionAndRotation(saveData.metadata.position, saveData.metadata.rotation);
        playerController.SetFlying((saveData.metadata.flags&1) == 1, Vector3.zero);
    }

    public float GetFov() {
        return saveData.metadata.fov;
    }

    public bool ContainsTarget(CameraTargetData cameraTargetData) {
        return saveData.metadata.PassesCountRequirement(cameraTargetData.GetCombinedID());
    }

    public void OnClick() {
        PhotoManager.instance.gallery.OnClickGalleryPhoto(this);
    }

    public string GetInfoText() {
        return saveData.metadata.GetInfoText();
    }

    public SaveData GetSaveData() {
        return saveData;
    }
}
