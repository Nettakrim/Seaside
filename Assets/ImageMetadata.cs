public class ImageMetadata {
    public byte version = 0;
    
    public byte[] ToBytes() {
        byte[] bytes = new byte[1];

        bytes[0] = version;    

        return bytes;
    }

    public static ImageMetadata FromBytes(byte[] bytes) {
        ImageMetadata imageMetadata = new ImageMetadata();

        imageMetadata.version = bytes[0];

        return imageMetadata;
    }
}