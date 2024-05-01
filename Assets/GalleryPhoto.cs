using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryPhoto : MonoBehaviour
{
    [SerializeField] private RawImage image;

    public void Initialise(Texture2D tex) {
        image.texture = tex;
    }
}
