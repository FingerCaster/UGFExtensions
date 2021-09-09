using System;
using GameFramework;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace UGFExtensions.Texture
{
    [Serializable]
    public class SetRawImage : ISetTexture2dObject
    {
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private RawImage m_RawImage;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private string TextureName { get; set; }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public string Texture2dFilePath { get; private set; }

        public void SetTexture(Texture2D texture)
        {
            m_RawImage.texture = texture;
        }

        public bool IsCanRelease()
        {
            return m_RawImage == null || m_RawImage.texture == null || m_RawImage.texture.name != TextureName;
        }

        public static SetRawImage Create(RawImage rawImage, string filePath)
        {
            SetRawImage item = ReferencePool.Acquire<SetRawImage>();
            item.m_RawImage = rawImage;
            item.Texture2dFilePath = filePath;
            int index1 = filePath.LastIndexOf("/", StringComparison.Ordinal);
            int index2 = filePath.LastIndexOf(".", StringComparison.Ordinal);
            item.TextureName = index2 < index1
                ? filePath.Substring(index1 + 1)
                : filePath.Substring(index1 + 1, index2 - index1 - 1);
            return item;
        }

        public void Clear()
        {
            m_RawImage = null;
            TextureName = null;
            Texture2dFilePath = null;
        }
    }
}