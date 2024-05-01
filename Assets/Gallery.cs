using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Gallery : MonoBehaviour
{
    private List<GalleryPhoto> photos;

    [SerializeField] private GalleryPhoto galleryPhotoPrefab;
    [SerializeField] private Transform photoLayoutParent;

    private void Start() {
        LoadFromFiles();
    }

    public void LoadFromFiles() {
        photos = new List<GalleryPhoto>();

        foreach (string name in Directory.GetFiles(Application.persistentDataPath, "*.png")) {
            byte[] bytes = File.ReadAllBytes(name);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            ImageConversion.LoadImage(tex, bytes);

            AddImage(tex);
        }
    }

    public void AddImage(Texture2D tex) {
        GalleryPhoto galleryPhoto = Instantiate(galleryPhotoPrefab, photoLayoutParent);
        galleryPhoto.Initialise(tex);
        tex.Apply(false, true);
    }
}
