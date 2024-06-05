using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private Dictionary<string, List<SaveData>> saveFiles;
    public bool persistentSave;

    public int saveSlot = 0;

    private void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);

        saveFiles = new Dictionary<string, List<SaveData>>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            SetPersistentSave(!persistentSave);
        }
    }

    public SaveData SaveImage(Texture2D tex) {
        SaveData saveData = new SaveData(tex);

        string area = SceneLoader.instance.GetCurrentAreaName();
        if (persistentSave) {
            saveData.SaveToFile(GetNextFilename(area));
        }
        saveFiles[area].Add(saveData);

        return saveData;
    }

    public List<SaveData> LoadFiles() {
        List<SaveData> saveDatas;

        string area = SceneLoader.instance.GetCurrentAreaName();
        if (!saveFiles.TryGetValue(area, out saveDatas)) {
            saveDatas = new List<SaveData>();
            saveFiles[area] = saveDatas;
        }

        saveDatas.RemoveAll(s => s.file != null);

        string directory = GetSaveDirectory(area);
        if (Directory.Exists(directory)) {
            foreach (FileInfo file in GetFilesNumerically(new DirectoryInfo(directory), "*.png")) {
                byte[] bytes = File.ReadAllBytes(file.FullName);
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Point;
                ImageConversion.LoadImage(tex, bytes);

                SaveData saveData = new SaveData(tex);
                saveData.SetFile(file);
                saveDatas.Add(saveData);
            }
        } else if (persistentSave) {
            Directory.CreateDirectory(directory);
        }

        return saveDatas;
    }


    private string GetNextFilename(string area) {
        int id;
        int count = saveFiles[SceneLoader.instance.GetCurrentAreaName()].Count;
        
        string path = GetSaveDirectory(area);
        if (File.Exists(path + (count+1).ToString() + ".png")) {
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
            id = count;
        }

        return GetSaveDirectory(area) + (id+1).ToString() + ".png";
    }

    private string GetSaveDirectory(string area) {
        return Application.persistentDataPath+"/"+saveSlot+"/"+area+"/";
    }

    // https://stackoverflow.com/questions/12077182/c-sharp-sort-files-by-natural-number-ordering-in-the-name
    private static FileInfo[] GetFilesNumerically(DirectoryInfo directory, string searchPattern, int numberPadding = 4) {
        return directory.GetFiles(searchPattern).OrderBy(file =>
            Regex.Replace(file.Name, @"\d+", match => match.Value.PadLeft(numberPadding, '0'))
        ).ToArray();
    }

    public void RemoveSaveDataReference(SaveData saveData) {
        foreach (List<SaveData> saveDatas in saveFiles.Values) {
            if (saveDatas.Remove(saveData)) {
                return;
            }
        }
    }

    public void SetPersistentSave(bool to) {
        if (!persistentSave && to) {
            foreach (string area in saveFiles.Keys) {

                string directory = GetSaveDirectory(area);
                if (!Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                string filename = GetNextFilename(area);
                foreach (SaveData saveData in saveFiles[area]) {
                    if (saveData.SaveToFile(filename)) {
                        saveData.ApplyTexture();
                        filename = GetNextFilename(area);
                    }
                }
            }
        }
        persistentSave = to;
    }
}
