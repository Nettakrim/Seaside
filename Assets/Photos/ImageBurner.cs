using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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



    public static class CoordinateFolding {
        //attempts to give each point in an image a single unique other point in the image, but one that is hard to predict
        //this means the burnt pixels are evenly distributed across the image as noise, instead of being a line at the bottom
        //this doesnt look as shuffled for different resolutions, and might not even work properly for non-square/non-power of 2
        public static int foldingSteps = 16;

        public static (int,int) UnfoldedCoordinates(int pos, int width, int height) {
            return (pos%width, pos/width);
        }

        public static (int,int) MultiFoldCoordinates(int pos, int width, int height) {
            return MultiFoldCoordinates(pos, width, height, foldingSteps);
        }

        public static (int,int) MultiFoldCoordinates(int pos, int width, int height, int count) {
            if (foldingSteps == 0) {
                return UnfoldedCoordinates(pos, width, height);
            }
            for (int i = 0; i < count; i++) {
                pos = FoldIndex(FoldCoordinates(pos, width, height), width, height);
            }
            return FoldCoordinates(pos, width, height);
        }

        public static (int,int) FoldCoordinates(int pos, int width, int height) {
            int x = (pos-(pos/height))%width;
            int y = pos%height;
            if (x <= width/2) x = (width/2)-x;
            return (x, y);
        }

        public static int FoldIndex((int,int) c, int width, int height) {
            return (c.Item1+(c.Item2*width))^53;
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
            (int,int) c = CoordinateFolding.MultiFoldCoordinates(pixelPos, tex.width, tex.height);
            Color color = tex.GetPixel(c.Item1, c.Item2);
            color.r = BurnValue(color.r, nibble, 1, 0);
            color.g = BurnValue(color.g, nibble, 1, 1);
            color.b = BurnValue(color.b, nibble, 2, 2); 
            tex.SetPixel(c.Item1, c.Item2, color);
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
                Debug.LogWarning("Image has invalid flags so its probably just a regular image (found "+flags+", should be "+correctFlags+")");
                Close();
                return;
            }

            limit += DataTypes.DecodeInt32(this);

            int maxLimit = (tex.width*tex.height)/2;
            if (limit > maxLimit || limit < 0) {
                Debug.LogWarning("Stored length of "+limit+" is larger than maximum amount "+maxLimit+" that could theoretically be in the image");
                Close();
                return;
            }

            isValid = true;
        }

        public void Close() {
            //force error if you try to decode after finishing
            limit = -1;
            isValid = false;
        }

        public static explicit operator Decoder(Texture2D tex) => new(tex);

        protected Texture2D tex;
        protected int position;
        protected int limit;
        protected bool isValid;

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
            (int,int) c = CoordinateFolding.MultiFoldCoordinates(pixelPos, tex.width, tex.height);
            Color color = tex.GetPixel(c.Item1, c.Item2);
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

        public bool IsValid() {
            return isValid;
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


        public static void EncodeUInt16(Encoder encoder, ushort value) {
            encoder.EncodeBytes(BitConverter.GetBytes(value));
        }

        public static ushort DecodeUInt16(Decoder decoder) {
            return BitConverter.ToUInt16(decoder.DecodeBytes(2));
        }


        public static void EncodeByteArrayListStart(Encoder encoder, int length, int size) {
            EncodeUInt16(encoder, (ushort)length);
            EncodeUInt16(encoder, (ushort)size);
        }

        public static void EncodeByteArrayList(Encoder encoder, List<byte[]> bytes) {
            EncodeByteArrayListStart(encoder, bytes.Count, bytes[0].Length);
            foreach (byte[] b in bytes) {
                encoder.EncodeBytes(b);
            }
        }


        public static (int,int) DecodeByteArrayListStart(Decoder decoder) {
            ushort d0 = DecodeUInt16(decoder);
            ushort d1 = DecodeUInt16(decoder);
            return (d0, d1);       
        }

        public static List<byte[]> DecodeByteArrayList(Decoder decoder) {
            (int d0, int d1) = DecodeByteArrayListStart(decoder);
            List<byte[]> bytesList = new List<byte[]>(d0);
            for (int i = 0; i < d0; i++) {
                bytesList.Add(decoder.DecodeBytes(d1));
            }

            return bytesList;
        }


        public static void EncodeChar(Encoder encoder, char value) {
            encoder.EncodeBytes(BitConverter.GetBytes(value));
        }


        public static char DecodeChar(Decoder decoder) {
            return BitConverter.ToChar(decoder.DecodeBytes(2));
        }


        public static void EncodeFixedLengthString(Encoder encoder, string value, int length) {
            if (value.Length > length) {
                value = value.Substring(0, length);
            } else if (value.Length < length) {
                value.PadRight(length, (char)0);
            }

            foreach (char c in value) {
                EncodeChar(encoder, c);
            }
        }

        public static string DecodeFixedLengthString(Decoder decoder, int length) {
            string s = "";
            for (int i = 0; i < length; i++) {
                char c = DecodeChar(decoder);
                if (c == (char)0) {
                    return s;
                }
                s += c;
            }
            return s;
        }


        public static void EncodeVariableLengthString(Encoder encoder, string value) {
            EncodeUInt16(encoder, (ushort)value.Length);
            EncodeFixedLengthString(encoder, value, value.Length);
        }

        public static string DecodeVariableLengthString(Decoder decoder) {
            int length = DecodeUInt16(decoder);
            return DecodeFixedLengthString(decoder, length);
        }
    }
}