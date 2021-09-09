using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GameFramework;
using GameFramework.FileSystem;
using GameFramework.ObjectPool;
using UGFExtensions.Await;
using UGFExtensions.Timer;
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
        /// 自动释放时间间隔
        /// </summary>
        [SerializeField] private int m_AutoReleaseInterval = 60;
        
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

        private async void Start()
        {
            FileSystemComponent fileSystemComponent =
                UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            SettingComponent settingComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            TimerComponent timerComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
            ObjectPoolComponent objectPoolComponent =
                UnityGameFramework.Runtime.GameEntry.GetComponent<ObjectPoolComponent>();
            await GameEntry.Timer.FrameAsync();
            timerComponent.AddRepeatedTimer(m_AutoReleaseInterval * 1000, -1, ReleaseUnused);
            m_Buffer = new byte[m_InitBufferLength];
            string fileName = settingComponent.GetString("TextureFileSystemFullPath", "TextureFileSystem");
            m_FullPath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, $"{fileName}.dat"));
            m_TextureFileSystem = File.Exists(m_FullPath)
                ? fileSystemComponent.LoadFileSystem(m_FullPath, FileSystemAccess.ReadWrite)
                : fileSystemComponent.CreateFileSystem(m_FullPath, FileSystemAccess.ReadWrite, m_FileSystemMaxFileLength, m_FileSystemMaxFileLength*8);
            m_TexturePool = objectPoolComponent.CreateMultiSpawnObjectPool<TextureItemObject>(
                "TexturePool",
                60.0f, 16, 60, 0);
            m_LoadTextureObjectsLinkedList = new LinkedList<LoadTextureObject>();
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
        }

        /// <summary>
        /// 从文件系统加载图片
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        private Texture2D GetTextureFromFileSystem(string file)
        {
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
        private async Task<Texture2D> GetTextureFromNetwork(string fileUrl, string filePath)
        {
            var data = await GameEntry.WebRequest.AddWebRequestAsync(fileUrl);
            if (!data.IsError)
            {
                Texture2D tex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                tex.LoadImage(data.Bytes);
                if (!string.IsNullOrEmpty(filePath))
                {
                    SaveTexture(filePath, data.Bytes);
                }

                return tex;
            }

            return null;
        }

        /// <summary>
        /// 从资源系统加载图片
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        private async Task<Texture2D> GetTextureFromResource(string assetPath)
        {
            return await GameEntry.Resource.LoadAssetAsync<Texture2D>(assetPath);
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
                m_TexturePool.Register(TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture,TextureLoad.FromFileSystem), true);
            }

            if (texture != null)
            {
                setTexture2dObject.SetTexture(texture);
                m_LoadTextureObjectsLinkedList.AddLast(new LoadTextureObject(setTexture2dObject, texture));
            }
        }

        /// <summary>
        /// 通过网络设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        /// <param name="saveFilePath">保存网络图片到本地的路径</param>
        public async void SetTextureByNetwork(ISetTexture2dObject setTexture2dObject, string saveFilePath = null)
        {
            Texture2D texture;
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
            }
            else
            {
                texture = await GetTextureFromNetwork(setTexture2dObject.Texture2dFilePath, saveFilePath);
            }

            if (texture != null)
            {
                m_TexturePool.Register(TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture,TextureLoad.FromNet), true);
                m_LoadTextureObjectsLinkedList.AddLast(new LoadTextureObject(setTexture2dObject, texture));
                setTexture2dObject.SetTexture(texture);
            }
        }

        /// <summary>
        /// 通过资源系统设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        public async void SetTextureByResources(ISetTexture2dObject setTexture2dObject)
        {
            Texture2D texture;
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
            }
            else
            {
                texture = await GetTextureFromResource(setTexture2dObject.Texture2dFilePath);
            }

            if (texture != null)
            {
                m_TexturePool.Register(TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture,TextureLoad.FromResource), true);
                m_LoadTextureObjectsLinkedList.AddLast(new LoadTextureObject(setTexture2dObject, texture));
                setTexture2dObject.SetTexture(texture);
            }
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