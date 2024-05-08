using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager instance;

    [SerializeField] protected List<CameraTargetData> targets;

    [NonSerialized] public List<CameraTarget> cameraTargets = new List<CameraTarget>();

    public void Awake() {
        instance = this;
    }

    public CameraTargetData GetCameraTargetData(string id) {
        foreach (CameraTargetData target in targets) {
            if (target.id == id) return target;
        }
        return null;
    }
}
