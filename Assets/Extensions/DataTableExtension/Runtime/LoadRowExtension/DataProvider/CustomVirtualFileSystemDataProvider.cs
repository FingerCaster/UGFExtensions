using GameFramework.FileSystem;
using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    /// <summary>
    /// 自定义虚拟文件系统数据提供(mergeAsset扩展合并的vfs文件  或者自行合并的vfs文件)
    /// </summary>
    public class CustomVirtualFileSystemDataProvider : IDataProvider
    {
        private string m_FilePath = null;
        private string m_AssetPath = null;
        private bool m_IsCached = false;
        private IFileSystem m_FileSystem = null;
        private FileSystemComponent m_FileSystemComponent;
        public CustomVirtualFileSystemDataProvider(FileSystemComponent fileSystemComponent,string filePath, string assetPath, bool isCached)
        {
            m_FileSystemComponent = fileSystemComponent;
            m_FilePath = filePath;
            m_AssetPath = assetPath;
            m_IsCached = isCached;
        }

        private IFileSystem GetOrCreateFileSystem()
        {
            if (m_FileSystem != null)
            {
                return m_FileSystem;
            }

            var fileSystem = m_FileSystemComponent.LoadFileSystem(m_FilePath, FileSystemAccess.Read);
            if (m_IsCached)
            {
                m_FileSystem = fileSystem;
            }

            return m_FileSystem;
        }
        
        public long ReadFileSegment(int offset, ref byte[] buffer, int startIndex, int length)
        {
            var fileSystem = GetOrCreateFileSystem();
            DataProviderUtility.EnsureBufferSize(startIndex+length, ref buffer);
            var realLength = fileSystem.ReadFileSegment(m_AssetPath,offset,buffer, startIndex, length);
            if (!m_IsCached)
            {
                m_FileSystemComponent.DestroyFileSystem(fileSystem,false);
            }

            return realLength;
        }

        public void Dispose()
        {
            m_FilePath = null;
            m_AssetPath = null;
            m_IsCached = false;
            m_FileSystemComponent.DestroyFileSystem(m_FileSystem,false);
            m_FileSystemComponent = null;
            m_FileSystem = null;
        }
    }
}