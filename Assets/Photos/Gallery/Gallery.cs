using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    [SerializeField] protected TextMeshProUGUI hintText;

    protected bool ready;
    protected bool teleporting;

    [SerializeField] protected GameObject teleport;
    [SerializeField] protected GameObject delete;

    [SerializeField] protected RandomAudioSource deleteSound;
    [SerializeField] protected RandomAudioSource teleportSound;

    protected void Awake() {
        LoadFromFiles();
        
        foreach (CameraTargetData target in TargetManager.instance.GetCameraTargetDatas()) {
            TodoItem todoItem = Instantiate(todoItemPrefab, todoList);
            todoItem.SetTargetData(target);
            todoItems.Add(todoItem);
        }
        UpdateTodoList();
        hintText.transform.SetAsLastSibling();
        manager.UpdateCompletion();

        if (photos.Count > 0) {
            SelectRandom();
            photos[currentPhoto].Teleport(manager.player);
        } else {
            manager.player.TeleportToDefaultSpawnPosition();
            manager.NextTutorialStep();
        }

        ready = true;
        teleporting = false;
    }

    public void LoadFromFiles() {
        photos = new List<GalleryPhoto>();

        List<SaveData> files = SaveManager.instance.LoadFiles();
        foreach (SaveData saveData in files) {
            AddImageToGallery(saveData, null);
        }
    }

    protected void AddImageToGallery(SaveData saveData, ImageMetadata metadata) {
        // gallery photos can fail if the encoded data is malformed
        bool success = saveData.SetMetadata(metadata);
        if (!success) {
            saveData.DeleteTexture();
            return;
        }

        GalleryPhoto galleryPhoto = Instantiate(galleryPhotoPrefab, photoLayoutParent);
        galleryPhoto.Initialise(saveData);
        photos.Add(galleryPhoto);
    }

    public void SaveNewImage(Texture2D tex, ImageMetadata imageMetadata) {
        imageMetadata.Encode((Encoder)tex);

        SaveData saveData = SaveManager.instance.SaveImage(tex);

        // the metadata is sent directly here, since theres no point in decoding the data when you already have it
        AddImageToGallery(saveData, imageMetadata);
    }

    public void Update() {
        if (manager.currentMode == PhotoManager.Mode.Gallery) {
            if (ready && InputManager.instance.moveX.GetDown()) {
                if (InputManager.instance.moveX.rawValue > 0) {
                    SetCurrentPhoto(currentPhoto+1);
                    UpdateGrid();
                } else {
                    SetCurrentPhoto(currentPhoto-1);
                    UpdateGrid();
                }
            }

            ready = !teleporting;
        }
    }

    public void UpdateGrid() {
        UpdateTodoList();

        if (photos.Count == 0) {
            loopImage.gameObject.SetActive(false);
            selectedImage.gameObject.SetActive(false);
            selectedInfo.SetActive(false);
            cameraPrompt.SetActive(true);
            exitPrompt.SetActive(false);
            cyclePrompt.SetActive(false);
            return;
        } else {
            cameraPrompt.SetActive(false);
            exitPrompt.SetActive(photos.Count == 1);
            cyclePrompt.SetActive(photos.Count == 2 || photos.Count == 3);
        }

        // reset order then cycle them by the amount needed
        // this could probably be done in a single loop, but this is simpler
        for (int i = 0; i < photos.Count; i++) {
            photos[i].transform.SetSiblingIndex(i);
        }

        for (int i = 0; i < (currentPhoto+((photos.Count+1)/2))%photos.Count; i++) {
            photos[i].transform.SetAsLastSibling();
        }

        selectedPhoto = photos[currentPhoto];
        selectedImage.texture = selectedPhoto.GetSaveData().tex;
        selectedImage.gameObject.SetActive(true);
        selectedInfo.SetActive(true);
        selectedGoals.text = selectedPhoto.GetInfoText();

        // if theres an even amount of photos a copy of the endmost one is added on the other side, that way the ui is centered
        if (photos.Count%2 == 0) {
            loopImage.gameObject.SetActive(true);
            GalleryPhoto galleryPhoto = photos[(currentPhoto+(photos.Count/2))%photos.Count];
            loopImage.texture = galleryPhoto.GetSaveData().tex;
            loopButton.onClick.RemoveAllListeners();
            loopButton.onClick.AddListener(delegate {OnClickGalleryPhoto(galleryPhoto);});
            loopImage.transform.GetChild(0).gameObject.SetActive(galleryPhoto.transform.GetChild(0).gameObject.activeSelf);
            loopImage.transform.SetAsLastSibling();
        } else {
            loopImage.gameObject.SetActive(false);
        }

        teleporting = false;

        ResetDeleteConfirm();
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
        InputManager.instance.SetLost(teleport);
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
        teleportSound.PlayRandom();
        EventSystem.current.SetSelectedGameObject(null);
        teleportAnimation.Play("TeleportEnter");
    }

    public void Teleport() {
        selectedPhoto.Teleport(manager.player);
        manager.photoTaking.SetFov(selectedPhoto.GetFov());
        manager.player.SetRotationSpeed(0);
        // the teleporting bool prevents using A/D to cycle images while the image is fading out
        teleporting = true;
    }

    public void TeleportEnd() {
        manager.SetMode(PhotoManager.Mode.Walking);
        manager.player.SetRotationSpeed(1);
        teleporting = false;
    }

    public void DeleteWithConfirm(Transform button) {
        GameObject start = button.GetChild(0).gameObject;
        GameObject confirm = button.GetChild(1).gameObject;
        bool confirmed = confirm.activeSelf;

        start.SetActive(confirmed);
        confirm.SetActive(!confirmed);

        if (confirmed) {
            DeleteSelected(true);
            deleteSound.PlayRandom();
        }
    }

    private void ResetDeleteConfirm() {
        delete.transform.GetChild(0).gameObject.SetActive(true);
        delete.transform.GetChild(1).gameObject.SetActive(false);
    }

    public Texture DeleteSelected(bool destroyTexture) {
        // deletes all data associated with the selected image, by default it also gets rid of the texture from memory
        // but the campfire needed to be able to access it, so the destroyTexture parameter makes it return the texture instead

        EventSystem.current.SetSelectedGameObject(null);
        if (photos.Count == 0) return null;
        GalleryPhoto photo = photos[currentPhoto];
        photos.Remove(photo);
        Destroy(photo.gameObject);

        SaveData saveData = photo.GetSaveData();
        if (destroyTexture) saveData.DeleteTexture();
        saveData.DeleteFile();

        SetCurrentPhoto(currentPhoto-1);
        UpdateGrid();
        return destroyTexture ? null : saveData.tex;
    }

    public void SelectRandom() {
        SetCurrentPhoto(Random.Range(0, photos.Count));
    }

    public int PhotoCount() {
        return photos.Count;
    }

    protected void UpdateTodoList() {
        int c = 0;
        TodoItem uncomplete = null;
        foreach (TodoItem todoItem in todoItems) {
            if (todoItem.UpdateComplete(photos)) {
                c++;
            } else {
                uncomplete = todoItem;
            }
        }
        if (c == todoItems.Count) {
            foreach (TodoItem todoItem in todoItems) {
                todoItem.SetComplete(true);
            }
        }
        
        if (c == todoItems.Count-1) {
            hintText.gameObject.SetActive(true);
            hintText.text = uncomplete.GetHint();
        } else {
            hintText.gameObject.SetActive(false);
        }
    }

    public bool TodoIsComplete(CameraTargetData cameraTargetData) {
        foreach (TodoItem todo in todoItems) {
            if (todo.IsType(cameraTargetData)) {
                return todo.IsComplete();
            }
        }
        return false;
    }

    public bool AllGoalsComplete() {
        foreach (TodoItem todo in todoItems) {
            if (!todo.IsComplete()) {
                return false;
            }
        }
        return true;
    }
}
