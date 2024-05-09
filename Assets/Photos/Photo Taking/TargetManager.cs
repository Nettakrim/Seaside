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

    public CameraTargetData GetCameraTargetData(string id, byte b) {
        foreach (CameraTargetData target in targets) {
            if (target.id == id && target.idByte == b) return target;
        }
        return null;
    }

    public List<CameraTargetData> GetCameraTargetDatas() {
        return targets;
    }
}
