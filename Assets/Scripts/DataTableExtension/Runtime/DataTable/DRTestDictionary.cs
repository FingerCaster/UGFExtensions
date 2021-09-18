//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2021-09-18 15:22:29.485
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
    public class DRTestDictionary : DataRowBase
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
        /// 获取测试字典(KeyType：int ValueType:Int)。
        /// </summary>
        public Dictionary<int,int> TestIntIntDictionary
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试字典(KeyType：int ValueType:vector3)。
        /// </summary>
        public Dictionary<int,Vector3> TestIntVector3Dictionary
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
			TestIntIntDictionary = DataTableExtension.ParseInt32Int32Dictionary(columnStrings[index++]);
			TestIntVector3Dictionary = DataTableExtension.ParseInt32Vector3Dictionary(columnStrings[index++]);
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
					TestIntIntDictionary = binaryReader.ReadInt32Int32Dictionary();
					TestIntVector3Dictionary = binaryReader.ReadInt32Vector3Dictionary();
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
