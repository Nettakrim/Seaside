using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ImageBurner;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Gallery : MonoBehaviour
{
    [SerializeField] private PhotoManager manager;

    private List<GalleryPhoto> photos;

    [SerializeField] private GalleryPhoto galleryPhotoPrefab;
    [SerializeField] private Transform photoLayoutParent;

    [SerializeField] private GameObject gallery;

    private GalleryPhoto selectedPhoto;
    [SerializeField] private RawImage selectedImage;

    private int currentPhoto;

    [SerializeField] private RawImage loopImage;

    private void Awake() {
        LoadFromFiles();
        if (photos.Count > 0) {
            photos[Random.Range(0, photos.Count)].Teleport(manager.player);
        } else {
            manager.player.TeleportToDefaultSpawnPosition();
        }
    }

    public void LoadFromFiles() {
        photos = new List<GalleryPhoto>();

        string directory = GetSaveDirectory();
        if (Directory.Exists(directory)) {
            foreach (FileInfo file in GetFilesNumerically(new DirectoryInfo(directory), "*.png")) {
                byte[] bytes = File.ReadAllBytes(file.FullName);
                Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
                tex.filterMode = FilterMode.Point;
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

        galleryPhoto.testButton.onClick.AddListener(delegate {OnClickGalleryPhoto(galleryPhoto);});
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

    public void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            SetCurrentPhoto(currentPhoto+1);
            UpdateGrid();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            SetCurrentPhoto(currentPhoto-1);
            UpdateGrid();
        }
    }

    public void UpdateGrid() {
        for (int i = 0; i < photos.Count; i++) {
            photos[i].transform.SetSiblingIndex(i);
        }

        for (int i = 0; i < (currentPhoto+((photos.Count+1)/2))%photos.Count; i++) {
            photos[i].transform.SetAsLastSibling();
        }

        selectedPhoto = photos[currentPhoto];
        selectedImage.texture = selectedPhoto.GetTexture();
        selectedImage.gameObject.SetActive(true);

        if (photos.Count%2 == 0) {
            loopImage.gameObject.SetActive(true);
            GalleryPhoto galleryPhoto = photos[(currentPhoto+(photos.Count/2))%photos.Count];
            loopImage.texture = galleryPhoto.GetTexture();
            Button button = loopImage.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate {OnClickGalleryPhoto(galleryPhoto);});
            loopImage.transform.SetAsLastSibling();
        } else {
            loopImage.gameObject.SetActive(false);
        }
    }

    public void SetCurrentPhoto(int to) {
        currentPhoto = (to+photos.Count)%photos.Count;
    }

    public void OpenGallery() {
        gallery.SetActive(true);
        selectedImage.gameObject.SetActive(false);
        manager.player.SetMovementLock(true);
        manager.player.SetRotationSpeed(0);
        UpdateGrid();
    }

    public void CloseGallery() {
        gallery.SetActive(false);
        manager.player.SetMovementLock(false);
        manager.player.SetRotationSpeed(1);
    }

    public void OnClickGalleryPhoto(GalleryPhoto galleryPhoto) {
        currentPhoto = photos.IndexOf(galleryPhoto);
        UpdateGrid();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void TeleportToSelectedPhoto() {
        selectedPhoto.Teleport(manager.player);
        manager.SetMode(PhotoManager.Mode.Walking);
    }

    //https://stackoverflow.com/questions/12077182/c-sharp-sort-files-by-natural-number-ordering-in-the-name
    public static FileInfo[] GetFilesNumerically(DirectoryInfo directory, string searchPattern, int numberPadding = 4) {
        return directory.GetFiles(searchPattern).OrderBy(file =>
            Regex.Replace(file.Name, @"\d+", match => match.Value.PadLeft(numberPadding, '0'))
        ).ToArray();
    }
}
