using System.IO;

namespace UGFExtensions
{
    public class CommonFileStream : IFileStream
    {
        private FileStream m_FileStream;
        private CommonFileStream()
        {
            
        }

        public static CommonFileStream Create(string filePath)
        {
            CommonFileStream commonFileStream = new CommonFileStream();
            commonFileStream.m_FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return commonFileStream;
        }

        public void Seek(long offset, SeekOrigin seekOrigin)
        {
            m_FileStream.Seek(offset, SeekOrigin.Begin);
        }

        public long Read(byte[] buffer, int offset, int count)
        {
           return m_FileStream.Read(buffer, offset, count);
        }

        // public long Position {
        //     get => m_FileStream.Position;
        //     set => m_FileStream.Seek(value, SeekOrigin.Begin);
        // }

        public void Dispose()
        {
            m_FileStream.Dispose();
        }
    }
}