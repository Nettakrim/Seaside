using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager instance;

    [SerializeField] protected List<CameraTargetData> targetDatas;

    [NonSerialized] public List<CameraTarget> targetsInWorld = new List<CameraTarget>();

    public void Awake() {
        instance = this;
    }

    public CameraTargetData GetCameraTargetDataFromID(char c, byte b) {
        foreach (CameraTargetData target in targetDatas) {
            if (target.idChar == c && target.idByte == b) return target;
        }
        return null;
    }

    public List<CameraTargetData> GetCameraTargetDatas() {
        return targetDatas;
    }
}
