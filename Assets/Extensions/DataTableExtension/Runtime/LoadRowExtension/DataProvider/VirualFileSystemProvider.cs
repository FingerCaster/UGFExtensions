using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    /// <summary>
    /// 虚拟文件系统数据提供方(打包设置FileSystem 系统合并文件)
    /// </summary>
    public class VirtualFileSystemDataProvider : IDataProvider
    {
        private string m_AssetPath = null;
        private static ResourceComponent m_ResourceComponent;
        public VirtualFileSystemDataProvider(ResourceComponent resourceComponent,string assetPath)
        {
            m_ResourceComponent = resourceComponent;
            m_AssetPath = assetPath;
        }

        public void Dispose()
        {
            m_AssetPath = null;
            m_ResourceComponent = null;
        }

        public long ReadFileSegment(int offset, ref byte[] buffer, int startIndex, int length)
        {
            DataProviderUtility.EnsureBufferSize(startIndex+length, ref buffer);
            return m_ResourceComponent.LoadBinarySegmentFromFileSystem(m_AssetPath,offset, buffer, startIndex, length);
        }
    }
}