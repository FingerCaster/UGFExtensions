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

        private BaseComponent m_BaseComponent;
        private ResourceComponent m_ResourceComponent;
        private DataTableComponent m_DataTableComponent;
        private FileSystemComponent m_FileSystemComponent;
        private void Start()
        {
            m_BaseComponent = GameEntry.GetComponent<BaseComponent>();
            m_ResourceComponent = GameEntry.GetComponent<ResourceComponent>();
            m_DataTableComponent = GameEntry.GetComponent<DataTableComponent>();
            m_FileSystemComponent = GameEntry.GetComponent<FileSystemComponent>();
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
        public void LoadDataTableRowConfig<T>(string assetName, string dataTable, string fileSystem,
            bool isCache = true)
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
        public void LoadDataTableRowConfig(Type type, string assetName, string dataTable, string fileSystem,
            bool isCache = true)
            => InternalLoadDataTableRowConfig(assetName, new TypeNamePair(type, dataTable), fileSystem, isCache);

        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="filePath">文件路径(如果在文件系统那么返回assetPath,否则返回具体文件路径)</param>
        /// <returns>返回二进制资源是否存在于文件系统</returns>
        /// <exception cref="Exception">不存在二进制文件</exception>
        private bool GetFilePath(string assetPath, out string filePath)
        {
            if (!m_BaseComponent.EditorResourceMode)
            {
                bool isSuccess = m_ResourceComponent.GetBinaryPath(assetPath,
                    out var isStorageInReadOnly, out var isStorageInFileSystem,
                    out var relativePath, out _);
                if (!isSuccess)
                {
                    throw new Exception($"DataTable binary asset {assetPath} is not exist.");
                }

                if (isStorageInFileSystem)
                {
                    filePath = assetPath;
                    return true;
                }

                filePath = Utility.Path.GetRegularPath(isStorageInReadOnly
                    ? Path.Combine(m_ResourceComponent.ReadOnlyPath, relativePath)
                    : Path.Combine(m_ResourceComponent.ReadWritePath, relativePath));
            }
            else
            {
                filePath = assetPath;
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
                rowConfig.DataProvider = new VirtualFileSystemDataProvider(m_ResourceComponent,assetName);
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
                    long configLength = rowConfig.DataProvider.ReadFileSegment(length, ref m_Buffer, 0, count);
                    rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length + count);
                }
            }

            m_DataTableRowConfigs.Add(typeNamePair, rowConfig);
            m_DataTableComponent.CreateDataTable(typeNamePair.Type, typeNamePair.Name);
        }

        private void InternalLoadDataTableRowConfig(string assetName, TypeNamePair typeNamePair, string fileSystem,
            bool isCache = true)
        {
            if (m_DataTableRowConfigs.TryGetValue(typeNamePair, out _))
            {
                return;
            }

            DataTableRowConfig rowConfig = new DataTableRowConfig();

            rowConfig.DataProvider = new CustomVirtualFileSystemDataProvider(m_FileSystemComponent,fileSystem, assetName, isCache);

            rowConfig.DataProvider.ReadFileSegment(0, ref m_Buffer, 0, 32);
            using (MemoryStream memoryStream = new MemoryStream(m_Buffer, 0, 32))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int count = binaryReader.Read7BitEncodedInt32(out int length);
                    long configLength = rowConfig.DataProvider.ReadFileSegment(length, ref m_Buffer, 0, count);
                    rowConfig.DeSerialize(m_Buffer, 0, (int)configLength, length + count);
                }
            }

            m_DataTableRowConfigs.Add(typeNamePair, rowConfig);
            m_DataTableComponent.CreateDataTable(typeNamePair.Type, typeNamePair.Name);
        }


        /// <summary>获取数据表行。</summary>
        /// <param name="id">数据表行的编号。</param>
        /// <returns>数据表行。</returns>
        public T GetDataRow<T>(int id) where T : class, IDataRow, new() =>
            InternalGetDataRow<T>(new TypeNamePair(typeof(T)), id);

        /// <summary>获取数据表行。</summary>
        /// <param name="dataTableName">数据表名</param>
        /// <param name="id">数据表行的编号。</param>
        /// <returns>数据表行。</returns>
        public T GetDataRow<T>(string dataTableName, int id) where T : class, IDataRow, new() =>
            InternalGetDataRow<T>(new TypeNamePair(typeof(T), dataTableName), id);

        private T InternalGetDataRow<T>(TypeNamePair typeNamePair, int id)
            where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config);
            if (config == null) return default;
            config.DataTableRowSettings.TryGetValue(id, out var value);
            if (value == null) return default;
            IDataTable<T> dataTableBase = m_DataTableComponent.GetDataTable<T>(typeNamePair.Name);
            if (dataTableBase.HasDataRow(id))
            {
                return dataTableBase.GetDataRow(id);
            }

            var realLength = config.DataProvider.ReadFileSegment(value.StartIndex, ref m_Buffer, 0,
                value.Length);
            dataTableBase.AddDataRow(m_Buffer, 0, (int)realLength, null);
            return dataTableBase.GetDataRow(id);
        }

        /// <summary>获取所有数据表行。</summary>
        public T[] GetAllDataRows<T>() where T : class, IDataRow, new() =>
            InternalGetAllDataRows<T>(new TypeNamePair(typeof(T)));

        /// <summary>获取所有数据表行。</summary>
        /// <param name="dataTableName">数据表名称</param>
        public T[] GetAllDataRows<T>(string dataTableName) where T : class, IDataRow, new() =>
            InternalGetAllDataRows<T>(new TypeNamePair(typeof(T), dataTableName));

        private T[] InternalGetAllDataRows<T>(TypeNamePair typeNamePair)
            where T : class, IDataRow, new()
        {
            m_DataTableRowConfigs.TryGetValue(typeNamePair, out var config);
            if (config == null) return default;
            IDataTable<T> dataTableBase = m_DataTableComponent.GetDataTable<T>(typeNamePair.Name);
            foreach (var dataTableSetting in config.DataTableRowSettings)
            {
                if (dataTableBase.HasDataRow(dataTableSetting.Key))
                {
                    continue;
                }

                var realLength = config.DataProvider.ReadFileSegment(dataTableSetting.Value.StartIndex, ref m_Buffer, 0,
                    dataTableSetting.Value.Length);
                dataTableBase.AddDataRow(m_Buffer, 0, (int)realLength, null);
            }

            return dataTableBase.GetAllDataRows();
        }

        public bool InternalDestroyDataTable<T>() where T : IDataRow =>
            InternalDestroyDataTable<T>(new TypeNamePair(typeof(T)));

        public bool InternalDestroyDataTable<T>(string dataTableName) where T : IDataRow =>
            InternalDestroyDataTable<T>(new TypeNamePair(typeof(T), dataTableName));

        private bool InternalDestroyDataTable<T>(TypeNamePair typeNamePair) where T : IDataRow
        {
            IDataTable<T> dataTable = m_DataTableComponent.GetDataTable<T>();
            if (dataTable == null)
            {
                return true;
            }

            var result = m_DataTableComponent.DestroyDataTable(dataTable);
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

        /// <summary>获取符合条件的数据表行。</summary>
        /// <param name="dataTableName">数据表名</param>
        /// <param name="condition">要检查的条件。</param>
        public T GetDataRow<T>(string dataTableName, Predicate<T> condition) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTableBase = m_DataTableComponent.GetDataTable<T>(dataTableName);
            return dataTableBase.GetDataRow(condition);
        }

        /// <summary>获取符合条件的数据表行。</summary>
        /// <param name="condition">要检查的条件。</param>
        public T GetDataRow<T>(Predicate<T> condition) where T : class, IDataRow, new()
        {
            IDataTable<T> dataTableBase = m_DataTableComponent.GetDataTable<T>();
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