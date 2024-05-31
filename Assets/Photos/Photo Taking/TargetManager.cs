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

    public CameraTargetData GetCameraTargetDataFromCombinedID(int id) {
        // combined id looks like 00000000ccccccccccccccccbbbbbbbb, where c is the bits of the char, and b is the bits of the byte
        return GetCameraTargetDataFromID((char)(id>>8), (byte)(id&255));
    }

    public List<CameraTargetData> GetCameraTargetDatas() {
        return targetDatas;
    }
}
