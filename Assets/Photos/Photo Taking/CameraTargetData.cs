using System;
using System.Collections;
using System.Collections.Generic;
using ImageBurner;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class CameraTargetData : ScriptableObject {
    public char idChar;
    public byte idByte;
    public string displayName;

    public float visibilityThreshold = 0.1f;

    public class Wrapper {
        public CameraTargetData cameraTargetData;
        public float visibility;

        public void Encode(Encoder encoder) {
            DataTypes.EncodeChar(encoder, cameraTargetData.idChar);
            encoder.EncodeByte(cameraTargetData.idByte);
            DataTypes.EncodeFloat(encoder, visibility);
            return ;
        }

        public void Decode(Decoder decoder) {
            char c = DataTypes.DecodeChar(decoder);
            byte b = decoder.DecodeByte();
            cameraTargetData = TargetManager.instance.GetCameraTargetDataFromID(c, b);
            visibility = DataTypes.DecodeFloat(decoder);
        }

        public static int GetByteLength() {
            return 7;
        }

        public bool PassesVisibilityCheck() {
            return visibility > cameraTargetData.visibilityThreshold;
        }

        public void MultiplyVisibility(float multiplier) {
            visibility *= multiplier;
        }
    }
}