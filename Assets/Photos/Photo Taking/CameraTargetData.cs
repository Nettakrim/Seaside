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

    public class Wrapper {
        public CameraTargetData cameraTargetData;
        public float viewProportion;

        public void Encode(Encoder encoder) {
            DataTypes.EncodeChar(encoder, cameraTargetData.idChar);
            encoder.EncodeByte(cameraTargetData.idByte);
            DataTypes.EncodeFloat(encoder, viewProportion);
            return ;
        }

        public void Decode(Decoder decoder) {
            char c = DataTypes.DecodeChar(decoder);
            byte b = decoder.DecodeByte();
            cameraTargetData = TargetManager.instance.GetCameraTargetData(c, b);
            viewProportion = DataTypes.DecodeFloat(decoder);
        }

        public static int GetByteLength() {
            return 7;
        }
    }
}