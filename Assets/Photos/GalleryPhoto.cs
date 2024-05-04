using System.Collections;
using System.Collections.Generic;
using ImageBurner;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    private ImageMetadata metadata;

    [SerializeField] private RawImage image;

    public Button testButton;

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
}
