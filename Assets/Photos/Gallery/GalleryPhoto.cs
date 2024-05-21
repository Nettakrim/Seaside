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

    public bool Initialise(Texture2D tex, ImageMetadata metadata, FileInfo file) {
        if (metadata == null) {
            this.metadata = new ImageMetadata();
            Decoder decoder = (Decoder)tex;
            if (decoder.IsValid()) {
                this.metadata.Decode(decoder);
            } else {
                return false;
            }
        } else {
            this.metadata = metadata;
        }

        image.texture = tex;
        this.file = file;

        foreach (int id in this.metadata.targets.Keys) {
            if (this.metadata.PassesCountRequirement(id)) {
                goalMarker.SetActive(true);
                break;
            }
        }
        return true;
    }

    public void Teleport(PlayerController playerController) {
        playerController.SetPositionAndRotation(metadata.position, metadata.rotation);
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

    public void Destroy() {
        Destroy(image.texture);
        Destroy(gameObject);
        file.Delete();
    }

    public string GetInfoText() {
        return metadata.GetInfoText();
    }
}
