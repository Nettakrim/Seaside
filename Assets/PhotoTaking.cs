using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhotoTaking : MonoBehaviour
{
    [SerializeField] private Camera photoCamera;
    private RenderTexture renderTexture;

    [SerializeField] private Gallery gallery;

    private int count;

    private void Start() {
        renderTexture = photoCamera.targetTexture;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            photoCamera.Render();
            SaveRenderTexture(count.ToString());
            count++;
        }
    }

    void SaveRenderTexture(string name) {
        Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, false);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = previous;

        byte[] bytes = tex.EncodeToPNG(); 
        
        string path = Application.persistentDataPath + "/" + name + ".png";
        File.WriteAllBytes(path, bytes);

        gallery.AddImage(tex);
    }
}
