using GameFramework.FileSystem;

namespace UGFExtensions
{
    public class CustomVirtualFileSystemDataProvider : IDataProvider
    {
        private string m_FilePath = null;
        private string m_AssetPath = null;
        private bool m_IsCached = false;
        private IFileSystem m_FileSystem = null;

        public CustomVirtualFileSystemDataProvider(string filePath, string assetPath, bool isCached)
        {
            m_FilePath = filePath;
            m_AssetPath = assetPath;
            m_IsCached = isCached;
        }

        public IFileSystem GetOrCreateFileSystem()
        {
            if (m_FileSystem != null)
            {
                return m_FileSystem;
            }

            var fileSystem = GameEntry.FileSystem.LoadFileSystem(m_FilePath, FileSystemAccess.Read);
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
                GameEntry.FileSystem.DestroyFileSystem(fileSystem,false);
            }

            return realLength;
        }

        public void Dispose()
        {
            m_FilePath = null;
            m_AssetPath = null;
            m_IsCached = false;
            GameEntry.FileSystem.DestroyFileSystem(m_FileSystem,false);
            m_FileSystem = null;
        }
    }
}