using System;
using System.Collections.Generic;

using UnityEngine;

namespace UGFExtensions
{
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "LegacySprite/SpriteCollection", order = 0)]
    public class SpriteCollection : ScriptableObject
    {
        [SerializeField] private Sprite m_Sprite;
        [SerializeField] private StringSpriteDictionary m_Sprites = new StringSpriteDictionary();
        public Sprite GetSprite(string spriteName)
        {
            m_Sprites.Sprites.TryGetValue(spriteName, out Sprite value);
            return value;
        }
#if UNITY_EDITOR
        public void AddSprite(string spritePath,Sprite sprite)
        {
            if (m_Sprites.Sprites.ContainsKey(spritePath))
            {
                Debug.LogError($"Sprite:{spritePath} is exist");
                return;
            }
            m_Sprites.Add(spritePath,sprite);
        }

        public void Clear()
        {
            m_Sprites.Sprites.Clear();
            m_Sprites.Keys.Clear();
            m_Sprites.Values.Clear();
        }
#endif
    }
    [Serializable]
    public class StringSpriteDictionary
    {
        [SerializeField]
        private List<string> m_Keys;
        [SerializeField]
        private List<Sprite> m_Values;

        private Dictionary<string, Sprite> m_Sprites;

        public List<string> Keys
        {
            get => m_Keys;
            set => m_Keys = value;
        }

        public List<Sprite> Values
        {
            get => m_Values;
            set => m_Values = value;
        }

        public Dictionary<string, Sprite> Sprites
        {
            get => m_Sprites;
            set => m_Sprites = value;
        }

        public StringSpriteDictionary()
        {
            m_Keys = new List<string>();
            m_Values = new List<Sprite>();
            m_Sprites = new Dictionary<string, Sprite>();
        }

        public void Add(string path,Sprite sprite)
        {
            m_Sprites.Add(path,sprite);
            m_Keys.Add(path);
            m_Values.Add(sprite);
        }
    }
}