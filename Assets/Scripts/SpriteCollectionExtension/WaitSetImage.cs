using System;
using UnityEditor;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace UGFExtensions.SpriteCollection
{
    [Serializable]
    public class WaitSetImage : ISetSpriteObject
    {
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private Image m_Image;

        public WaitSetImage(Image obj, string collection, string spriteName)
        {
            m_Image = obj;
            SpritePath = spriteName;
            CollectionPath = collection;
            int index1 = SpritePath.LastIndexOf("/", StringComparison.Ordinal);
            int index2 = SpritePath.LastIndexOf(".", StringComparison.Ordinal);
            SpriteName = index2 < index1
                ? SpritePath.Substring(index1 + 1)
                : SpritePath.Substring(index1 + 1, index2 - index1 - 1);
        }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public string SpritePath { get; }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public string CollectionPath { get; }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private string SpriteName { get; }

        public void SetSprite(Sprite sprite)
        {
            m_Image.sprite = sprite;
        }

        public bool IsCanRelease()
        {
            return m_Image == null || m_Image.sprite == null || m_Image.sprite.name != SpriteName;
        }
#if !ODIN_INSPECTOR && UNITY_EDITOR
        public Rect DrawSetSpriteObject(Rect rect)
        {
            EditorGUI.ObjectField(rect, "Image", m_Image, typeof(Image), true);
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.TextField(rect, "CollectionPath", CollectionPath);
            rect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.TextField(rect, "SpritePath", SpritePath);
            rect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.TextField(rect, "CollectionPath", CollectionPath);
            rect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.TextField(rect, "SpriteName", SpriteName);
            rect.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.Toggle(rect, "IsCanRelease", IsCanRelease());
            return rect;
        }
#endif
    }
}