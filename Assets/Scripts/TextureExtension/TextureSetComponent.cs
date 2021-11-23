using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using GameFramework.Event;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace UGFExtensions.Texture
{
    public partial class TextureSetComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 文件系统最大文件数量
        /// </summary>
        [SerializeField] private int m_FileSystemMaxFileLength = 64;

        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        [SerializeField] private int m_InitBufferLength = 1024 * 64;

        /// <summary>
        /// 检查是否可以释放间隔
        /// </summary>
        [SerializeField] private float m_CheckCanReleaseInterval = 30;

        private float m_CheckCanReleaseTime = 0.0f;

        /// <summary>
        /// 对象池自动释放时间间隔
        /// </summary>
        [SerializeField] private float m_AutoReleaseInterval = 30;

        /// <summary>
        /// 图片文件系统
        /// </summary>
        private IFileSystem m_TextureFileSystem;

        /// <summary>
        /// 文件系统全路径
        /// </summary>
        private string m_FullPath;

        /// <summary>
        /// 图片加载缓存
        /// </summary>
        private byte[] m_Buffer;

        /// <summary>
        /// 保存加载的图片对象
        /// </summary>
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private LinkedList<LoadTextureObject> m_LoadTextureObjectsLinkedList;

        /// <summary>
        /// 散图集合对象池
        /// </summary>
        private IObjectPool<TextureItemObject> m_TexturePool;

        
        /// <summary>
        /// 文件系统组件
        /// </summary>
        private FileSystemComponent m_FileSystemComponent;
        /// <summary>
        /// 资源组件
        /// </summary>
        private ResourceComponent m_ResourceComponent;
        private WebRequestComponent m_WebRequestComponent;

#if UNITY_EDITOR
        public LinkedList<LoadTextureObject> LoadTextureObjectsLinkedList
        {
            get => m_LoadTextureObjectsLinkedList;
            set => m_LoadTextureObjectsLinkedList = value;
        }
#endif
        private IEnumerator Start()
        {
            m_FileSystemComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            SettingComponent settingComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            ObjectPoolComponent objectPoolComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            EventComponent eventComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
            WebRequestComponent webRequestComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<WebRequestComponent>();
            m_ResourceComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<ResourceComponent>();
            yield return new WaitForEndOfFrame();
            eventComponent.Subscribe(WebRequestSuccessEventArgs.EventId,OnWebGetTextureSuccess);
            eventComponent.Subscribe(WebRequestFailureEventArgs.EventId,OnWebGetTextureFailure);
                m_Buffer = new byte[m_InitBufferLength];
            string fileName = settingComponent.GetString("TextureFileSystemFullPath", "TextureFileSystem");
            m_FullPath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, $"{fileName}.dat"));
            if (File.Exists(m_FullPath))
            {
                m_TextureFileSystem = m_FileSystemComponent.LoadFileSystem(m_FullPath, FileSystemAccess.ReadWrite);
            }

            m_TexturePool = objectPoolComponent.CreateMultiSpawnObjectPool<TextureItemObject>(
                "TexturePool",
                m_AutoReleaseInterval, 16, 60, 0);

            m_LoadTextureObjectsLinkedList = new LinkedList<LoadTextureObject>();
        }

        private void OnWebGetTextureFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs webRequestSuccessEventArgs = (WebRequestFailureEventArgs)e;
            WebGetTextureData webGetTextureData = webRequestSuccessEventArgs.UserData as WebGetTextureData;
            if (webGetTextureData == null || webGetTextureData.UserData != this)
            {
                return;
            }
            Log.Error("Can not download Texture2D from '{1}' with error message '{2}'.",webRequestSuccessEventArgs.WebRequestUri,webRequestSuccessEventArgs.ErrorMessage);
            ReferencePool.Release(webGetTextureData);
        }

        private void OnWebGetTextureSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs webRequestSuccessEventArgs = (WebRequestSuccessEventArgs)e;
            WebGetTextureData webGetTextureData = webRequestSuccessEventArgs.UserData as WebGetTextureData;
            if (webGetTextureData == null || webGetTextureData.UserData != this)
            {
                return;
            }
            Texture2D tex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            var bytes = webRequestSuccessEventArgs.GetWebResponseBytes();
            tex.LoadImage(bytes);
            if (!string.IsNullOrEmpty(webGetTextureData.FilePath))
            {
                SaveTexture(webGetTextureData.FilePath, bytes);
            }
            m_TexturePool.Register(TextureItemObject.Create(webGetTextureData.SetTexture2dObject.Texture2dFilePath, tex, TextureLoad.FromNet), true);
            SetTexture(webGetTextureData.SetTexture2dObject, tex);
            ReferencePool.Release(webGetTextureData);
        }
        
      

        private void Update()
        {
            m_CheckCanReleaseTime += Time.unscaledDeltaTime;
            if (m_CheckCanReleaseTime < (double)m_AutoReleaseInterval)
                return;
            ReleaseUnused();
        }

        /// <summary>
        /// 回收无引用的Texture。
        /// </summary>
