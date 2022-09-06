using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace UGFExtensions
{
	public static partial class BinaryReaderExtension
	{
		public static bool[] ReadBooleanArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			bool[] array = new bool[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadBoolean();
			}
			return array;
		}
		public static byte[] ReadByteArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			byte[] array = new byte[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadByte();
			}
			return array;
		}
		public static char[] ReadCharArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			char[] array = new char[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadChar();
			}
			return array;
		}
		public static Color32[] ReadColor32Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Color32[] array = new Color32[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadColor32(binaryReader);
			}
			return array;
		}
		public static Color[] ReadColorArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Color[] array = new Color[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadColor(binaryReader);
			}
			return array;
		}
		public static DateTime[] ReadDateTimeArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			DateTime[] array = new DateTime[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadDateTime(binaryReader);
			}
			return array;
		}
		public static decimal[] ReadDecimalArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			decimal[] array = new decimal[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadDecimal();
			}
			return array;
		}
		public static double[] ReadDoubleArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			double[] array = new double[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadDouble();
			}
			return array;
		}
		public static float[] ReadSingleArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			float[] array = new float[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadSingle();
			}
			return array;
		}
		public static int[] ReadInt32Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			int[] array = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.Read7BitEncodedInt32();
			}
			return array;
		}
		public static long[] ReadInt64Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			long[] array = new long[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.Read7BitEncodedInt64();
			}
			return array;
		}
		public static Quaternion[] ReadQuaternionArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Quaternion[] array = new Quaternion[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadQuaternion(binaryReader);
			}
			return array;
		}
		public static Rect[] ReadRectArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Rect[] array = new Rect[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadRect(binaryReader);
			}
			return array;
		}
		public static sbyte[] ReadSByteArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			sbyte[] array = new sbyte[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadSByte();
			}
			return array;
		}
		public static short[] ReadInt16Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			short[] array = new short[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadInt16();
			}
			return array;
		}
		public static string[] ReadStringArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadString();
			}
			return array;
		}
		public static uint[] ReadUInt32Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			uint[] array = new uint[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.Read7BitEncodedUInt32();
			}
			return array;
		}
		public static ulong[] ReadUInt64Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			ulong[] array = new ulong[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.Read7BitEncodedUInt64();
			}
			return array;
		}
		public static ushort[] ReadUInt16Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			ushort[] array = new ushort[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = binaryReader.ReadUInt16();
			}
			return array;
		}
		public static Vector2[] ReadVector2Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Vector2[] array = new Vector2[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadVector2(binaryReader);
			}
			return array;
		}
		public static Vector3[] ReadVector3Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Vector3[] array = new Vector3[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadVector3(binaryReader);
			}
			return array;
		}
		public static Vector4[] ReadVector4Array(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Vector4[] array = new Vector4[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadVector4(binaryReader);
			}
			return array;
		}
		public static UGFExtensions.Test.TestEnum[] ReadUGFExtensionsTestTestEnumArray(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			UGFExtensions.Test.TestEnum[] array = new UGFExtensions.Test.TestEnum[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = (UGFExtensions.Test.TestEnum)binaryReader.Read7BitEncodedInt32();
			}
			return array;
		}
	}
}
