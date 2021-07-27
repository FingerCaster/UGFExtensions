using GameFramework;

namespace UGFExtensions.Await
{
    /// <summary>
    /// web 访问结果
    /// </summary>
    public class WebResult : IReference
    {
        /// <summary>
        /// 获取 Web 请求任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }
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
       


        public static WebResult Create(int serialId,byte[] bytes, bool isError, string errorMessage, object userData)
        {
            WebResult webResult = ReferencePool.Acquire<WebResult>();
            webResult.SerialId = serialId;
            webResult.Bytes = bytes;
            webResult.IsError = isError;
            webResult.ErrorMessage = errorMessage;
            webResult.UserData = userData;
            return webResult;
        }
        
        public void Clear()
        {
            SerialId = 0;
            Bytes = null;
            IsError = false;
            ErrorMessage = string.Empty;
            UserData = null;
        }
    }
}