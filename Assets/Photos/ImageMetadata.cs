using System.Collections.Generic;
using ImageBurner;
using UnityEngine;

public class ImageMetadata {
    public byte version = 0;
    public Vector3 position;
    public Vector2 rotation;
    public float fov;
    public byte flags;
    public Dictionary<int, List<CameraTargetData.Wrapper>> targets;
    bool moreToSee;
 
    public void Encode(Encoder encoder) {
        // a version is stored just in case a breaking change is made in the future, that way images could be automatically updated
        encoder.EncodeByte(version);

        // position rotation and fov are needed for teleportation
        DataTypes.EncodeVector3(encoder, position);
        DataTypes.EncodeVector2(encoder, rotation);
        DataTypes.EncodeFloat(encoder, fov);
        encoder.EncodeByte(flags);
        
        // storing all the targets is a bit excessive - it could just be a list of booleans
        // but at only 7 bytes per target it can store 4600 target wrappers, so it doesnt really matter
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
        flags = decoder.DecodeByte();

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

    public void SetTargetVisibility(Visibility.VisibilityResult visibilityResult) {
        SetTargets(visibilityResult.visible);

        // checks if there are targets that only slightly failed the visbility check and arent already passing elsewhere in the image
        // this is used to give a prompt to the player, and is only tracked up until the image is saved
        foreach (CameraTargetData.Wrapper wrapper in visibilityResult.misses) {
            if (!PassesCountRequirement(wrapper.cameraTargetData.GetCombinedID())) {
                moreToSee = true;
                return;
            }
        }
    }

    public void OnSave() {
        moreToSee = false;
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
        string info = "";
        foreach (int i in targets.Keys) {
            List<CameraTargetData.Wrapper> list = targets[i];
            CameraTargetData cameraTargetData = list[0].cameraTargetData;
            bool isNew = list.Count >= cameraTargetData.requiredCount && !PhotoManager.instance.gallery.TodoIsComplete(cameraTargetData);

            string s = cameraTargetData.displayName.Replace("#", list.Count.ToString());

            if (isNew) {
                info += "<i>"+s+"!</i>";
            } else {
                info += s;
            }

            info += "\n";

        }
        if (moreToSee) {
            info += "Get a clear view...";
            if (targets.Count == 0) {
                info += "\n- Zoom in\n- Move closer";
            }
        }
        return info;
    }
}