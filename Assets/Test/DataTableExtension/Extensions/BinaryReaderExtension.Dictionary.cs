using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace UGFExtensions
{
	public static partial class BinaryReaderExtension
	{
		public static Dictionary<int,int> ReadInt32Int32Dictionary(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Dictionary<int,int> dictionary = new Dictionary<int,int>(count);
			for (int i = 0; i < count; i++)
			{
				dictionary.Add(binaryReader.Read7BitEncodedInt32(),binaryReader.Read7BitEncodedInt32());
			}
			return dictionary;
		}
		public static Dictionary<int,Vector3> ReadInt32Vector3Dictionary(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Dictionary<int,Vector3> dictionary = new Dictionary<int,Vector3>(count);
			for (int i = 0; i < count; i++)
			{
				dictionary.Add(binaryReader.Read7BitEncodedInt32(), ReadVector3(binaryReader));
			}
			return dictionary;
		}
		public static Dictionary<UGFExtensions.Test.TestEnum,int> ReadUGFExtensionsTestTestEnumInt32Dictionary(this BinaryReader binaryReader)
		{
			int count = binaryReader.Read7BitEncodedInt32();
			Dictionary<UGFExtensions.Test.TestEnum,int> dictionary = new Dictionary<UGFExtensions.Test.TestEnum,int>(count);
			for (int i = 0; i < count; i++)
			{
				dictionary.Add((UGFExtensions.Test.TestEnum) binaryReader.Read7BitEncodedInt32(),(int) binaryReader.Read7BitEncodedInt32());
			}
			return dictionary;
		}
	}
}
