using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageBurner {
    public static class HeaderInfo {
        public static int size = 8;

        public static int GetFlagInt(int width, int height) {
            //first byte of the flag int is all 0, meaning its this default checksum-style flag, which is some arbitrary hash to do with the width and height of the image
            //if the essentially reserved 4 bytes at the start ever needed to be used, then the first byte should be something other than 0 to signify that a different format is being used
            return (((((width+1)<<4)^(height-1)<<1)%16777213)^58913)&0xFFFFFF;
        }
    }



    public class Encoder {
        public Encoder(Texture2D texture) {
            tex = texture;
            position = HeaderInfo.size;
            limit = (tex.width*tex.height)/2;
        }

        public void Close() {
            int length = position-HeaderInfo.size;
            position = 0;
            DataTypes.EncodeInt32(this, HeaderInfo.GetFlagInt(tex.width, tex.height));
            DataTypes.EncodeInt32(this, length);
            //force error if you try to encode after finishing
            limit = -1;
        }

        public static explicit operator Encoder(Texture2D tex) => new(tex);

        protected Texture2D tex;
        protected int position;
        protected int limit;

        public void EncodeBytes(byte[] bytes) {
            foreach (byte b in bytes) {
                EncodeByte(b);
            }
        }

        public void EncodeByte(byte b) {
            EncodeByte(b, position);
        }

        protected void EncodeByte(byte b, int pos) {
            if (pos > limit) {
                throw new IndexOutOfRangeException("Byte index "+pos+" out of bounds for limit "+limit+", texture is "+tex.width+"x"+tex.height);
            }
            EncodeNibble((byte)(b >> 4), pos*2);
            EncodeNibble((byte)(b & 15), pos*2 + 1);
            position = pos+1;
        }

        protected void EncodeNibble(byte nibble, int pixelPos) {
            Color color = tex.GetPixel(pixelPos%tex.width, pixelPos/tex.width);
            color.r = BurnValue(color.r, nibble, 1, 0);
            color.g = BurnValue(color.g, nibble, 1, 1);
            color.b = BurnValue(color.b, nibble, 2, 2); 
            tex.SetPixel(pixelPos%tex.width, pixelPos/tex.width, color);
        }

        protected float BurnValue(float f, int burnValue, int burnAmount, int shift) {
            int i = (int)(f*255f);
            int rounding = 1 << burnAmount;
            i = (i/rounding)*rounding;
            i += (burnValue&((rounding-1)<<shift))>>shift;
            return i/255f;
        }

        public int GetRemainingBytes() {
            return Mathf.Max(limit-position, 0);
        }
    }



    public class Decoder {
        public Decoder(Texture2D texture) {
            tex = texture;
            position = 0;
            limit = HeaderInfo.size;

            int flags = DataTypes.DecodeInt32(this);
            int correctFlags = HeaderInfo.GetFlagInt(tex.width, tex.height);
            if (flags != correctFlags) {
                throw new Exception("Image has invalid flags so its probably just a regular image (found "+flags+", should be "+correctFlags+")");
            }

            limit = DataTypes.DecodeInt32(this)+HeaderInfo.size;
        }

        public void Close() {
            //force error if you try to decode after finishing
            limit = -1;   
        }

        public static explicit operator Decoder(Texture2D tex) => new(tex);

        protected Texture2D tex;
        protected int position;
        protected int limit;

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

        protected byte DecodeByte(int pos) {
            if (pos > limit) {
                Debug.LogWarning("Byte index "+pos+" out of bounds for limit "+limit+", returning 0");
                return 0;
            }
            byte b = (byte)((DecodeNibble(pos*2) << 4) | DecodeNibble(pos*2 + 1));
            position = pos+1;
            return b;
        }

        protected byte DecodeNibble(int pixelPos) {
            Color color = tex.GetPixel(pixelPos%tex.width, pixelPos/tex.width);
            byte b = 0;
            b |= GetBurntValue(color.r, 1, 0);
            b |= GetBurntValue(color.g, 1, 1);
            b |= GetBurntValue(color.b, 2, 2);
            return b;
        }

        protected byte GetBurntValue(float f, int burnAmount, int shift) {
            int i = (int)(f*255f);
            return (byte)((i%(1<<burnAmount)) << shift);
        }

        public int GetRemainingBytes() {
            return Mathf.Max(limit-position, 0);
        }
    }



    public static class DataTypes {
        public static void EncodeInt32(Encoder encoder, int value) {
            encoder.EncodeBytes(BitConverter.GetBytes(value));
        }

        public static int DecodeInt32(Decoder decoder) {
            return BitConverter.ToInt32(decoder.DecodeBytes(4));
        }

        
        public static void EncodeFloat(Encoder encoder, float value) {
            encoder.EncodeBytes(BitConverter.GetBytes(value));
        }

        public static float DecodeFloat(Decoder decoder) {
            return BitConverter.ToSingle(decoder.DecodeBytes(4));
        }


        public static void EncodeVector2(Encoder encoder, Vector2 v) {
            EncodeFloat(encoder, v.x);
            EncodeFloat(encoder, v.y);
        }

        public static Vector2 DecodeVector2(Decoder decoder) {
            return new Vector2(DecodeFloat(decoder), DecodeFloat(decoder));
        }


        public static void EncodeVector3(Encoder encoder, Vector3 v) {
            EncodeFloat(encoder, v.x);
            EncodeFloat(encoder, v.y);
            EncodeFloat(encoder, v.z);
        }

        public static Vector3 DecodeVector3(Decoder decoder) {
            return new Vector3(DecodeFloat(decoder), DecodeFloat(decoder), DecodeFloat(decoder));
        }
    }
}