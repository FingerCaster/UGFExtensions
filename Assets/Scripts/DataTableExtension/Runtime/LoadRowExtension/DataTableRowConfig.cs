using System.Collections.Generic;
using System.IO;
using System.Text;

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
        /// 数据表文件路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 文件流
        /// </summary>
        public IFileStream FileStream { get; set; }
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
            using (MemoryStream memoryStream = new MemoryStream(bytes, startIndex, length, false))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    Count = binaryReader.Read7BitEncodedInt32();
                    DataTableRowSettings = new Dictionary<int, DataTableRowSetting>(Count);
                    for (int i = 0; i < Count; i++)
                    {
                        int key = binaryReader.Read7BitEncodedInt32();
                        DataTableRowSetting value = new DataTableRowSetting(
                            binaryReader.Read7BitEncodedInt32() + filePosition, binaryReader.Read7BitEncodedInt32());
                        DataTableRowSettings.Add(key, value);
                    }
                }
            }
        }
    }
}