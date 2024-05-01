using System.Collections;
using System.Collections.Generic;
using ImageBurner;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    private ImageMetadata metadata;

    [SerializeField] private RawImage image;

    public void Initialise(Texture2D tex, ImageMetadata metadata) {
        if (metadata == null) {
            this.metadata = new ImageMetadata();
            this.metadata.Decode((Decoder)tex);
        } else {
            this.metadata = metadata;
        }

        image.texture = tex;
    }
}
