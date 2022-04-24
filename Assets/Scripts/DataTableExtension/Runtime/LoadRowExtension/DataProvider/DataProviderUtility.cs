namespace UGFExtensions
{
    public static class DataProviderUtility
    {
        /// <summary>
        /// 保证缓存大小
        /// </summary>
        /// <param name="count">数据大小</param>
        /// <param name="buffer">缓存</param>
        public static void EnsureBufferSize(int count,ref byte[] buffer)
        {
            int length = buffer.Length;
            while (length < count)
            {
                length *= 2;
            }

            if (length != buffer.Length)
            {
                buffer = new byte[length];
            }
        }
    }
}