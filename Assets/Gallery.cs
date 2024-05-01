using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImageBurner;
using UnityEngine;

public class Gallery : MonoBehaviour
{
    private List<GalleryPhoto> photos;

    [SerializeField] private GalleryPhoto galleryPhotoPrefab;
    [SerializeField] private Transform photoLayoutParent;

    [SerializeField] private PlayerController player;

    private void Start() {
        LoadFromFiles();
    }

    public void LoadFromFiles() {
        photos = new List<GalleryPhoto>();

        foreach (string name in Directory.GetFiles(Application.persistentDataPath, "*.png")) {
            byte[] bytes = File.ReadAllBytes(name);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            ImageConversion.LoadImage(tex, bytes);

            AddImageToGallery(tex, null);
        }
    }

    protected void AddImageToGallery(Texture2D tex, ImageMetadata metadata) {
        GalleryPhoto galleryPhoto = Instantiate(galleryPhotoPrefab, photoLayoutParent);
        galleryPhoto.Initialise(tex, metadata);
        tex.Apply(false, true);

        galleryPhoto.testButton.onClick.AddListener(delegate {galleryPhoto.Teleport(player);});
        photos.Add(galleryPhoto);
    }

    public void SaveNewImage(Texture2D tex, ImageMetadata imageMetadata) {
        imageMetadata.Encode((Encoder)tex);
        File.WriteAllBytes(GetNextFilename(), tex.EncodeToPNG());

        AddImageToGallery(tex, imageMetadata);
    }

    protected string GetNextFilename() {
        return Application.persistentDataPath + "/" + photos.Count + ".png";
    }
}
