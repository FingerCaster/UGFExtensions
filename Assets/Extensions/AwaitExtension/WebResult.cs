namespace UGFExtensions.Await
{
    /// <summary>
    /// web 访问结果
    /// </summary>
    public class WebResult
    {
        /// <summary>
        /// web请求 返回数据
        /// </summary>
        public byte[] Bytes { get; private set; }
        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool IsError { get; private set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; private set; }


        public static WebResult Create(byte[] bytes, bool isError, string errorMessage, object userData)
        {
            return new WebResult(bytes, isError, errorMessage, userData);
        }

        private WebResult(byte[] bytes, bool isError, string errorMessage, object userData)
        {
            Bytes = bytes;
            IsError = isError;
            ErrorMessage = errorMessage;
            UserData = userData;
        }
    }
}
