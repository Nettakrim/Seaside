using ImageBurner;
using UnityEngine;

public class ImageMetadata {
    public byte version = 0;
    public Vector3 position;
    public Vector2 rotation;
    public float fov;
 
    public void Encode(Encoder encoder) {
        encoder.EncodeByte(version);

        DataTypes.EncodeVector3(encoder, position);
        DataTypes.EncodeVector2(encoder, rotation);
        DataTypes.EncodeFloat(encoder, fov);

        encoder.Close();
    }

    public void Decode(Decoder decoder) {
        version = decoder.DecodeByte();
        
        position = DataTypes.DecodeVector3(decoder);
        rotation = DataTypes.DecodeVector2(decoder);
        fov = DataTypes.DecodeFloat(decoder);

        decoder.Close();
    }
}