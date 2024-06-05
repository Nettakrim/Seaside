using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImageBurner;
using UnityEngine;

public class SaveData
{
    public FileInfo file;
    public Texture2D tex;
    public ImageMetadata metadata;

    public SaveData(Texture2D tex) {
        this.tex = tex;
    }

    public void SetFile(FileInfo file) {
        this.file = file;
    }

    public bool SaveToFile(string filename) {
        if (file != null) {
            return false;
        }

        File.WriteAllBytes(filename, tex.EncodeToPNG());
        SetFile(new FileInfo(filename));
        return true;
    }

    public bool SetMetadata(ImageMetadata potentialMetadata) {
        if (metadata != null) {
            return true;
        }

        if (potentialMetadata == null) {
            metadata = new ImageMetadata();
            Decoder decoder = (Decoder)tex;
            if (decoder.IsValid()) {
                metadata.Decode(decoder);
            } else {
                return false;
            }
        } else {
            metadata = potentialMetadata;
            metadata.OnSave();
        }

        // at this point we never need to access the texture data on the cpu again
        // unless its unpersistent, in which case we may need it when toggling into persistent mode
        if (file != null) {
            ApplyTexture();
        }

        return true;
    }

    public void DeleteFile() {
        if (file != null) {
            file.Delete();
        }
        SaveManager.instance.RemoveSaveDataReference(this);
    }

    public void DeleteTexture() {
        GameObject.Destroy(tex);
        if (file == null) {
            SaveManager.instance.RemoveSaveDataReference(this);
        }
    }

    public void ApplyTexture() {
        tex.Apply(false, true);
    }
}