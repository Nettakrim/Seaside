using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    private ImageMetadata imageMetadata;

    [SerializeField] private RawImage image;

    public void Initialise(Texture2D tex) {
        byte[] bytes = ImageBurner.Decode(tex);
        imageMetadata = ImageMetadata.FromBytes(bytes);

        image.texture = tex;
    }
}