#if ODIN_INSPECTOR
        [Button("Release Unused")]
#endif
        public void ReleaseUnused()
        {
            LinkedListNode<LoadTextureObject> current = m_LoadTextureObjectsLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.Texture2dObject.IsCanRelease())
                {
                    m_TexturePool.Unspawn(current.Value.Texture2D);
                    ReferencePool.Release(current.Value.Texture2dObject);
                    m_LoadTextureObjectsLinkedList.Remove(current);
                }

                current = next;
            }

            m_CheckCanReleaseTime = 0f;
        }
        /// <summary>
        /// 从文件系统加载图片
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        private Texture2D GetTextureFromFileSystem(string file)
        {
            if (m_TextureFileSystem == null)
            {
                return null;
            }
            bool hasFile = m_TextureFileSystem.HasFile(file);
            if (!hasFile) return null;
            CheckBuffer(file);
            int byteRead = m_TextureFileSystem.ReadFile(file, m_Buffer);
            Debug.Log(byteRead);
            Texture2D tex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            byte[] bytes = new byte[byteRead];
            Array.Copy(m_Buffer, bytes, byteRead);
            tex.LoadImage(bytes);
            return tex;
        }

        /// <summary>
        /// 从网络加载图片
        /// </summary>
        /// <param name="fileUrl">图片网络路径</param>
        /// <param name="filePath">保存图片到文件系统的路径</param>
        /// <returns></returns>
        private void GetTextureFromNetwork(string fileUrl, string filePath,ISetTexture2dObject setTexture2dObject)
        {
            m_WebRequestComponent.AddWebRequest(fileUrl, WebGetTextureData.Create(setTexture2dObject,this,filePath));
        }

        /// <summary>
        /// 从资源系统加载图片
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        private void GetTextureFromResource(string assetPath,ISetTexture2dObject setTexture2dObject)
        {
            m_ResourceComponent.LoadAsset(assetPath, typeof(Texture2D), new LoadAssetCallbacks(
                (tempAssetName, asset, duration, userdata) =>
                {
                    Texture2D texture =  asset as Texture2D;
                    if (texture != null)
                    {
                        m_TexturePool.Register(TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture, TextureLoad.FromNet), true);
                        SetTexture(setTexture2dObject,asset as Texture2D);
                    }
                    else
                    {
                        Log.Error(new GameFrameworkException($"Load Texture2D failure asset type is {asset.GetType()}."));
                    }
                },
                (tempAssetName, status, errorMessage, userdata) =>
                {
                    Log.Error("Can not load Texture2D from '{1}' with error message '{2}'.",tempAssetName,errorMessage);
                }
            ));
        }

        /// <summary>
        /// 通过文件系统设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        public void SetTextureByFileSystem(ISetTexture2dObject setTexture2dObject)
        {
            Texture2D texture;
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
            }
            else
            {
                texture = GetTextureFromFileSystem(setTexture2dObject.Texture2dFilePath);
                m_TexturePool.Register(TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture, TextureLoad.FromFileSystem), true);
            }

            if (texture != null)
            {
                SetTexture(setTexture2dObject, texture);
            }
        }

        /// <summary>
        /// 通过网络设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        /// <param name="saveFilePath">保存网络图片到本地的路径</param>
        public void SetTextureByNetwork(ISetTexture2dObject setTexture2dObject, string saveFilePath = null)
        {
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                var texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
                SetTexture(setTexture2dObject, texture);
            }
            else
            {
                GetTextureFromNetwork(setTexture2dObject.Texture2dFilePath, saveFilePath,setTexture2dObject);
            }
        }

        /// <summary>
        /// 通过资源系统设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        public void SetTextureByResources(ISetTexture2dObject setTexture2dObject)
        {
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                var texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
                SetTexture(setTexture2dObject, texture);
            }
            else
            {
                GetTextureFromResource(setTexture2dObject.Texture2dFilePath,setTexture2dObject);
            }
        }

        private void SetTexture(ISetTexture2dObject setTexture2dObject,Texture2D texture)
        {
            m_LoadTextureObjectsLinkedList.AddLast(new LoadTextureObject(setTexture2dObject, texture));
            setTexture2dObject.SetTexture(texture);
        }

        /// <summary>
        /// 检查加载图片缓存大小(不足自动扩容为原来的2倍)
        /// </summary>
        /// <param name="file">当前读取的文件</param>
        private void CheckBuffer(string file)
        {
            var fileInfo = m_TextureFileSystem.GetFileInfo(file);
            if (m_Buffer.Length < fileInfo.Length)
            {
                int length = m_Buffer.Length * 2;
                while (length < fileInfo.Length)
                {
                    length *= 2;
                }

                m_Buffer = new byte[length];
            }
        }

        /// <summary>
        /// 检查文件系统大小(不足自动扩容为原来的2倍)
        /// </summary>
        private void CheckFileSystem()
        {
            if (m_TextureFileSystem == null)
            {
                m_TextureFileSystem = m_FileSystemComponent.CreateFileSystem(m_FullPath, FileSystemAccess.ReadWrite,
                    m_FileSystemMaxFileLength, m_FileSystemMaxFileLength * 8);
            }

            if (m_TextureFileSystem.FileCount < m_TextureFileSystem.MaxFileCount) return;
            FileSystemComponent fileSystemComponent =
                UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            SettingComponent settingComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            string fileName = settingComponent.GetString("TextureFileSystemFullPath", "TextureFileSystem");
            fileName = fileName != "TextureFileSystem" ? "TextureFileSystem" : "TextureFileSystemNew";
            m_FullPath = Path.Combine(Application.persistentDataPath, $"{fileName}.dat");
            settingComponent.SetString("TextureFileSystemFullPath", fileName);
            settingComponent.Save();
            IFileSystem newFileSystem = fileSystemComponent.CreateFileSystem(m_FullPath, FileSystemAccess.ReadWrite,
                m_TextureFileSystem.MaxFileCount * 2, m_TextureFileSystem.MaxFileCount * 16);
            var fileInfos = m_TextureFileSystem.GetAllFileInfos();

            foreach (var fileInfo in fileInfos)
            {
                CheckBuffer(fileInfo.Name);
                int byteRead = m_TextureFileSystem.ReadFile(fileInfo.Name, m_Buffer);
                byte[] bytes = new byte[byteRead];
                Array.Copy(m_Buffer, bytes, byteRead);
                newFileSystem.WriteFile(fileInfo.Name, bytes);
            }

            fileSystemComponent.DestroyFileSystem(m_TextureFileSystem, true);
            m_TextureFileSystem = newFileSystem;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="file">保存路径</param>
        /// <param name="texture">图片</param>
        /// <returns></returns>
        public bool SaveTexture(string file, Texture2D texture)
        {
            CheckFileSystem();
            byte[] bytes = texture.EncodeToPNG();
            return m_TextureFileSystem.WriteFile(file, bytes);
        }

        /// <summary>检查是否存在指定文件。</summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public bool HasFile(string file)
        {
            return m_TextureFileSystem.HasFile(file);
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="file">保存路径</param>
        /// <param name="texture">图片byte数组</param>
        /// <returns></returns>
        public bool SaveTexture(string file, byte[] texture)
        {
            CheckFileSystem();
            return m_TextureFileSystem.WriteFile(file, texture);
        }
    }
}