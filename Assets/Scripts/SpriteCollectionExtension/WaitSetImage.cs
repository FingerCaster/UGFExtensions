using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace UGFExtensions.SpriteCollection
{
    [Serializable]
    public class WaitSetImage : ISetSpriteObject
    {
        [ShowInInspector] private Image m_Image;
        public WaitSetImage(Image obj, string collection, string spriteName)
        {
            m_Image = obj;
            SpritePath = spriteName;
            CollectionPath = collection;
            int index1 = SpritePath.LastIndexOf("/", StringComparison.Ordinal);
            int index2 = SpritePath.LastIndexOf(".", StringComparison.Ordinal);
            SpriteName = index2<index1 ? SpritePath.Substring(index1+1) : SpritePath.Substring(index1+1,index2-index1-1);

        }
        [ShowInInspector] public string SpritePath { get; }
        [ShowInInspector]  public string CollectionPath { get; }
        [ShowInInspector]  private string SpriteName { get; }

        public void SetSprite(Sprite sprite)
        {
            m_Image.sprite = sprite;
        }

        public bool IsCanRelease()
        {
            return m_Image == null || m_Image.sprite == null || m_Image.sprite.name != SpriteName;
        }
    }
}