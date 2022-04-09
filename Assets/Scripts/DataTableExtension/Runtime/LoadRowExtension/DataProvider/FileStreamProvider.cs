using System.IO;

namespace UGFExtensions
{
    /// <summary>
    /// 文件流数据提供
    /// </summary>
    public class FileStreamProvider : IDataProvider
    {
        private IFileStream m_FileStream = null;
        private string m_FilePath = null;
        private bool m_IsCached = false;

        public FileStreamProvider(string filePath, bool isCached)
        {
            m_FilePath = filePath;
            m_IsCached = isCached;
        }

        public IFileStream GetOrCreateFileSystem()
        {
            if (m_FileStream != null)
            {
                return m_FileStream;
            }

            var fileStream = FileStreamHelper.CreateFileStream(m_FilePath);
            if (m_IsCached)
            {
                m_FileStream = fileStream;
            }

            return fileStream;
        }

        public long ReadFileSegment(int offset, ref byte[] buffer, int startIndex, int length)
        {
            var fileStream = GetOrCreateFileSystem();
            fileStream.Seek(offset, SeekOrigin.Begin);
            DataProviderUtility.EnsureBufferSize(startIndex+length, ref buffer);
            var realLength = fileStream.Read(buffer, 0, length);
            if (!m_IsCached)
            {
                fileStream.Dispose();
            }

            return realLength;
        }

        public void Dispose()
        {
            m_FileStream?.Dispose();
            m_FilePath = null;
            m_IsCached = false;
        }
    }
}