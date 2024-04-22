using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoTaking : MonoBehaviour
{
    [SerializeField] private Camera photoCamera;

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            photoCamera.Render();
        }
    }
}
