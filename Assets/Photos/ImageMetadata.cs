using System.Collections.Generic;
using ImageBurner;
using UnityEngine;

public class ImageMetadata {
    public byte version = 0;
    public Vector3 position;
    public Vector2 rotation;
    public float fov;
    public List<CameraTargetData.Wrapper> targets;
 
    public void Encode(Encoder encoder) {
        encoder.EncodeByte(version);

        DataTypes.EncodeVector3(encoder, position);
        DataTypes.EncodeVector2(encoder, rotation);
        DataTypes.EncodeFloat(encoder, fov);
        
        DataTypes.EncodeByteArrayListStart(encoder, targets.Count, CameraTargetData.Wrapper.GetByteLength());
        foreach (CameraTargetData.Wrapper wrapper in targets) {
            wrapper.Encode(encoder);
        }

        encoder.Close();
    }

    public void Decode(Decoder decoder) {
        version = decoder.DecodeByte();
        
        position = DataTypes.DecodeVector3(decoder);
        rotation = DataTypes.DecodeVector2(decoder);
        fov = DataTypes.DecodeFloat(decoder);

        (int length, int size) = DataTypes.DecodeByteArrayListStart(decoder);
        targets = new List<CameraTargetData.Wrapper>(length);
        for (int x = 0; x < length; x++) {
            CameraTargetData.Wrapper wrapper = new CameraTargetData.Wrapper();
            wrapper.Decode(decoder);
            if (wrapper.cameraTargetData != null) {
                targets.Add(wrapper);
            }
        }

        decoder.Close();
    }

    public string GetInfoText() {
        string s = "";
        foreach (CameraTargetData.Wrapper wrapper in targets) {
            s += wrapper.cameraTargetData.displayName+" "+wrapper.visibility+"\n";
        }
        return s;
    }
}