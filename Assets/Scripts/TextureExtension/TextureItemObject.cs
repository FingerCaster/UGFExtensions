using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace UGFExtensions.Texture
{
    public class TextureItemObject : ObjectBase
    {
        private TextureLoad m_TextureLoad;
        public static TextureItemObject Create(string collectionPath, UnityEngine.Texture target,TextureLoad textureLoad)
        {
            TextureItemObject item = ReferencePool.Acquire<TextureItemObject>();
            item.Initialize(collectionPath, target);
            item.m_TextureLoad = textureLoad;
            return item;
        }

        protected override void Release(bool isShutdown)
        {
            UnityEngine.Texture texture = (UnityEngine.Texture)Target;
            if (texture == null)
            {
                return;
            }

            switch (m_TextureLoad)
            {
                case TextureLoad.FromResource:
                    GameEntry.Resource.UnloadAsset(texture);
                    break;
                case TextureLoad.FromNet:
                case TextureLoad.FromFileSystem:
                    Object.Destroy(texture);
                    break;
            }
        }
    }
    
           
    public enum TextureLoad
    {
        /// <summary>
        /// 从文件系统
        /// </summary>
        FromFileSystem,
        /// <summary>
        /// 从网络
        /// </summary>
        FromNet,
        /// <summary>
        /// 从资源包
        /// </summary>
        FromResource
    }
}