using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImageBurner;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    protected ImageMetadata metadata;

    [SerializeField] protected RawImage image;

    [SerializeField] protected GameObject goalMarker;

    protected FileInfo file;

    public bool Initialise(Texture2D tex, ImageMetadata potentialMetadata, FileInfo file) {
        if (potentialMetadata == null) {
            metadata = new ImageMetadata();
            Decoder decoder = (Decoder)tex;
            if (decoder.IsValid()) {
                metadata.Decode(decoder);
            } else {
                return false;
            }
        } else {
            metadata = potentialMetadata;
            metadata.OnSave();
        }

        image.texture = tex;
        this.file = file;

        // puts a little checkmark if the photo has any targets that complete a todo
        foreach (int id in metadata.targets.Keys) {
            if (metadata.PassesCountRequirement(id)) {
                goalMarker.SetActive(true);
                break;
            }
        }
        return true;
    }

    public void Teleport(PlayerController playerController) {
        playerController.SetPositionAndRotation(metadata.position, metadata.rotation);
        playerController.SetFlying((metadata.flags&1) == 1, Vector3.zero);
    }

    public float GetFov() {
        return metadata.fov;
    }

    public Texture GetTexture() {
        return image.texture;
    }

    public bool ContainsTarget(CameraTargetData cameraTargetData) {
        return metadata.PassesCountRequirement(cameraTargetData.GetCombinedID());
    }

    public void OnClick() {
        PhotoManager.instance.gallery.OnClickGalleryPhoto(this);
    }

    public Texture Destroy() {
        Destroy(gameObject);
        file.Delete();
        return image.texture;
    }

    public string GetInfoText() {
        return metadata.GetInfoText();
    }
}
