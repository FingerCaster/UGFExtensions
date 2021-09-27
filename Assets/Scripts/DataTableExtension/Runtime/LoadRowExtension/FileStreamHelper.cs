using System;
using System.IO;

namespace UGFExtensions
{
    public static class FileStreamHelper
    {
        private const string AndroidFileSystemPrefixString = "jar:";

        /// <summary>
        /// 创建文件流。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <returns>创建的文件流。</returns>
        public static IFileStream CreateFileStream(string fullPath)
        {
            if (fullPath.StartsWith(AndroidFileSystemPrefixString, StringComparison.Ordinal))
            {
                return AndroidFileStream.Create(fullPath);
            }
            else
            {
                return CommonFileStream.Create(fullPath);
            }
        }
    }
}