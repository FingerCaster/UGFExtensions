namespace UGFExtensions
{
    /// <summary>
    /// 虚拟文件系统数据提供方(打包设置FileSystem 系统合并文件)
    /// </summary>
    public class VirtualFileSystemDataProvider : IDataProvider
    {
        private string m_AssetPath = null;

        public VirtualFileSystemDataProvider(string assetPath)
        {
            m_AssetPath = assetPath;
        }

        public void Dispose()
        {
            m_AssetPath = null;
        }

        public long ReadFileSegment(int offset, ref byte[] buffer, int startIndex, int length)
        {
            DataProviderUtility.EnsureBufferSize(startIndex+length, ref buffer);
            return GameEntry.Resource.LoadBinarySegmentFromFileSystem(m_AssetPath,offset, buffer, startIndex, length);
        }
    }
}