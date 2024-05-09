using System.Collections;
using System.Collections.Generic;
using ImageBurner;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    private ImageMetadata metadata;

    [SerializeField] private RawImage image;

    public bool Initialise(Texture2D tex, ImageMetadata metadata) {
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
        foreach (CameraTargetData.Wrapper wrapper in metadata.targets) {
            if (wrapper.cameraTargetData == cameraTargetData) {
                return true;
            }
        }
        return false;
    }

    public void OnClick() {
        PhotoManager.instance.gallery.OnClickGalleryPhoto(this);
    }
}
