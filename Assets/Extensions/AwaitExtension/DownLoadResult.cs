namespace UGFExtensions.Await
{
    /// <summary>
    /// DownLoad 结果
    /// </summary>
    public class DownLoadResult
    {
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

        public static DownLoadResult Create(bool isError, string errorMessage, object userData)
        {
            return new DownLoadResult(isError, errorMessage, userData);
        }

        private DownLoadResult(bool isError, string errorMessage, object userData)
        {
            IsError = isError;
            ErrorMessage = errorMessage;
            UserData = userData;
        }
    }
}
