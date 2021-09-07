using System;
using System.IO;
using GameFramework;
using GameFramework.FileSystem;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    public class FileTextureComponent : GameFrameworkComponent
    {
        private IFileSystem m_TextureFileSystem;
        private string m_FullPath;
        private byte[] m_Buffer = new byte[1024 * 64];
        
        private async void Start()
        {
            FileSystemComponent fileSystemComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
            SettingComponent settingComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
            await GameEntry.Timer.FrameAsync();
            string fileName = settingComponent.GetString("TextureFileSystemFullPath", "FileTexture");
            m_FullPath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath,$"{fileName}.dat"));
            if (File.Exists(m_FullPath))
            {
                m_TextureFileSystem = fileSystemComponent.LoadFileSystem(m_FullPath, FileSystemAccess.ReadWrite);
            }
            else
            {
                m_TextureFileSystem = fileSystemComponent.CreateFileSystem(m_FullPath, FileSystemAccess.ReadWrite, 64, 512);
            }
        }

        public Texture2D GetTexture(string file)
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

        private void CheckFileSystem()
        {
            if (m_TextureFileSystem.FileCount>=m_TextureFileSystem.MaxFileCount)
            {
                FileSystemComponent fileSystemComponent =
                    UnityGameFramework.Runtime.GameEntry.GetComponent<FileSystemComponent>();
                SettingComponent settingComponent =
                    UnityGameFramework.Runtime.GameEntry.GetComponent<SettingComponent>();
                string fileName = settingComponent.GetString("TextureFileSystemFullPath","FileTexture");
                fileName = fileName!= "FileTexture" ? "FileTexture" : "FileTextureNew";
                m_FullPath = Path.Combine(Application.persistentDataPath,$"{fileName}.dat");
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
        }

        public bool SaveTexture(string file, Texture2D texture)
        {
            CheckFileSystem();
            byte[] bytes = texture.EncodeToPNG();
            return m_TextureFileSystem.WriteFile(file, bytes);
        }

        public bool SaveTexture(string file, byte[] texture)
        {
            CheckFileSystem();
            return m_TextureFileSystem.WriteFile(file, texture);
        }
    }
}