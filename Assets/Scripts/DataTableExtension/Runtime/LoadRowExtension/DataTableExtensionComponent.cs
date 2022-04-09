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
        public int GetCount<T>(string dataTable) where T : class, IDataRow, new() =>
            InternalGetCount(new TypeNamePair(typeof(T), dataTable));

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
        /// <param name="isCache">是否缓存文件流</param>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig<T>(string assetName, bool isCache = true)
            where T : class, IDataRow, new() =>
            InternalLoadDataTableRowConfig(assetName, new TypeNamePair(typeof(T)), isCache);

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="type">数据表类型</param>
        /// <param name="assetName">资源名</param>
        /// <param name="isCache">是否缓存文件流</param>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig(Type type, string assetName, bool isCache = true)
            => InternalLoadDataTableRowConfig(assetName, new TypeNamePair(type), isCache);

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="dataTable">数据表名</param>
        /// <param name="isCache">是否缓存文件流</param>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig<T>(string assetName, string dataTable, bool isCache = true)
            where T : class, IDataRow, new() =>
            InternalLoadDataTableRowConfig(assetName, new TypeNamePair(typeof(T), dataTable), isCache);
        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="type">数据表类型</param>
        /// <param name="assetName">资源名</param>
        /// <param name="dataTable"></param>
        /// <param name="isCache">是否缓存文件流</param>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig(Type type, string assetName, string dataTable, bool isCache = true)
            => InternalLoadDataTableRowConfig(assetName, new TypeNamePair(type, dataTable), isCache);

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="dataTable">数据表名</param>
        /// <param name="fileSystem"></param>
        /// <param name="isCache">是否缓存文件流</param>
        /// <typeparam name="T">数据表类型</typeparam>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig<T>(string assetName, string dataTable,string fileSystem, bool isCache = true)
            where T : class, IDataRow, new() =>
            InternalLoadDataTableRowConfig(assetName, new TypeNamePair(typeof(T), dataTable), fileSystem, isCache);

        /// <summary>
        /// 加载数据表配置
        /// </summary>
        /// <param name="type">数据表类型</param>
        /// <param name="assetName">资源名</param>
        /// <param name="dataTable"></param>
        /// <param name="fileSystem"></param>
        /// <param name="isCache">是否缓存文件流</param>
        /// <exception cref="Exception"></exception>
        public void LoadDataTableRowConfig(Type type, string assetName, string dataTable,string fileSystem, bool isCache = true)
            => InternalLoadDataTableRowConfig(assetName, new TypeNamePair(type, dataTable),fileSystem, isCache);
        private bool GetFilePath(string assetName, out string filePath)
        {
            if (!GameEntry.Base.EditorResourceMode)
            {
                bool isSuccess = GameEntry.Resource.GetBinaryPath(assetName,
                    out var isStorageInReadOnly, out var isStorageInFileSystem,
                    out var relativePath, out var fileName);
                if (!isSuccess)
                {
                    throw new Exception($"DataTable binary asset {assetName} is not exist.");
                }

                if (isStorageInFileSystem)
                {
                    filePath = assetName;
                    return true;
                }

                filePath = Utility.Path.GetRegularPath(isStorageInReadOnly
                    ? Path.Combine(GameEntry.Resource.ReadOnlyPath, relativePath)
                    : Path.Combine(GameEntry.Resource.ReadWritePath, relativePath));
            }
            else
            {
                filePath = assetName;
            }

            return false;
        }

        private void InternalLoadDataTableRowConfig(string assetName, TypeNamePair typeNamePair,
            bool isCache = true)
        {
            if (m_DataTableRowConfigs.TryGetValue(typeNamePair, out _))
            {
                return;
            }

            bool isStorageInFileSystem = GetFilePath(assetName, out var filePath);

            DataTableRowConfig rowConfig = new DataTableRowConfig();

            if (isStorageInFileSystem)
            {
                rowConfig.DataProvider = new VirtualFileSystemDataProvider(assetName);
            }
            else
            {
                rowConfig.DataProvider = new FileStreamProvider(filePath, isCache);
            }
            rowConfig.DataProvider.ReadFileSegment(0, ref m_Buffer, 0, 32);
            using (MemoryStream memoryStream = new MemoryStream(m_Buffer, 0, 32))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int count = binaryReader.Read7BitEncodedInt32(out int length);
                    long configLength = rowConfig.DataProvider.ReadFileSegment(length,ref m_Buffer, 0, count);
                    rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length + count);
                }
            }
            m_DataTableRowConfigs.Add(typeNamePair, rowConfig);
            GameEntry.DataTable.CreateDataTable(typeNamePair.Type, typeNamePair.Name);
        }

        private void InternalLoadDataTableRowConfig(string assetName, TypeNamePair typeNamePair,string fileSystem,
            bool isCache = true)
        {
            if (m_DataTableRowConfigs.TryGetValue(typeNamePair, out _))
            {
                return;
            }
            
            DataTableRowConfig rowConfig = new DataTableRowConfig();

            rowConfig.DataProvider = new CustomVirtualFileSystemDataProvider(fileSystem,assetName,isCache);

            rowConfig.DataProvider.ReadFileSegment(0, ref m_Buffer, 0, 32);
            using (MemoryStream memoryStream = new MemoryStream(m_Buffer, 0, 32))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int count = binaryReader.Read7BitEncodedInt32(out int length);
                    long configLength = rowConfig.DataProvider.ReadFileSegment(length,ref m_Buffer, 0, count);
                    rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length + count);
                }
            }
            m_DataTableRowConfigs.Add(typeNamePair, rowConfig);
            GameEntry.DataTable.CreateDataTable(typeNamePair.Type, typeNamePair.Name);
        }
        public T GetDataRow<T>(int id, object userdata = null) where T : class, IDataRow, new() =>
            InternalGetDataRow<T>(new TypeNamePair(typeof(T)), id, userdata);

        public T GetDataRow<T>(string dataTableName, int id, object userdata = null) where T : class, IDataRow, new() =>
            InternalGetDataRow<T>(new TypeNamePair(typeof(T), dataTableName), id, userdata);

        private T InternalGetDataRow<T>(TypeNamePair typeNamePair, int id, object userdata = null)
            where T : class, IDataRow, new()
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

            var realLength = config.DataProvider.ReadFileSegment(value.StartIndex, ref m_Buffer, 0,
                value.Length);
            dataTableBase.AddDataRow(m_Buffer, 0, (int)realLength, userdata);
            return dataTableBase.GetDataRow(id);
        }
        
        public T[] GetAllDataRows<T>(object userdata = null) where T : class, IDataRow, new() =>
            InternalGetAllDataRows<T>(new TypeNamePair(typeof(T)), userdata);

        public T[] GetAllDataRows<T>(string dataTableName, object userdata = null) where T : class, IDataRow, new() =>
            InternalGetAllDataRows<T>(new TypeNamePair(typeof(T), dataTableName), userdata);

        private T[] InternalGetAllDataRows<T>(TypeNamePair typeNamePair, object userdata = null)
            where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config);
            if (config == null) return default;
            IDataTable<T> dataTableBase = GameEntry.DataTable.GetDataTable<T>(typeNamePair.Name);
            foreach (var dataTableSetting in config.DataTableRowSettings)
            {
                if (dataTableBase.HasDataRow(dataTableSetting.Key))
                {
                    continue;
                }

                var realLength = config.DataProvider.ReadFileSegment(dataTableSetting.Value.StartIndex, ref m_Buffer, 0,
                    dataTableSetting.Value.Length);
                dataTableBase.AddDataRow(m_Buffer, 0, (int)realLength, userdata);
            }
            return dataTableBase.GetAllDataRows();
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
                    config.DataProvider.Dispose();
                    config.DataProvider = null;
                    m_DataTableRowConfigs.Remove(typeNamePair);
                }
            }

            return result;
        }

        public T GetDataRow<T>(string dataTableName, Predicate<T> condition) where T : class, IDataRow, new()
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
                item.Value.DataProvider.Dispose();
                item.Value.DataProvider = null;
            }

            m_DataTableRowConfigs = null;
        }
    }
}