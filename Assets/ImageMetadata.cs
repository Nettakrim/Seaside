public class ImageMetadata {
    public byte version = 0;
    
    public void Encode(ImageBurner.Encoder encoder) {
        encoder.EncodeByte(version);
        encoder.Close();
    }

    public void Decode(ImageBurner.Decoder decoder) {
        version = decoder.DecodeByte();
        decoder.Close();
    }
}