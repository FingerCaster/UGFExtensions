using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.FileSystem;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    public class DataTableExtensionComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        [SerializeField] private int m_InitBufferLength = 1024 * 64;

        /// <summary>
        /// 图片加载缓存
        /// </summary>
        private byte[] m_Buffer;

        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        private Dictionary<Type, DataTableRowConfig> m_DataTableRowConfigs;

        private void Start()
        {
            m_DataTableRowConfigs = new Dictionary<Type, DataTableRowConfig>();
            m_Buffer = new byte[m_InitBufferLength];
        }

        public void LoadDataTableRowConfig<T>(string assetName) where T : class, IDataRow, new()
        {
            string filePath;
            if (!GameEntry.Base.EditorResourceMode)
            {
                bool isSuccess = GameEntry.Resource.GetBinaryPath(assetName,
                    out var isStorageInReadOnly, out var isStorageInFileSystem,
                    out var relativePath, out var filename);
                if (!isSuccess)
                {
                    throw new Exception("DataTable binary asset is not exist.");
                }

                if (isStorageInFileSystem)
                {
                    throw new Exception("DataTable binary asset can not in filesystem.");
                }

                filePath = Utility.Path.GetRegularPath(isStorageInReadOnly
                    ? Path.Combine(GameEntry.Resource.ReadOnlyPath, relativePath)
                    : Path.Combine(GameEntry.Resource.ReadWritePath, relativePath));
            }
            else
            {
                filePath = assetName;
            }
            
            DataTableRowConfig rowConfig = new DataTableRowConfig
            {
                Path = filePath
            };

            using (IFileStream fileStream = FileStreamHelper.CreateFileStream(rowConfig.Path))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Read(m_Buffer, 0, 32);
                using (MemoryStream memoryStream = new MemoryStream(m_Buffer,0,32))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        int count = binaryReader.Read7BitEncodedInt32(out int length);
                        fileStream.Seek(length, SeekOrigin.Begin);
                        EnsureBufferSize(count);
                        long configLength = fileStream.Read(m_Buffer, 0, count);
                        rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length+count);
                    }
                }
            }

            m_DataTableRowConfigs.Add(typeof(T), rowConfig);
            GameEntry.DataTable.CreateDataTable<T>();
        }

        public T GetDataRow<T>(int id) where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeof(T), out var config);
            if (config == null) return default;
            config.DataTableRowSettings.TryGetValue(id, out var value);
            if (value == null) return default;
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>();
            if (dataTableBase.HasDataRow(id))
            {
                return dataTableBase.GetDataRow(id);
            }

            using (IFileStream fileStream = FileStreamHelper.CreateFileStream(config.Path))
            {
                fileStream.Seek(value.StartIndex, SeekOrigin.Begin);
                EnsureBufferSize(value.Length);
                long length = fileStream.Read(m_Buffer, 0, value.Length);
                dataTableBase.AddDataRow(m_Buffer, 0, (int)length, null);
                return dataTableBase.GetDataRow(id);
            }
        }

        public T[] GetAllDataRows<T>() where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeof(T), out var config);
            if (config == null) return default;
            using (IFileStream fileStream = FileStreamHelper.CreateFileStream(config.Path))
            {
                IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>();
                foreach (var dataTableSetting in config.DataTableRowSettings)
                {
                    if (dataTableBase.HasDataRow(dataTableSetting.Key))
                    {
                        continue;
                    }

                    fileStream.Seek(dataTableSetting.Value.StartIndex, SeekOrigin.Begin);
                    EnsureBufferSize(dataTableSetting.Value.Length);
                    long length = fileStream.Read(m_Buffer, 0, dataTableSetting.Value.Length);
                    dataTableBase.AddDataRow(m_Buffer, 0, (int)length, null);
                }

                return dataTableBase.GetAllDataRows();
            }
        }

        /// <summary>
        /// 保证缓存大小
        /// </summary>
        /// <param name="count">读取文件大小</param>
        private void EnsureBufferSize(int count)
        {
            int length = m_Buffer.Length;
            while (length < count)
            {
                length *= 2;
            }

            if (length != m_Buffer.Length)
            {
                m_Buffer = new byte[length];
            }
        }
    }
}