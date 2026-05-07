using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using GameFramework.FileSystem;

namespace UGFExtensions
{
    /// <summary>
    /// 数据表行配置
    /// </summary>
    public class DataTableRowConfig
    {
        /// <summary>
        /// 配置长度
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 数据表所有行设置
        /// </summary>
        public Dictionary<int, DataTableRowSetting> DataTableRowSettings { get; set; }
        /// <summary>
        /// 数据提供者
        /// </summary>
        public IDataProvider DataProvider { get; set; }
#if UNITY_EDITOR
        /// <summary>
        /// 序列化数据表配置
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
                {
                    binaryWriter.Write7BitEncodedInt32(Count);
                    foreach (KeyValuePair<int, DataTableRowSetting> item in DataTableRowSettings)
                    {
                        binaryWriter.Write7BitEncodedInt32(item.Key);
                        binaryWriter.Write7BitEncodedInt32(item.Value.StartIndex);
                        binaryWriter.Write7BitEncodedInt32(item.Value.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }
#endif
        /// <summary>
        /// 反序列化数据表配置
        /// </summary>
        /// <param name="bytes">数据</param>
        /// <param name="startIndex">数据起始位置</param>
        /// <param name="length">数据大小</param>
        /// <param name="filePosition">文件流位置</param>
        public void DeSerialize(byte[] bytes, int startIndex, int length, int filePosition)
        {
            if (bytes == null)
            {
                throw new GameFrameworkException("Data table row config bytes is invalid.");
            }

            if (startIndex < 0 || length < 0 || filePosition < 0 || startIndex > bytes.Length - length)
            {
                throw new GameFrameworkException("Data table row config range is invalid.");
            }

            using (MemoryStream memoryStream = new MemoryStream(bytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    Count = binaryReader.Read7BitEncodedInt32();
                    if (Count < 0 || Count > length / 3)
                    {
                        throw new GameFrameworkException("Data table row config count is invalid.");
                    }

                    DataTableRowSettings = new Dictionary<int, DataTableRowSetting>(Count);
                    for (int i = 0; i < Count; i++)
                    {
                        int key = binaryReader.Read7BitEncodedInt32();
                        int rowStartIndex = binaryReader.Read7BitEncodedInt32();
                        int rowLength = binaryReader.Read7BitEncodedInt32();
                        if (key < 0 || rowStartIndex < 0 || rowLength <= 0 || rowStartIndex > int.MaxValue - filePosition)
                        {
                            throw new GameFrameworkException("Data table row setting is invalid.");
                        }

                        DataTableRowSetting value = new DataTableRowSetting(rowStartIndex + filePosition, rowLength);
                        DataTableRowSettings.Add(key, value);
                    }

                    if (memoryStream.Position != memoryStream.Length)
                    {
                        throw new GameFrameworkException("Data table row config length is invalid.");
                    }
                }
            }
        }
    }
}
