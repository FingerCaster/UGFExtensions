using System;
using System.IO;
using GameFramework;
using UnityEngine;

namespace UGFExtensions
{
    public class AndroidFileStream : IFileStream
    {
        private static readonly string SplitFlag = "!/assets/";
        private static readonly int SplitFlagLength = SplitFlag.Length;
        private static readonly AndroidJavaObject s_AssetManager = null;
        private static readonly IntPtr s_InternalReadMethodId = IntPtr.Zero;
        private static readonly jvalue[] s_InternalReadArgs = null;

        private AndroidJavaObject m_FileStream;
        private IntPtr m_FileStreamRawObject;

        static AndroidFileStream()
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unityPlayer == null)
            {
                throw new Exception("Unity player is invalid.");
            }

            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (currentActivity == null)
            {
                throw new Exception("Current activity is invalid.");
            }

            AndroidJavaObject assetManager = currentActivity.Call<AndroidJavaObject>("getAssets");
            if (assetManager == null)
            {
                throw new Exception("Asset manager is invalid.");
            }

            s_AssetManager = assetManager;

            IntPtr inputStreamClassPtr = AndroidJNI.FindClass("java/io/InputStream");
            s_InternalReadMethodId = AndroidJNIHelper.GetMethodID(inputStreamClassPtr, "read", "([BII)I");
            s_InternalReadArgs = new jvalue[3];

            AndroidJNI.DeleteLocalRef(inputStreamClassPtr);
            currentActivity.Dispose();
            unityPlayer.Dispose();
        }
        // public long Position
        // {
        //     get { return Length;}
        //     set { Seek(value, SeekOrigin.Begin); }
        // }
        /// <summary>
        /// 获取文件系统流长度。
        /// </summary>
        private long Length
        {
            get
            {
                return InternalAvailable();
            }
        }

        private AndroidFileStream()
        {
        }

        
        
        public static AndroidFileStream Create(string filePath)
        {
            AndroidFileStream androidFileStream = new AndroidFileStream();
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("Full path is invalid.");
            }
            int position = filePath.LastIndexOf(SplitFlag, StringComparison.Ordinal);
            if (position < 0)
            {
                throw new Exception("Can not find split flag in full path.");
            }

            string fileName = filePath.Substring(position + SplitFlagLength);
            androidFileStream.m_FileStream = androidFileStream.InternalOpen(fileName);
            if (androidFileStream.m_FileStream == null)
            {
                throw new Exception(Utility.Text.Format("Open file '{0}' from Android asset manager failure.", filePath));
            }

            androidFileStream.m_FileStreamRawObject = androidFileStream.m_FileStream.GetRawObject();
            return androidFileStream;
        }
   

        /// <summary>
        /// 定位文件系统流位置。
        /// </summary>
        /// <param name="offset">要定位的文件系统流位置的偏移。</param>
        /// <param name="origin">要定位的文件系统流位置的方式。</param>
        public void Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                Seek(Length + offset, SeekOrigin.Begin);
                return;
            }

            if (origin == SeekOrigin.Begin)
            {
                InternalReset();
            }

            while (offset > 0)
            {
                long skip = InternalSkip(offset);
                if (skip < 0)
                {
                    return;
                }

                offset -= skip;
            }
        }

        public long Read(byte[] buffer, int startIndex, int length)
        {
            byte[] result = null;
            int bytesRead = InternalRead(length, out result);
            Array.Copy(result, 0, buffer, startIndex, bytesRead);
            return bytesRead;
        }
        private int InternalAvailable()
        {
            return m_FileStream.Call<int>("available");
        }
        private AndroidJavaObject InternalOpen(string fileName)
        {
            return s_AssetManager.Call<AndroidJavaObject>("open", fileName);
        }
        
        private void InternalClose()
        {
            m_FileStream.Call("close");
        }

        private int InternalRead(int length, out byte[] result)
        {
#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            IntPtr resultPtr = AndroidJNI.NewByteArray(length);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            int offset = 0;
            int bytesLeft = length;
            while (bytesLeft > 0)
            {
                s_InternalReadArgs[0] = new jvalue() { l = resultPtr };
                s_InternalReadArgs[1] = new jvalue() { i = offset };
                s_InternalReadArgs[2] = new jvalue() { i = bytesLeft };
                int bytesRead = AndroidJNI.CallIntMethod(m_FileStreamRawObject, s_InternalReadMethodId, s_InternalReadArgs);
                if (bytesRead <= 0)
                {
                    break;
                }

                offset += bytesRead;
                bytesLeft -= bytesRead;
            }

#if UNITY_2019_2_OR_NEWER
#pragma warning disable CS0618
#endif
            result = AndroidJNI.FromByteArray(resultPtr);
#if UNITY_2019_2_OR_NEWER
#pragma warning restore CS0618
#endif
            AndroidJNI.DeleteLocalRef(resultPtr);
            return offset;
        }
        private void InternalReset()
        {
            m_FileStream.Call("reset");
        }

        private long InternalSkip(long offset)
        {
            return m_FileStream.Call<long>("skip", offset);
        }
      
        public void Dispose()
        {
            InternalClose();
            m_FileStream.Dispose();
        }
    }
}