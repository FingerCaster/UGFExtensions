using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGFExtensions.SpriteCollection
{
    [CreateAssetMenu(fileName = "SpriteCollection", menuName = "UGFExtensions/SpriteCollection", order = 0)]
    public class SpriteCollection : SerializedScriptableObject
    {
        [OdinSerialize] [DictionaryDrawerSettings(KeyLabel = "Path", ValueLabel = "Sprite", IsReadOnly = true)]
        private Dictionary<string, Sprite> m_Sprites = new Dictionary<string, Sprite>();

        public Sprite GetSprite(string path)
        {
            m_Sprites.TryGetValue(path, out Sprite sprite);
            return sprite;
        }
#if UNITY_EDITOR
        [OdinSerialize]
        [OnValueChanged("OnListChange", includeChildren: true)]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = false,HideAddButton = true)]
        [AssetsOnly]
        private List<Object> m_Objects = new List<Object>();
        private void OnListChange()
        {
            for (int i = m_Objects.Count - 1; i >= 0; i--)
            {
                if (!ObjectFilter(m_Objects[i]))
                {
                    m_Objects.RemoveAt(i);
                }
            }

            m_Objects = m_Objects.Distinct().ToList();
            Pack();
        }

        [Button("Pack Preview", Expanded = false, ButtonHeight = 18)]
        [HorizontalGroup("Pack Preview", width: 82)]
        [PropertySpace(16)]
        public void Pack()
        {
            m_Sprites.Clear();
            for (int i = 0; i < m_Objects.Count; i++)
            {
                Object obj = m_Objects[i];
                string path = AssetDatabase.GetAssetPath(obj);
                if (obj is Sprite sp)
                {
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                    if (objects.Length == 2)
                    {
                        m_Sprites[path] = sp;;
                    }
                    else
                    {
                        string regularPath = Utility.Path.GetRegularPath(Path.Combine(path, sp.name));
                        m_Sprites[regularPath] = sp;
                    }
                }
                else if (obj is Texture2D)
                {
                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                    if (objects.Length == 2)
                    {
                        m_Sprites[path] = GetSprites(objects)[0];;
                    }
                    else
                    {
                        Sprite[] sprites = GetSprites(objects);
                        for (int j = 0; j < sprites.Length; j++)
                        {
                            string regularPath = Utility.Path.GetRegularPath(Path.Combine(path, sprites[j].name));
                            m_Sprites[regularPath] = sprites[j];
                        }
                    }
                }
                else if (obj is DefaultAsset && ProjectWindowUtil.IsFolder(obj.GetInstanceID()))
                {
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(_ => !_.EndsWith(".meta")).Select(Utility.Path.GetRegularPath).ToArray();
                    foreach (string file in files)
                    {
                        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(file);
                        if (objects.Length == 2)
                        {
                            m_Sprites[file] = GetSprites(objects)[0];
                        }
                        else
                        {
                            Sprite[] sprites = GetSprites(objects);
                            for (int j = 0; j < sprites.Length; j++)
                            {
                                string regularPath = Utility.Path.GetRegularPath(Path.Combine(file, sprites[j].name));
                                m_Sprites[regularPath] = sprites[j];
                            }
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }
        private bool ObjectFilter(Object o)
        {
            return o != null && (o is Sprite || o is Texture2D ||
                                 (o is DefaultAsset && ProjectWindowUtil.IsFolder(o.GetInstanceID())));
        }

        private Sprite[] GetSprites(Object[] objects)
        {
            return objects.OfType<Sprite>().ToArray();
        }
#endif
    }
}