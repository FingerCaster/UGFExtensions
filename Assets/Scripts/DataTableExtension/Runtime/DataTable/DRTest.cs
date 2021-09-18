//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2021-09-18 15:22:29.473
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;


namespace UGFExtensions
{
    /// <summary>
    /// 测试表格生成。
    /// </summary>
    public class DRTest : DataRowBase
    {
        private int m_Id = 0;

        /// <summary>
        /// 获取编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// 获取Bool值。
        /// </summary>
        public bool BoolValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Byte值。
        /// </summary>
        public byte ByteValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Char值。
        /// </summary>
        public char CharValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Color32值。
        /// </summary>
        public Color32 Color32Value
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Color值。
        /// </summary>
        public Color ColorValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取DateTime值。
        /// </summary>
        public DateTime DateTimeValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Decimal值。
        /// </summary>
        public decimal DecimalValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Double值。
        /// </summary>
        public double DoubleValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Float值。
        /// </summary>
        public float FloatValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Int值。
        /// </summary>
        public int IntValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Long值。
        /// </summary>
        public long LongValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Quaternion值。
        /// </summary>
        public Quaternion QuaternionValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Rect值。
        /// </summary>
        public Rect RectValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SByte值。
        /// </summary>
        public sbyte SByteValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Short值。
        /// </summary>
        public short ShortValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取String值。
        /// </summary>
        public string StringValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UInt值。
        /// </summary>
        public uint UIntValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ULong值。
        /// </summary>
        public ulong ULongValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UShort值。
        /// </summary>
        public ushort UShortValue
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector2值。
        /// </summary>
        public Vector2 Vector2Value
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector3值。
        /// </summary>
        public Vector3 Vector3Value
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector4值。
        /// </summary>
        public Vector4 Vector4Value
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Bool列表。
        /// </summary>
        public List<bool> BoolList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Byte列表。
        /// </summary>
        public List<byte> ByteList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Char列表。
        /// </summary>
        public List<char> CharList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Color32列表。
        /// </summary>
        public List<Color32> Color32List
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Color列表。
        /// </summary>
        public List<Color> ColorList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取DateTime列表。
        /// </summary>
        public List<DateTime> DateTimeList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Decimal列表。
        /// </summary>
        public List<decimal> DecimalList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Double列表。
        /// </summary>
        public List<double> DoubleList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Float列表。
        /// </summary>
        public List<float> FloatList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Int列表。
        /// </summary>
        public List<int> IntList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Long列表。
        /// </summary>
        public List<long> LongList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Quaternion列表。
        /// </summary>
        public List<Quaternion> QuaternionList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Rect列表。
        /// </summary>
        public List<Rect> RectList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SByte列表。
        /// </summary>
        public List<sbyte> SByteList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Short列表。
        /// </summary>
        public List<short> ShortList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取String列表。
        /// </summary>
        public List<string> StringList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UInt列表。
        /// </summary>
        public List<uint> UIntList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ULong列表。
        /// </summary>
        public List<ulong> ULongList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UShort列表。
        /// </summary>
        public List<ushort> UShortList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector2列表。
        /// </summary>
        public List<Vector2> Vector2List
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector3列表。
        /// </summary>
        public List<Vector3> Vector3List
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector4列表。
        /// </summary>
        public List<Vector4> Vector4List
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Bool数组。
        /// </summary>
        public bool[] BoolArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Byte数组。
        /// </summary>
        public byte[] ByteArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Char数组。
        /// </summary>
        public char[] CharArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Color32数组。
        /// </summary>
        public Color32[] Color32Array
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Color数组。
        /// </summary>
        public Color[] ColorArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取DateTime数组。
        /// </summary>
        public DateTime[] DateTimeArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Decimal数组。
        /// </summary>
        public decimal[] DecimalArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Double数组。
        /// </summary>
        public double[] DoubleArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Float数组。
        /// </summary>
        public float[] FloatArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Int数组。
        /// </summary>
        public int[] IntArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Long数组。
        /// </summary>
        public long[] LongArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Quaternion数组。
        /// </summary>
        public Quaternion[] QuaternionArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Rect数组。
        /// </summary>
        public Rect[] RectArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取SByte数组。
        /// </summary>
        public sbyte[] SByteArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Short数组。
        /// </summary>
        public short[] ShortArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取String数组。
        /// </summary>
        public string[] StringArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UInt数组。
        /// </summary>
        public uint[] UIntArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取ULong数组。
        /// </summary>
        public ulong[] ULongArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取UShort数组。
        /// </summary>
        public ushort[] UShortArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector2数组。
        /// </summary>
        public Vector2[] Vector2Array
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector3数组。
        /// </summary>
        public Vector3[] Vector3Array
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取Vector4数组。
        /// </summary>
        public Vector4[] Vector4Array
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);
            for (int i = 0; i < columnStrings.Length; i++)
            {
                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);
            }

            int index = 0;
            index++;
            m_Id = int.Parse(columnStrings[index++]);
            index++;
			BoolValue = bool.Parse(columnStrings[index++]);
			ByteValue = byte.Parse(columnStrings[index++]);
			CharValue = char.Parse(columnStrings[index++]);
			Color32Value = DataTableExtension.ParseColor32(columnStrings[index++]);
			ColorValue = DataTableExtension.ParseColor(columnStrings[index++]);
            index++;
			DateTimeValue = DateTime.Parse(columnStrings[index++]);
			DecimalValue = decimal.Parse(columnStrings[index++]);
			DoubleValue = double.Parse(columnStrings[index++]);
			FloatValue = float.Parse(columnStrings[index++]);
			IntValue = int.Parse(columnStrings[index++]);
			LongValue = long.Parse(columnStrings[index++]);
			QuaternionValue = DataTableExtension.ParseQuaternion(columnStrings[index++]);
			RectValue = DataTableExtension.ParseRect(columnStrings[index++]);
			SByteValue = sbyte.Parse(columnStrings[index++]);
			ShortValue = short.Parse(columnStrings[index++]);
			StringValue = columnStrings[index++];
			UIntValue = uint.Parse(columnStrings[index++]);
			ULongValue = ulong.Parse(columnStrings[index++]);
			UShortValue = ushort.Parse(columnStrings[index++]);
			Vector2Value = DataTableExtension.ParseVector2(columnStrings[index++]);
			Vector3Value = DataTableExtension.ParseVector3(columnStrings[index++]);
			Vector4Value = DataTableExtension.ParseVector4(columnStrings[index++]);
			BoolList = DataTableExtension.ParseBooleanList(columnStrings[index++]);
			ByteList = DataTableExtension.ParseByteList(columnStrings[index++]);
			CharList = DataTableExtension.ParseCharList(columnStrings[index++]);
			Color32List = DataTableExtension.ParseColor32List(columnStrings[index++]);
			ColorList = DataTableExtension.ParseColorList(columnStrings[index++]);
			DateTimeList = DataTableExtension.ParseDateTimeList(columnStrings[index++]);
			DecimalList = DataTableExtension.ParseDecimalList(columnStrings[index++]);
			DoubleList = DataTableExtension.ParseDoubleList(columnStrings[index++]);
			FloatList = DataTableExtension.ParseSingleList(columnStrings[index++]);
			IntList = DataTableExtension.ParseInt32List(columnStrings[index++]);
			LongList = DataTableExtension.ParseInt64List(columnStrings[index++]);
			QuaternionList = DataTableExtension.ParseQuaternionList(columnStrings[index++]);
			RectList = DataTableExtension.ParseRectList(columnStrings[index++]);
			SByteList = DataTableExtension.ParseSByteList(columnStrings[index++]);
			ShortList = DataTableExtension.ParseInt16List(columnStrings[index++]);
			StringList = DataTableExtension.ParseStringList(columnStrings[index++]);
			UIntList = DataTableExtension.ParseUInt32List(columnStrings[index++]);
			ULongList = DataTableExtension.ParseUInt64List(columnStrings[index++]);
			UShortList = DataTableExtension.ParseUInt16List(columnStrings[index++]);
			Vector2List = DataTableExtension.ParseVector2List(columnStrings[index++]);
			Vector3List = DataTableExtension.ParseVector3List(columnStrings[index++]);
			Vector4List = DataTableExtension.ParseVector4List(columnStrings[index++]);
			BoolArray = DataTableExtension.ParseBooleanArray(columnStrings[index++]);
			ByteArray = DataTableExtension.ParseByteArray(columnStrings[index++]);
			CharArray = DataTableExtension.ParseCharArray(columnStrings[index++]);
			Color32Array = DataTableExtension.ParseColor32Array(columnStrings[index++]);
			ColorArray = DataTableExtension.ParseColorArray(columnStrings[index++]);
			DateTimeArray = DataTableExtension.ParseDateTimeArray(columnStrings[index++]);
			DecimalArray = DataTableExtension.ParseDecimalArray(columnStrings[index++]);
			DoubleArray = DataTableExtension.ParseDoubleArray(columnStrings[index++]);
			FloatArray = DataTableExtension.ParseSingleArray(columnStrings[index++]);
			IntArray = DataTableExtension.ParseInt32Array(columnStrings[index++]);
			LongArray = DataTableExtension.ParseInt64Array(columnStrings[index++]);
			QuaternionArray = DataTableExtension.ParseQuaternionArray(columnStrings[index++]);
			RectArray = DataTableExtension.ParseRectArray(columnStrings[index++]);
			SByteArray = DataTableExtension.ParseSByteArray(columnStrings[index++]);
			ShortArray = DataTableExtension.ParseInt16Array(columnStrings[index++]);
			StringArray = DataTableExtension.ParseStringArray(columnStrings[index++]);
			UIntArray = DataTableExtension.ParseUInt32Array(columnStrings[index++]);
			ULongArray = DataTableExtension.ParseUInt64Array(columnStrings[index++]);
			UShortArray = DataTableExtension.ParseUInt16Array(columnStrings[index++]);
			Vector2Array = DataTableExtension.ParseVector2Array(columnStrings[index++]);
			Vector3Array = DataTableExtension.ParseVector3Array(columnStrings[index++]);
			Vector4Array = DataTableExtension.ParseVector4Array(columnStrings[index++]);
            GeneratePropertyArray();
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    m_Id = binaryReader.Read7BitEncodedInt32();
                    BoolValue = binaryReader.ReadBoolean();
                    ByteValue = binaryReader.ReadByte();
                    CharValue = binaryReader.ReadChar();
                    Color32Value = binaryReader.ReadColor32();
                    ColorValue = binaryReader.ReadColor();
                    DateTimeValue = binaryReader.ReadDateTime();
                    DecimalValue = binaryReader.ReadDecimal();
                    DoubleValue = binaryReader.ReadDouble();
                    FloatValue = binaryReader.ReadSingle();
                    IntValue = binaryReader.Read7BitEncodedInt32();
                    LongValue = binaryReader.Read7BitEncodedInt64();
                    QuaternionValue = binaryReader.ReadQuaternion();
                    RectValue = binaryReader.ReadRect();
                    SByteValue = binaryReader.ReadSByte();
                    ShortValue = binaryReader.ReadInt16();
                    StringValue = binaryReader.ReadString();
                    UIntValue = binaryReader.Read7BitEncodedUInt32();
                    ULongValue = binaryReader.Read7BitEncodedUInt64();
                    UShortValue = binaryReader.ReadUInt16();
                    Vector2Value = binaryReader.ReadVector2();
                    Vector3Value = binaryReader.ReadVector3();
                    Vector4Value = binaryReader.ReadVector4();
					BoolList = binaryReader.ReadBooleanList();
					ByteList = binaryReader.ReadByteList();
					CharList = binaryReader.ReadCharList();
					Color32List = binaryReader.ReadColor32List();
					ColorList = binaryReader.ReadColorList();
					DateTimeList = binaryReader.ReadDateTimeList();
					DecimalList = binaryReader.ReadDecimalList();
					DoubleList = binaryReader.ReadDoubleList();
					FloatList = binaryReader.ReadSingleList();
					IntList = binaryReader.ReadInt32List();
					LongList = binaryReader.ReadInt64List();
					QuaternionList = binaryReader.ReadQuaternionList();
					RectList = binaryReader.ReadRectList();
					SByteList = binaryReader.ReadSByteList();
					ShortList = binaryReader.ReadInt16List();
					StringList = binaryReader.ReadStringList();
					UIntList = binaryReader.ReadUInt32List();
					ULongList = binaryReader.ReadUInt64List();
					UShortList = binaryReader.ReadUInt16List();
					Vector2List = binaryReader.ReadVector2List();
					Vector3List = binaryReader.ReadVector3List();
					Vector4List = binaryReader.ReadVector4List();
					BoolArray = binaryReader.ReadBooleanArray();
					ByteArray = binaryReader.ReadByteArray();
					CharArray = binaryReader.ReadCharArray();
					Color32Array = binaryReader.ReadColor32Array();
					ColorArray = binaryReader.ReadColorArray();
					DateTimeArray = binaryReader.ReadDateTimeArray();
					DecimalArray = binaryReader.ReadDecimalArray();
					DoubleArray = binaryReader.ReadDoubleArray();
					FloatArray = binaryReader.ReadSingleArray();
					IntArray = binaryReader.ReadInt32Array();
					LongArray = binaryReader.ReadInt64Array();
					QuaternionArray = binaryReader.ReadQuaternionArray();
					RectArray = binaryReader.ReadRectArray();
					SByteArray = binaryReader.ReadSByteArray();
					ShortArray = binaryReader.ReadInt16Array();
					StringArray = binaryReader.ReadStringArray();
					UIntArray = binaryReader.ReadUInt32Array();
					ULongArray = binaryReader.ReadUInt64Array();
					UShortArray = binaryReader.ReadUInt16Array();
					Vector2Array = binaryReader.ReadVector2Array();
					Vector3Array = binaryReader.ReadVector3Array();
					Vector4Array = binaryReader.ReadVector4Array();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private void GeneratePropertyArray()
        {

        }
    }
}
