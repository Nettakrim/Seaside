using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageBurner {
    public static void Encode(Texture2D tex, byte[] data) {
        int limit = ((tex.width*tex.height)/2)-5;
        if (data.Length > limit) {
            throw new Exception("Cant encode more than "+limit+" bytes in a "+tex.width+"x"+tex.height+" image ("+data.Length+" bytes attempted)");
        }

        ImageEncoder encoder = new ImageEncoder(tex, 0);
        encoder.EncodeByte(0); //reserved byte for any flags like version
        encoder.EncodeBytes(BitConverter.GetBytes(data.Length));
        encoder.EncodeBytes(data);
    }

    public static byte[] Decode(Texture2D tex) {
        ImageDecoder decoder = new ImageDecoder(tex, 0);
        byte flags = decoder.DecodeByte();
        if (flags != 0) {
            throw new Exception("Image has invalid flags ("+flags+")");
        }

        int length = BitConverter.ToInt32(decoder.DecodeBytes(4));
        return decoder.DecodeBytes(length);
    }

    public class ImageEncoder {
        public ImageEncoder(Texture2D tex, int startPosition) {
            this.tex = tex;
            position = startPosition;
        }

        public Texture2D tex;
        public int position = 0;

        public void EncodeBytes(byte[] bytes) {
            foreach (byte b in bytes) {
                EncodeByte(b);
            }
        }

        public void EncodeByte(byte b) {
            EncodeByte(b, position);
        }

        public void EncodeByte(byte b, int pos) {
            EncodeNibble((byte)(b >> 4), pos*2);
            EncodeNibble((byte)(b & 15), pos*2 + 1);
            position = pos+1;
        }

        private void EncodeNibble(byte nibble, int pixelPos) {
            Color color = tex.GetPixel(pixelPos%tex.width, pixelPos/tex.width);
            color.r = BurnValue(color.r, nibble, 1, 0);
            color.g = BurnValue(color.g, nibble, 1, 1);
            color.b = BurnValue(color.b, nibble, 2, 2); 
            tex.SetPixel(pixelPos%tex.width, pixelPos/tex.width, color);
        }

        private float BurnValue(float f, int burnValue, int burnAmount, int shift) {
            int i = (int)(f*255f);
            int rounding = 1 << burnAmount;
            i = (i/rounding)*rounding;
            i += (burnValue&((rounding-1)<<shift))>>shift;
            return i/255f;
        }
    }

    public class ImageDecoder {
        public ImageDecoder(Texture2D tex, int startPosition) {
            this.tex = tex;
            position = startPosition;
        }

        public Texture2D tex;
        public int position = 0;

        public byte[] DecodeBytes(int length) {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++) {
                bytes[i] = DecodeByte();
            }
            return bytes;
        }

        public byte DecodeByte() {
            return DecodeByte(position);
        }

        public byte DecodeByte(int pos) {
            byte b = (byte)((DecodeNibble(pos*2) << 4) | DecodeNibble(pos*2 + 1));
            position = pos+1;
            return b;
        }

        private byte DecodeNibble(int pixelPos) {
            Color color = tex.GetPixel(pixelPos%tex.width, pixelPos/tex.width);
            byte b = 0;
            b |= UnburnValue(color.r, 1, 0);
            b |= UnburnValue(color.g, 1, 1);
            b |= UnburnValue(color.b, 2, 2);
            return b;
        }

        private byte UnburnValue(float f, int burnAmount, int shift) {
            int i = (int)(f*255f);
            return (byte)((i%(1<<burnAmount)) << shift);
        }
    }
}