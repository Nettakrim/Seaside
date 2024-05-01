using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImageBurner;
using UnityEngine;

public class PhotoTaking : MonoBehaviour
{
    [SerializeField] protected Camera photoCamera;
    protected RenderTexture renderTexture;

    [SerializeField] protected Gallery gallery;

    protected ImageMetadata currentMetadata;

    protected void Start() {
        renderTexture = photoCamera.targetTexture;
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            TakePhoto();
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            SaveLastPhoto();
        }
    }

    protected void TakePhoto() {
        photoCamera.Render();
        currentMetadata = new ImageMetadata();
    }

    protected void SaveLastPhoto() {
        if (currentMetadata == null) {
            Debug.LogWarning("SaveLastPhoto() was called, but there is currently no unsaved photo!");
            return;
        }

        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, false);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = previous;

        gallery.SaveNewImage(tex, currentMetadata);
        currentMetadata = null;
    }
}
