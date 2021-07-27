using System;
using GameFramework;

namespace UGFExtensions.Await
{
    /// <summary>
    /// DownLoad 结果
    /// </summary>
    public class DownLoadResult : IReference
    {
        /// <summary>
        /// 获取 Download 请求任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }
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

        public static DownLoadResult Create(int serialId,bool isError, string errorMessage, object userData)
        {
            DownLoadResult downLoadResult = ReferencePool.Acquire<DownLoadResult>();
            downLoadResult.SerialId = serialId;
            downLoadResult.IsError = isError;
            downLoadResult.ErrorMessage = errorMessage;
            downLoadResult.UserData = userData;
            return downLoadResult;
        }

        public void Clear()
        {
            SerialId = 0;
            IsError = false;
            ErrorMessage = string.Empty;
            UserData = null;
        }
    }
}