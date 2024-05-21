using System.Collections.Generic;
using ImageBurner;
using UnityEngine;

public class ImageMetadata {
    public byte version = 0;
    public Vector3 position;
    public Vector2 rotation;
    public float fov;
    public Dictionary<int, List<CameraTargetData.Wrapper>> targets;
 
    public void Encode(Encoder encoder) {
        encoder.EncodeByte(version);

        DataTypes.EncodeVector3(encoder, position);
        DataTypes.EncodeVector2(encoder, rotation);
        DataTypes.EncodeFloat(encoder, fov);
        

        List<CameraTargetData.Wrapper> targetsList = new List<CameraTargetData.Wrapper>();
        foreach (int id in targets.Keys) {
            targetsList.AddRange(targets[id]);
        }

        DataTypes.EncodeByteArrayListStart(encoder, targetsList.Count, CameraTargetData.Wrapper.GetByteLength());
        foreach (CameraTargetData.Wrapper wrapper in targetsList) {
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
        List<CameraTargetData.Wrapper> targetsList = new List<CameraTargetData.Wrapper>(length);
        for (int x = 0; x < length; x++) {
            CameraTargetData.Wrapper wrapper = new CameraTargetData.Wrapper();
            wrapper.Decode(decoder);
            if (wrapper.cameraTargetData != null) {
                targetsList.Add(wrapper);
            }
        }
        SetTargets(targetsList);

        decoder.Close();
    }

    public void SetTargets(List<CameraTargetData.Wrapper> targetsList) {
        targets = new Dictionary<int, List<CameraTargetData.Wrapper>>();

        targetsList.Sort();
        targetsList.Reverse();

        int currentId = -1;
        foreach (CameraTargetData.Wrapper wrapper in targetsList) {
            int newId = wrapper.cameraTargetData.GetCombinedID();
            if (newId != currentId) {
                currentId = newId;
                targets[currentId] = new List<CameraTargetData.Wrapper>() {wrapper};
            } else {
                targets[currentId].Add(wrapper);
            }
        }
    }

    public bool PassesCountRequirement(int id) {
        return targets.TryGetValue(id, out var result) && result.Count >= result[0].cameraTargetData.requiredCount;
    }

    public string GetInfoText() {
        string s = "";
        foreach (int i in targets.Keys) {
            List<CameraTargetData.Wrapper> list = targets[i];
            s += list[0].cameraTargetData.displayName.Replace("#", list.Count.ToString())+"\n";
        }
        return s;
    }
}