using System;
using System.IO;
using GameFramework;
using UnityEngine;

namespace UGFExtensions
{
    public static partial class BinaryReaderExtension
    {
        public static Color32 ReadColor32(this BinaryReader binaryReader)
        {
            return new Color32(binaryReader.ReadByte(), binaryReader.ReadByte(), binaryReader.ReadByte(),
                binaryReader.ReadByte());
        }

        public static Color ReadColor(this BinaryReader binaryReader)
        {
            return new Color(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }

        public static DateTime ReadDateTime(this BinaryReader binaryReader)
        {
            return new DateTime(binaryReader.ReadInt64());
        }

        public static Quaternion ReadQuaternion(this BinaryReader binaryReader)
        {
            return new Quaternion(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }

        public static Rect ReadRect(this BinaryReader binaryReader)
        {
            return new Rect(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }

        public static Vector2 ReadVector2(this BinaryReader binaryReader)
        {
            return new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader binaryReader)
        {
            return new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader binaryReader)
        {
            return new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                binaryReader.ReadSingle());
        }
        /// <summary>
        /// 从二进制流读取编码后的 32 位有符号整数。
        /// </summary>
        /// <param name="binaryReader">要读取的二进制流。</param>
        /// <param name="length">读取的位数</param>
        /// <returns>读取的 32 位有符号整数。</returns>
        public static int Read7BitEncodedInt32(this BinaryReader binaryReader,out int length)
        {
            length = 0;
            int value = 0;
            int shift = 0;
            byte b;
            do
            {
                if (shift >= 35)
                {
                    throw new GameFrameworkException("7 bit encoded int is invalid.");
                }

                b = binaryReader.ReadByte();
                value |= (b & 0x7f) << shift;
                shift += 7;
                length++;
            } while ((b & 0x80) != 0);

            return value;
        }
    }
}