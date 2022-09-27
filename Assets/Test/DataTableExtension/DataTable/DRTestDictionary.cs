//------------------------------------------------------------
// 此文件由工具自动生成，请勿直接修改。
// 生成时间：2022-09-27 10:36:31.472
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

        /// <summary>
        /// 获取测试整数1。
        /// </summary>
        public int TestInt1
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取测试整数2。
        /// </summary>
        public int TestInt2
        {
            get;
            private set;
        }

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columnStrings = dataRowString.Split(UGFExtensions.DataTableExtension.DataSplitSeparators);
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
			TestInt1 = int.Parse(columnStrings[index++]);
			TestInt2 = int.Parse(columnStrings[index++]);
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
                    TestInt1 = binaryReader.Read7BitEncodedInt32();
                    TestInt2 = binaryReader.Read7BitEncodedInt32();
                }
            }

            GeneratePropertyArray();
            return true;
        }

        private KeyValuePair<int, int>[] m_TestInt = null;

        public int TestIntCount
        {
            get
            {
                return m_TestInt.Length;
            }
        }

        public int GetTestInt(int id)
        {
            foreach (KeyValuePair<int, int> i in m_TestInt)
            {
                if (i.Key == id)
                {
                    return i.Value;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("GetTestInt with invalid id '{0}'.", id.ToString()));
        }

        public int GetTestIntAt(int index)
        {
            if (index < 0 || index >= m_TestInt.Length)
            {
                throw new GameFrameworkException(Utility.Text.Format("GetTestIntAt with invalid index '{0}'.", index.ToString()));
            }

            return m_TestInt[index].Value;
        }

        private void GeneratePropertyArray()
        {
            m_TestInt = new KeyValuePair<int, int>[]
            {
                new KeyValuePair<int, int>(1, TestInt1),
                new KeyValuePair<int, int>(2, TestInt2),
            };
        }
    }
}
