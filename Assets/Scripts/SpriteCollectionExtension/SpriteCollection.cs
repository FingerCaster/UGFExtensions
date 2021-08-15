using System;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

namespace UGFExtensions
{
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "UGFExtensions/SpriteCollection", order = 0)]
    public class SpriteCollection : ScriptableObject
    {
        [SerializeField] private StringSpriteDictionary m_Sprites;
        [SerializeField] private List<Object> m_Objects;

        // public IDictionary<string, string> Sprites
        // {
        //     get => m_Sprites;
        //     set => m_Sprites = value;
        // }
        public IDictionary<string, Sprite> Sprites
        {
            get { return m_Sprites; }
            set { m_Sprites.CopyFrom (value); }
        }
        
    }
}