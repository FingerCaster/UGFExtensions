//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2021-07-19 19:01:10.201
//------------------------------------------------------------

using GameFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityGameFramework.Runtime;


namespace DE
{
    /// <summary>
    /// 测试表格生成。
    /// </summary>
    public class DRTestEnum : DataRowBase
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
        /// 获取测试枚举。
        /// </summary>
        public Test.TestEnum TestEnum
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试枚举list。
        /// </summary>
        public List<Test.TestEnum> TestEnumList
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试枚举Array。
        /// </summary>
        public Test.TestEnum[] TestEnumArray
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试枚举字典。
        /// </summary>
        public Dictionary<Test.TestEnum,int> TestEnumDic
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
			TestEnum = (Test.TestEnum)int.Parse(columnStrings[index++]);
			TestEnumList = DataTableExtension.ParseTestTestEnumList(columnStrings[index++]);
			TestEnumArray = DataTableExtension.ParseTestTestEnumArray(columnStrings[index++]);
			TestEnumDic = DataTableExtension.ParseTestTestEnumInt32Dictionary(columnStrings[index++]);
            index++;
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
					TestEnum = (Test.TestEnum)binaryReader.Read7BitEncodedInt32();
					TestEnumList = binaryReader.ReadTestTestEnumList();
					TestEnumArray = binaryReader.ReadTestTestEnumArray();
					TestEnumDic = binaryReader.ReadTestTestEnumInt32Dictionary();
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
