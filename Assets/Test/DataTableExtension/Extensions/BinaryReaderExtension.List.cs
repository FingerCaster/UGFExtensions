using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace UGFExtensions
{
	public static partial class BinaryReaderExtension
	{
		public static List<bool> ReadBooleanList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<bool> list = new List<bool>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadBoolean());
			}
			return list;
		}
		public static List<byte> ReadByteList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<byte> list = new List<byte>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadByte());
			}
			return list;
		}
		public static List<char> ReadCharList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<char> list = new List<char>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadChar());
			}
			return list;
		}
		public static List<Color32> ReadColor32List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Color32> list = new List<Color32>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadColor32(binaryReader));
			}
			return list;
		}
		public static List<Color> ReadColorList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Color> list = new List<Color>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadColor(binaryReader));
			}
			return list;
		}
		public static List<DateTime> ReadDateTimeList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<DateTime> list = new List<DateTime>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadDateTime(binaryReader));
			}
			return list;
		}
		public static List<decimal> ReadDecimalList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<decimal> list = new List<decimal>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadDecimal());
			}
			return list;
		}
		public static List<double> ReadDoubleList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<double> list = new List<double>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadDouble());
			}
			return list;
		}
		public static List<float> ReadSingleList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<float> list = new List<float>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadSingle());
			}
			return list;
		}
		public static List<int> ReadInt32List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<int> list = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.Read7BitEncodedInt32());
			}
			return list;
		}
		public static List<long> ReadInt64List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<long> list = new List<long>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.Read7BitEncodedInt64());
			}
			return list;
		}
		public static List<Quaternion> ReadQuaternionList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Quaternion> list = new List<Quaternion>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadQuaternion(binaryReader));
			}
			return list;
		}
		public static List<Rect> ReadRectList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Rect> list = new List<Rect>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadRect(binaryReader));
			}
			return list;
		}
		public static List<sbyte> ReadSByteList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<sbyte> list = new List<sbyte>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadSByte());
			}
			return list;
		}
		public static List<short> ReadInt16List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<short> list = new List<short>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadInt16());
			}
			return list;
		}
		public static List<string> ReadStringList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<string> list = new List<string>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadString());
			}
			return list;
		}
		public static List<uint> ReadUInt32List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<uint> list = new List<uint>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.Read7BitEncodedUInt32());
			}
			return list;
		}
		public static List<ulong> ReadUInt64List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<ulong> list = new List<ulong>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.Read7BitEncodedUInt64());
			}
			return list;
		}
		public static List<ushort> ReadUInt16List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<ushort> list = new List<ushort>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(binaryReader.ReadUInt16());
			}
			return list;
		}
		public static List<Vector2> ReadVector2List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Vector2> list = new List<Vector2>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadVector2(binaryReader));
			}
			return list;
		}
		public static List<Vector3> ReadVector3List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Vector3> list = new List<Vector3>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadVector3(binaryReader));
			}
			return list;
		}
		public static List<Vector4> ReadVector4List(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<Vector4> list = new List<Vector4>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ReadVector4(binaryReader));
			}
			return list;
		}
		public static List<UGFExtensions.Test.TestEnum> ReadUGFExtensionsTestTestEnumList(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			List<UGFExtensions.Test.TestEnum> list = new List<UGFExtensions.Test.TestEnum>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add((UGFExtensions.Test.TestEnum)binaryReader.Read7BitEncodedInt32());
			}
			return list;
		}
	}
}
