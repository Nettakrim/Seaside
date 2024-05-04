using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImageBurner;
using UnityEngine;

public class Gallery : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private List<GalleryPhoto> photos;

    [SerializeField] private GalleryPhoto galleryPhotoPrefab;
    [SerializeField] private Transform photoLayoutParent;

    private void Start() {
        LoadFromFiles();
        if (photos.Count > 0) {
            photos[Random.Range(0, photos.Count)].Teleport(player);
        }
    }

    public void LoadFromFiles() {
        photos = new List<GalleryPhoto>();

        string directory = GetSaveDirectory();
        if (Directory.Exists(directory)) {
            foreach (string name in Directory.GetFiles(directory, "*.png")) {
                byte[] bytes = File.ReadAllBytes(name);
                Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                ImageConversion.LoadImage(tex, bytes);

                AddImageToGallery(tex, null);
            }
        } else {
            Directory.CreateDirectory(directory);
        }
    }

    protected void AddImageToGallery(Texture2D tex, ImageMetadata metadata) {
        GalleryPhoto galleryPhoto = Instantiate(galleryPhotoPrefab, photoLayoutParent);
        bool success = galleryPhoto.Initialise(tex, metadata);
        if (!success) {
            Destroy(galleryPhoto.gameObject);
            Destroy(tex);
            return;
        }
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
        int id;
        
        string path = GetSaveDirectory();
        if (File.Exists(path + (photos.Count+1).ToString() + ".png")) {
            id = 0;
            string[] files = Directory.GetFiles(path, "*.png");
            foreach (string file in files) {
                string s = file[path.Length..^4];
                if (int.TryParse(s, out int i)) {
                    if (i > id) {
                        id = i;
                    }
                }
            }
        } else {
            id = photos.Count;
        }

        return GetSaveDirectory() + (id+1).ToString() + ".png";
    }

    public string GetSaveDirectory() {
        return Application.persistentDataPath + "/1/";
    }
}
