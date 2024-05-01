using System.Collections;
using System.Collections.Generic;
using ImageBurner;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    private ImageMetadata imageMetadata;

    [SerializeField] private RawImage image;

    public void Initialise(Texture2D tex) {
        ImageMetadata imageMetadata = new ImageMetadata();
        imageMetadata.Decode((Decoder)tex);

        image.texture = tex;
    }
}
