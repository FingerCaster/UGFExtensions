using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace UGFExtensions
{
	public static partial class DataTableExtension
	{
		public static bool[] ParseBooleanArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			bool[] array = new bool[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Boolean.Parse(splitValue[i]);
			}

			return array;
		}
		public static byte[] ParseByteArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			byte[] array = new byte[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Byte.Parse(splitValue[i]);
			}

			return array;
		}
		public static char[] ParseCharArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			char[] array = new char[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Char.Parse(splitValue[i]);
			}

			return array;
		}
		public static Color32[] ParseColor32Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Color32[] array = new Color32[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseColor32(splitValue[i]);
			}

			return array;
		}
		public static Color[] ParseColorArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Color[] array = new Color[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseColor(splitValue[i]);
			}

			return array;
		}
		public static DateTime[] ParseDateTimeArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			DateTime[] array = new DateTime[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = DateTime.Parse(splitValue[i]);
			}

			return array;
		}
		public static decimal[] ParseDecimalArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			decimal[] array = new decimal[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Decimal.Parse(splitValue[i]);
			}

			return array;
		}
		public static double[] ParseDoubleArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			double[] array = new double[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Double.Parse(splitValue[i]);
			}

			return array;
		}
		public static float[] ParseSingleArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			float[] array = new float[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Single.Parse(splitValue[i]);
			}

			return array;
		}
		public static int[] ParseInt32Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			int[] array = new int[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Int32.Parse(splitValue[i]);
			}

			return array;
		}
		public static long[] ParseInt64Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			long[] array = new long[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Int64.Parse(splitValue[i]);
			}

			return array;
		}
		public static Quaternion[] ParseQuaternionArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Quaternion[] array = new Quaternion[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseQuaternion(splitValue[i]);
			}

			return array;
		}
		public static Rect[] ParseRectArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Rect[] array = new Rect[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseRect(splitValue[i]);
			}

			return array;
		}
		public static sbyte[] ParseSByteArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			sbyte[] array = new sbyte[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = SByte.Parse(splitValue[i]);
			}

			return array;
		}
		public static short[] ParseInt16Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			short[] array = new short[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = Int16.Parse(splitValue[i]);
			}

			return array;
		}
		public static string[] ParseStringArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			string[] array = new string[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = splitValue[i];
			}

			return array;
		}
		public static uint[] ParseUInt32Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			uint[] array = new uint[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = UInt32.Parse(splitValue[i]);
			}

			return array;
		}
		public static ulong[] ParseUInt64Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			ulong[] array = new ulong[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = UInt64.Parse(splitValue[i]);
			}

			return array;
		}
		public static ushort[] ParseUInt16Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			ushort[] array = new ushort[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = UInt16.Parse(splitValue[i]);
			}

			return array;
		}
		public static Vector2[] ParseVector2Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Vector2[] array = new Vector2[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseVector2(splitValue[i]);
			}

			return array;
		}
		public static Vector3[] ParseVector3Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Vector3[] array = new Vector3[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseVector3(splitValue[i]);
			}

			return array;
		}
		public static Vector4[] ParseVector4Array(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split('|');
			Vector4[] array = new Vector4[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				array[i] = ParseVector4(splitValue[i]);
			}

			return array;
		}
		public static Test.TestEnum[] ParseTestTestEnumArray(string value)
		{
			if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
				return null;
			string[] splitValue = value.Split(',');
			Test.TestEnum[] array = new Test.TestEnum[splitValue.Length];
			for (int i = 0; i < splitValue.Length; i++)
			{
				bool isInt = int.TryParse(splitValue[i], out int v);
				if (isInt)
				{
					array[i] = (Test.TestEnum)v;
					continue;
				}
				bool isString = EnumParse(splitValue[i], out Test.TestEnum v1);
				if (isString)
				{
					array[i] = v1;
				}
			}

			return array;
		}
	}
}
