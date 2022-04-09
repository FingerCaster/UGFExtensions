namespace UGFExtensions
{
    public static class DataProviderUtility
    {
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