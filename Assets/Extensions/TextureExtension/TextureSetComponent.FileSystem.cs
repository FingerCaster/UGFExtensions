using System;
using System.IO;
using System.Net;
using GameFramework;
using GameFramework.FileSystem;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Texture
{
    public partial class TextureSetComponent
    {
                
        /// <summary>
        /// 文件系统组件
        /// </summary>
        private FileSystemComponent m_FileSystemComponent;
      
        /// <summary>
        /// 图片文件系统
        /// </summary>
        private IFileSystem m_TextureFileSystem;

        private const int DefaultMaxDownloadBytes = 8 * 1024 * 1024;

        [SerializeField] private string[] m_AllowedNetworkTextureHosts = new string[0];

        /// <summary>
        /// 文件系统全路径
        /// </summary>
        private string m_FullPath;

        /// <summary>
        /// 图片加载缓存
        /// </summary>
        private byte[] m_Buffer;
        
        /// <summary>
        /// 文件系统最大文件数量
        /// </summary>
        [SerializeField] private int m_FileSystemMaxFileLength = 64;

        /// <summary>
        /// 初始化Buffer长度
        /// </summary>
        [SerializeField] private int m_InitBufferLength = 1024 * 64;

        /// <summary>
        /// 网络图片最大下载字节数。
        /// </summary>
        [SerializeField] private int m_MaxDownloadBytes = DefaultMaxDownloadBytes;

        private void InitializedFileSystem()
        {
            SettingComponent settingComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            m_FileSystemComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            m_Buffer = new byte[m_InitBufferLength];
            string fileName = settingComponent.GetString("TextureFileSystemFullPath", "TextureFileSystem");
            m_FullPath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, $"{fileName}.dat"));
            if (File.Exists(m_FullPath))
            {
                m_TextureFileSystem = m_FileSystemComponent.LoadFileSystem(m_FullPath, FileSystemAccess.ReadWrite);
            }
        }

        /// <summary>
        /// 从文件系统加载图片
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        private Texture2D GetTextureFromFileSystem(string file)
        {
            if (m_TextureFileSystem == null || !IsValidFileKey(file))
            {
                return null;
            }

            bool hasFile = m_TextureFileSystem.HasFile(file);
            if (!hasFile) return null;
            CheckBuffer(file);
            int byteRead = m_TextureFileSystem.ReadFile(file, m_Buffer);
            if (byteRead <= 0 || byteRead > GetMaxDownloadBytes())
            {
                return null;
            }

            byte[] bytes = new byte[byteRead];
            Array.Copy(m_Buffer, bytes, byteRead);
            if (!TryLoadTextureFromBytes(bytes, out Texture2D tex))
            {
                return null;
            }

            return tex;
        }

        /// <summary>
        /// 通过文件系统设置图片
        /// </summary>
        /// <param name="setTexture2dObject">需要设置图片的对象</param>
        public void SetTextureByFileSystem(ISetTexture2dObject setTexture2dObject)
        {
            if (setTexture2dObject == null || !IsValidFileKey(setTexture2dObject.Texture2dFilePath))
            {
                return;
            }

            Texture2D texture;
            if (m_TexturePool.CanSpawn(setTexture2dObject.Texture2dFilePath))
            {
                texture = (Texture2D)m_TexturePool.Spawn(setTexture2dObject.Texture2dFilePath).Target;
            }
            else
            {
                texture = GetTextureFromFileSystem(setTexture2dObject.Texture2dFilePath);
                if (texture != null)
                {
                    m_TexturePool.Register(TextureItemObject.Create(setTexture2dObject.Texture2dFilePath, texture, TextureLoad.FromFileSystem), true);
                }
            }

            if (texture != null)
            {
                SetTexture(setTexture2dObject, texture);
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
                if (byteRead <= 0 || byteRead > GetMaxDownloadBytes())
                {
                    continue;
                }
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
            if (!IsValidFileKey(file) || texture == null)
            {
                return false;
            }
            CheckFileSystem();
            byte[] bytes = texture.EncodeToPNG();
            return m_TextureFileSystem.WriteFile(file, bytes);
        }

        /// <summary>检查是否存在指定文件。</summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public bool HasFile(string file)
        {
            return m_TextureFileSystem != null && IsValidFileKey(file) && m_TextureFileSystem.HasFile(file);
        }

        /// <summary>删除指定的文件。</summary>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public bool DeleteFile(string file)
        {
            return m_TextureFileSystem == null || (IsValidFileKey(file) && m_TextureFileSystem.DeleteFile(file));
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="file">保存路径</param>
        /// <param name="texture">图片byte数组</param>
        /// <returns></returns>
        public bool SaveTexture(string file, byte[] texture)
        {
            if (!IsValidFileKey(file) || !IsValidDownloadBytes(texture))
            {
                return false;
            }
            CheckFileSystem();
            return m_TextureFileSystem.WriteFile(file, texture);
        }

        private bool IsValidFileKey(string file)
        {
            if (string.IsNullOrWhiteSpace(file) || Path.IsPathRooted(file) || file.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }

            string normalized = file.Replace('\\', '/');
            string[] segments = normalized.Split('/');
            foreach (string segment in segments)
            {
                if (string.IsNullOrEmpty(segment) || segment == "." || segment == "..")
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidDownloadBytes(byte[] bytes)
        {
            return bytes != null && bytes.Length > 0 && bytes.Length <= GetMaxDownloadBytes();
        }

        private int GetMaxDownloadBytes()
        {
            return m_MaxDownloadBytes > 0 ? m_MaxDownloadBytes : DefaultMaxDownloadBytes;
        }

        private bool TryLoadTextureFromBytes(byte[] bytes, out Texture2D texture)
        {
            texture = null;
            if (!IsValidDownloadBytes(bytes))
            {
                return false;
            }

            texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(bytes))
            {
                UnityEngine.Object.Destroy(texture);
                texture = null;
                return false;
            }

            return true;
        }

        private bool IsValidUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                return false;
            }

            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri))
            {
                return false;
            }

            if (!parsedUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(parsedUri.UserInfo) ||
                !string.IsNullOrEmpty(parsedUri.Query) ||
                !string.IsNullOrEmpty(parsedUri.Fragment))
            {
                return false;
            }

            string host = parsedUri.Host;
            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            if (!IsAllowedNetworkTextureHost(host))
            {
                return false;
            }

            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (IPAddress.TryParse(host, out IPAddress address))
            {
                return !IsPrivateOrLoopbackAddress(address);
            }

            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                if (addresses.Length == 0)
                {
                    return false;
                }

                foreach (IPAddress resolvedAddress in addresses)
                {
                    if (IsPrivateOrLoopbackAddress(resolvedAddress))
                    {
                        return false;
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }

            return true;
        }

        private bool IsAllowedNetworkTextureHost(string host)
        {
            if (m_AllowedNetworkTextureHosts == null || m_AllowedNetworkTextureHosts.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < m_AllowedNetworkTextureHosts.Length; i++)
            {
                if (host.Equals(m_AllowedNetworkTextureHosts[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetSafeUriForLog(string uri)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri))
            {
                return "<invalid>";
            }

            return parsedUri.GetLeftPart(UriPartial.Path);
        }

        private static bool IsPrivateOrLoopbackAddress(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            byte[] bytes = address.GetAddressBytes();
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return bytes[0] == 10 ||
                       bytes[0] == 0 ||
                       bytes[0] == 127 ||
                       bytes[0] == 192 && bytes[1] == 168 ||
                       bytes[0] == 169 && bytes[1] == 254 ||
                       bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31;
            }

            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal ||
                       address.Equals(IPAddress.IPv6Loopback) ||
                       address.Equals(IPAddress.IPv6None) ||
                       address.Equals(IPAddress.IPv6Any) ||
                       address.IsIPv4MappedToIPv6 && IsPrivateOrLoopbackAddress(address.MapToIPv4()) ||
                       bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80 ||
                       bytes[0] == 0xfc || bytes[0] == 0xfd;
            }

            return false;
        }
    }
}
