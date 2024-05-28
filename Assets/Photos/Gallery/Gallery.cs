using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ImageBurner;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Gallery : MonoBehaviour
{
    [SerializeField] protected PhotoManager manager;

    protected List<GalleryPhoto> photos;

    [SerializeField] protected GalleryPhoto galleryPhotoPrefab;
    [SerializeField] protected Transform photoLayoutParent;

    [SerializeField] protected GameObject gallery;

    protected GalleryPhoto selectedPhoto;

    protected int currentPhoto;

    [SerializeField] protected RawImage loopImage;
    [SerializeField] protected Button loopButton;

    [SerializeField] protected RawImage selectedImage;

    [SerializeField] protected Animator teleportAnimation;

    [SerializeField] protected Transform todoList;
    [SerializeField] protected TodoItem todoItemPrefab;

    [SerializeField] protected List<TodoItem> todoItems;

    [SerializeField] protected GameObject selectedInfo;
    [SerializeField] protected TextMeshProUGUI selectedGoals;

    [SerializeField] protected GameObject cameraPrompt;
    [SerializeField] protected GameObject exitPrompt;
    [SerializeField] protected GameObject cyclePrompt;

    protected bool ready;
    protected bool teleporting;

    protected void Awake() {
        LoadFromFiles();
        if (photos.Count > 0) {
            SelectRandom();
            photos[currentPhoto].Teleport(manager.player);
        } else {
            manager.player.TeleportToDefaultSpawnPosition();
            manager.NextTutorialStep();
        }
        
        foreach (CameraTargetData target in TargetManager.instance.GetCameraTargetDatas()) {
            TodoItem todoItem = Instantiate(todoItemPrefab, todoList);
            todoItem.SetTargetData(target);
            todoItems.Add(todoItem);
        }

        ready = true;
        teleporting = false;
    }

    public void LoadFromFiles() {
        photos = new List<GalleryPhoto>();

        string directory = GetSaveDirectory();
        if (Directory.Exists(directory)) {
            foreach (FileInfo file in GetFilesNumerically(new DirectoryInfo(directory), "*.png")) {
                byte[] bytes = File.ReadAllBytes(file.FullName);
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                ImageConversion.LoadImage(tex, bytes);

                AddImageToGallery(tex, null, file);
            }
        } else {
            Directory.CreateDirectory(directory);
        }
    }

    protected void AddImageToGallery(Texture2D tex, ImageMetadata metadata, FileInfo file) {
        GalleryPhoto galleryPhoto = Instantiate(galleryPhotoPrefab, photoLayoutParent);
        bool success = galleryPhoto.Initialise(tex, metadata, file);
        if (!success) {
            Destroy(galleryPhoto.gameObject);
            Destroy(tex);
            return;
        }
        tex.Apply(false, true);

        photos.Add(galleryPhoto);
    }

    public void SaveNewImage(Texture2D tex, ImageMetadata imageMetadata) {
        imageMetadata.Encode((Encoder)tex);

        string file = GetNextFilename();
        File.WriteAllBytes(file, tex.EncodeToPNG());

        AddImageToGallery(tex, imageMetadata, new FileInfo(file));
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
        if (manager.currentMode == PhotoManager.Mode.Gallery) {
            if (ready) {
                if (Input.GetKeyDown(KeyCode.D)) {
                    SetCurrentPhoto(currentPhoto+1);
                    UpdateGrid();
                }

                if (Input.GetKeyDown(KeyCode.A)) {
                    SetCurrentPhoto(currentPhoto-1);
                    UpdateGrid();
                }
            }

            if (Input.GetKey(KeyCode.Space)) {
                TeleportStart();
            }

            ready = !teleporting;
        }
    }

    public void UpdateGrid() {
        int c = 0;
        foreach (TodoItem todoItem in todoItems) {
            if (todoItem.UpdateComplete(photos)) {
                c++;
            }
        }
        if (c == todoItems.Count) {
            foreach (TodoItem todoItem in todoItems) {
                todoItem.SetComplete(true);
            }
        }


        if (photos.Count == 0) {
            loopImage.gameObject.SetActive(false);
            selectedImage.gameObject.SetActive(false);
            selectedInfo.SetActive(false);
            cameraPrompt.SetActive(true);
            return;
        } else {
            cameraPrompt.SetActive(false);
            exitPrompt.SetActive(photos.Count == 1);
            cyclePrompt.SetActive(photos.Count == 2 || photos.Count == 3);
        }

        for (int i = 0; i < photos.Count; i++) {
            photos[i].transform.SetSiblingIndex(i);
        }

        for (int i = 0; i < (currentPhoto+((photos.Count+1)/2))%photos.Count; i++) {
            photos[i].transform.SetAsLastSibling();
        }

        selectedPhoto = photos[currentPhoto];
        selectedImage.texture = selectedPhoto.GetTexture();
        selectedImage.gameObject.SetActive(true);
        selectedInfo.SetActive(true);
        selectedGoals.text = selectedPhoto.GetInfoText();

        if (photos.Count%2 == 0) {
            loopImage.gameObject.SetActive(true);
            GalleryPhoto galleryPhoto = photos[(currentPhoto+(photos.Count/2))%photos.Count];
            loopImage.texture = galleryPhoto.GetTexture();
            loopButton.onClick.RemoveAllListeners();
            loopButton.onClick.AddListener(delegate {OnClickGalleryPhoto(galleryPhoto);});
            loopImage.transform.GetChild(0).gameObject.SetActive(galleryPhoto.transform.GetChild(0).gameObject.activeSelf);
            loopImage.transform.SetAsLastSibling();
        } else {
            loopImage.gameObject.SetActive(false);
        }
    }

    public void SetCurrentPhoto(int to) {
        if (photos.Count == 0) {
            currentPhoto = 0;
            return;
        }
        currentPhoto = (to+photos.Count)%photos.Count;
    }

    public void OpenGallery() {
        ready = false;
        gallery.SetActive(true);
        manager.player.SetMovementLock(true);
        manager.player.SetRotationSpeed(0);
        manager.interactor.SetCanInteract(false);
        UpdateGrid();
    }

    public void CloseGallery() {
        gallery.SetActive(false);
        manager.player.SetMovementLock(false);
        manager.player.SetRotationSpeed(1);
        manager.interactor.SetCanInteract(true);
    }

    public void OnClickGalleryPhoto(GalleryPhoto galleryPhoto) {
        currentPhoto = photos.IndexOf(galleryPhoto);
        UpdateGrid();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void TeleportStart() {
        if (selectedPhoto == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        teleportAnimation.Play("TeleportEnter");
    }

    public void Teleport() {
        selectedPhoto.Teleport(manager.player);
        manager.photoTaking.SetFov(selectedPhoto.GetFov());
        manager.player.SetRotationSpeed(0);
        teleporting = true;
    }

    public void TeleportEnd() {
        manager.SetMode(PhotoManager.Mode.Walking);
        manager.player.SetRotationSpeed(1);
        teleporting = false;
    }

    public void DeleteSelected() {
        DeleteSelected(true);
    }

    public Texture DeleteSelected(bool destroyTexture) {
        EventSystem.current.SetSelectedGameObject(null);
        if (photos.Count == 0) return null;
        GalleryPhoto photo = photos[currentPhoto];
        photos.Remove(photo);
        Texture texture = photo.Destroy();
        if (destroyTexture) Destroy(texture);

        SetCurrentPhoto(currentPhoto-1);
        UpdateGrid();
        return destroyTexture ? null : texture;
    }

    public void SelectRandom() {
        SetCurrentPhoto(Random.Range(0, photos.Count));
    }

    //https://stackoverflow.com/questions/12077182/c-sharp-sort-files-by-natural-number-ordering-in-the-name
    public static FileInfo[] GetFilesNumerically(DirectoryInfo directory, string searchPattern, int numberPadding = 4) {
        return directory.GetFiles(searchPattern).OrderBy(file =>
            Regex.Replace(file.Name, @"\d+", match => match.Value.PadLeft(numberPadding, '0'))
        ).ToArray();
    }

    public int PhotoCount() {
        return photos.Count;
    }

    public bool TodoIsComplete(CameraTargetData cameraTargetData) {
        foreach (TodoItem todo in todoItems) {
            if (todo.IsType(cameraTargetData)) {
                return todo.IsComplete();
            }
        }
        return false;
    }
}
