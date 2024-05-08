using System;
using System.Collections;
using System.Collections.Generic;
using ImageBurner;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class CameraTargetData : ScriptableObject {
    public string id;
    public string displayName;

    private void OnValidate() {
        if (id.Length > idLength) {
            id = id.Substring(0, idLength);
            EditorUtility.SetDirty(this);
        }
    }

    private static int idLength = 2;

    public class Wrapper {
        public CameraTargetData cameraTargetData;
        public float viewProportion;

        public void Encode(Encoder encoder) {
            DataTypes.EncodeFixedLengthString(encoder, cameraTargetData.id, idLength);
            DataTypes.EncodeFloat(encoder, viewProportion);
            return ;
        }

        public void Decode(Decoder decoder) {
            cameraTargetData = TargetManager.instance.GetCameraTargetData(DataTypes.DecodeFixedLengthString(decoder, idLength));
            viewProportion = DataTypes.DecodeFloat(decoder);
        }

        public static int GetByteLength() {
            return 8;
        }
    }
}