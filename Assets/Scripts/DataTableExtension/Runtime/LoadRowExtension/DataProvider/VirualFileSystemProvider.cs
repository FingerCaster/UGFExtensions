namespace UGFExtensions
{
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