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
    public partial class DataTableExtensionComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        [SerializeField] private int m_InitBufferLength = 1024 * 64;

        /// <summary>
        /// 加载缓存
        /// </summary>
        private byte[] m_Buffer;

        /// <summary>
        /// 所有数据表配置
        /// </summary>
        private Dictionary<TypeNamePair, DataTableRowConfig> m_DataTableRowConfigs;

        private void Start()
        {
            m_DataTableRowConfigs = new Dictionary<TypeNamePair, DataTableRowConfig>();
            m_Buffer = new byte[m_InitBufferLength];
        }
        /// <summary>
        /// 获取数据表数据数量
        /// </summary>
        /// <param name="dataTable">数据表名</param>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <returns>数据表数据数量</returns>
        public int GetCount<T>(string dataTable) where T : class, IDataRow, new()=>InternalGetCount(new TypeNamePair(typeof(T),dataTable));
        /// <summary>
        /// 获取数据表数据数量
        /// </summary>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <returns>数据表数据数量</returns>
        public int GetCount<T>() => InternalGetCount(new TypeNamePair(typeof(T)));

        private int InternalGetCount(TypeNamePair typeNamePair)
        {
            if (!m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config))
            {
                throw new Exception("GetCount must be load datatable row config !");
            }
            
            return config.DataTableRowSettings.Count;
        }

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="isCacheFileStream">是否缓存文件流</param>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig<T>(string assetName, bool isCacheFileStream = true)
            where T : class, IDataRow, new() =>
            InternalLoadDataTableRowConfig(assetName, new TypeNamePair(typeof(T)), isCacheFileStream);

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="type">数据表类型</param>
        /// <param name="assetName">资源名</param>
        /// <param name="isCacheFileStream">是否缓存文件流</param>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig(Type type, string assetName, bool isCacheFileStream = true)
            => InternalLoadDataTableRowConfig(assetName, new TypeNamePair(type), isCacheFileStream);

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="dataTable">数据表名</param>
        /// <param name="isCacheFileStream">是否缓存文件流</param>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig<T>(string assetName, string dataTable, bool isCacheFileStream = true)
            where T : class, IDataRow, new() =>
            InternalLoadDataTableRowConfig(assetName, new TypeNamePair(typeof(T), dataTable), isCacheFileStream);

        private void InternalLoadDataTableRowConfig(string assetName, TypeNamePair typeNamePair,
            bool isCacheFileStream = true)
        {
            if (m_DataTableRowConfigs.TryGetValue(typeNamePair, out _))
            {
                return;
            }

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

            IFileStream fileStream = FileStreamHelper.CreateFileStream(filePath);

            DataTableRowConfig rowConfig = new DataTableRowConfig
            {
                Path = filePath,
                FileStream = fileStream,
            };

            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.Read(m_Buffer, 0, 32);
            using (MemoryStream memoryStream = new MemoryStream(m_Buffer, 0, 32))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int count = binaryReader.Read7BitEncodedInt32(out int length);
                    fileStream.Seek(length, SeekOrigin.Begin);
                    EnsureBufferSize(count);
                    long configLength = fileStream.Read(m_Buffer, 0, count);
                    rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length + count);
                }
            }

            if (!isCacheFileStream)
            {
                fileStream.Dispose();
                rowConfig.FileStream = null;
            }

            m_DataTableRowConfigs.Add(typeNamePair, rowConfig);
            GameEntry.DataTable.CreateDataTable(typeNamePair.Type, typeNamePair.Name);
        }


        public T GetDataRow<T>(int id,object userdata = null) where T : class, IDataRow, new() =>
            InternalGetDataRow<T>(new TypeNamePair(typeof(T)), id,userdata);

        public T GetDataRow<T>(string dataTableName, int id,object userdata = null) where T : class, IDataRow, new() =>
            InternalGetDataRow<T>(new TypeNamePair(typeof(T), dataTableName), id,userdata);

        private T InternalGetDataRow<T>(TypeNamePair typeNamePair, int id,object userdata = null) where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config);
            if (config == null) return default;
            config.DataTableRowSettings.TryGetValue(id, out var value);
            if (value == null) return default;
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>(typeNamePair.Name);
            if (dataTableBase.HasDataRow(id))
            {
                return dataTableBase.GetDataRow(id);
            }

            if (config.FileStream != null)
            {
                AddDataRow(dataTableBase, config.FileStream, value.StartIndex, value.Length,userdata);
            }
            else
            {
                using (IFileStream fileStream = FileStreamHelper.CreateFileStream(config.Path))
                {
                    AddDataRow(dataTableBase, fileStream, value.StartIndex, value.Length,userdata);
                }
            }

            return dataTableBase.GetDataRow(id);
        }

        private void AddDataRow<T>(IDataTable<T> dataTable, IFileStream fileStream, int startIndex, int length,object userdata = null)
            where T : class, IDataRow, new()
        {
            fileStream.Seek(startIndex, SeekOrigin.Begin);
            EnsureBufferSize(length);
            long readLength = fileStream.Read(m_Buffer, 0, length);
            dataTable.AddDataRow(m_Buffer, 0, (int)readLength, userdata);
        }

        public T[] GetAllDataRows<T>(object userdata = null) where T : class, IDataRow, new() =>
            InternalGetAllDataRows<T>(new TypeNamePair(typeof(T)),userdata);

        public T[] GetAllDataRows<T>(string dataTableName,object userdata = null) where T : class, IDataRow, new() =>
            InternalGetAllDataRows<T>(new TypeNamePair(typeof(T), dataTableName),userdata);

        private T[] InternalGetAllDataRows<T>(TypeNamePair typeNamePair,object userdata = null) where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config);
            if (config == null) return default;
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>(typeNamePair.Name);

            IFileStream fileStream = config.FileStream ?? FileStreamHelper.CreateFileStream(config.Path);
            using (fileStream)
            {
                foreach (var dataTableSetting in config.DataTableRowSettings)
                {
                    if (dataTableBase.HasDataRow(dataTableSetting.Key))
                    {
                        continue;
                    }

                    AddDataRow(dataTableBase, fileStream, dataTableSetting.Value.StartIndex,
                        dataTableSetting.Value.Length,userdata);
                }

                return dataTableBase.GetAllDataRows();
            }
        }

        public bool InternalDestroyDataTable<T>() where T : IDataRow =>
            InternalDestroyDataTable<T>(new TypeNamePair(typeof(T)));

        public bool InternalDestroyDataTable<T>(string dataTableName) where T : IDataRow =>
            InternalDestroyDataTable<T>(new TypeNamePair(typeof(T), dataTableName));

        private bool InternalDestroyDataTable<T>(TypeNamePair typeNamePair) where T : IDataRow
        {
            IDataTable<T> dataTable = GameEntry.DataTable.GetDataTable<T>();
            if (dataTable == null)
            {
                return true;
            }

            var result = GameEntry.DataTable.DestroyDataTable(dataTable);
            if (result)
            {
                if (m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config))
                {
                    config.FileStream.Dispose();
                    config.FileStream = null;
                    m_DataTableRowConfigs.Remove(typeNamePair);
                }
            }

            return result;
        }

        public T GetDataRow<T>(string dataTableName,Predicate<T> condition) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>(dataTableName);
            return dataTableBase.GetDataRow(condition);
        }
        public T GetDataRow<T>(Predicate<T> condition) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>();
            return dataTableBase.GetDataRow(condition);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<TypeNamePair, DataTableRowConfig> item in m_DataTableRowConfigs)
            {
                item.Value.FileStream.Dispose();
                item.Value.FileStream = null;
            }

            m_DataTableRowConfigs = null;
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