using System;
using GameFramework.FileSystem;

namespace UGFExtensions
{
    public interface IDataProvider : IDisposable
    {
        /// <summary>
        /// 读取文件的片段。
        /// </summary>
        /// <param name="offset">要读取片段的偏移。</param>
        /// <param name="buffer">存储读取二进制资源片段内容的二进制流。</param>
        /// <param name="startIndex">存储读取二进制资源片段内容的二进制流的起始位置。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际加载了多少字节。</returns>
        long ReadFileSegment(int offset, ref byte[] buffer, int startIndex, int length);
    }
}