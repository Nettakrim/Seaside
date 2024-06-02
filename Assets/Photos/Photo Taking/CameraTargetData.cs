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

    public float visibilityThreshold = 0.25f;
    public float nearMissThreshold = 0.15f;
    public int requiredCount = 1;

    public string hint;

    public int GetCombinedID() {
        return (idChar << 8) + idByte;
    }

    public class Wrapper : IComparable<Wrapper> {
        public CameraTargetData cameraTargetData;
        public float visibility;

        public void Encode(Encoder encoder) {
            DataTypes.EncodeChar(encoder, cameraTargetData.idChar);
            encoder.EncodeByte(cameraTargetData.idByte);
            DataTypes.EncodeFloat(encoder, visibility);
        }

        public void Decode(Decoder decoder) {
            char c = DataTypes.DecodeChar(decoder);
            byte b = decoder.DecodeByte();
            cameraTargetData = TargetManager.instance.GetCameraTargetDataFromID(c, b);
            visibility = DataTypes.DecodeFloat(decoder);
        }

        public static int GetByteLength() {
            // it is very important that this is correct
            // +2 idChar
            // +1 idByte
            // +4 visiblity
            return 7;
        }

        public bool PassesVisibilityCheck() {
            return visibility > cameraTargetData.visibilityThreshold;
        }

        public bool PassesNearMissCheck() {
            return visibility > cameraTargetData.nearMissThreshold;
        }

        public void MultiplyVisibility(float multiplier) {
            visibility *= multiplier;
        }

        public int CompareTo(Wrapper other) {
            // sort by id, subsort by visibility
            int compare = cameraTargetData.GetCombinedID().CompareTo(other.cameraTargetData.GetCombinedID());
            if (compare != 0) return compare;
            return visibility.CompareTo(other.visibility);
        }
    }
}