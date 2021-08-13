using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGFExtensions.Editor
{
    [CustomPropertyDrawer(typeof(StringSpriteDictionary))]
    public class StringSpriteDictionaryDrawer : PropertyDrawer
    {
        private List<Data> m_Sprites;
        private ReorderableList m_ReorderableList;
        private int m_PageIndex = 0;
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            //var rect = new Rect(position.x, position.y - 20, position.width, 18);
            SerializedProperty keys = property.FindPropertyRelative("m_Keys");
            SerializedProperty values = property.FindPropertyRelative("m_Values");
            int count = Math.Min(keys.arraySize, values.arraySize);
            m_Sprites = new List<Data>(count);
            for (int i = 0; i < count; i++)
            {
                m_Sprites.Add(new Data(keys.GetArrayElementAtIndex(i).stringValue,
                    values.GetArrayElementAtIndex(i).objectReferenceValue as Sprite));
            }
            // if (m_Sprites == null)
            // { 
            //   
            // }
          
            if (m_ReorderableList == null)
            {
                m_ReorderableList =
                    new ReorderableList(m_Sprites, typeof(List<Data>), true, false, true, true);
            
                m_ReorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    int sCount = m_Sprites.Count-m_PageIndex * 10 ;
                    sCount = sCount > 10 ? 10 : sCount;
                    for (int i = 0; i < sCount; i++)
                    {
                        Data data = m_Sprites[m_PageIndex * 10+i];
                        var keyRect = new Rect(rect.x, rect.y + i*30, rect.width/2, 30);
                      
                        
                        EditorGUI.LabelField(keyRect,data.Key); 
                        var valueRect = new Rect(rect.x+rect.width/2, rect.y + i*30+3.5f, rect.width/2, 16);
                        values.GetArrayElementAtIndex(m_PageIndex * 10+i).objectReferenceValue =
                            EditorGUI.ObjectField(valueRect,data.Value,typeof(Sprite),false) as Sprite;
                    }
                };
                m_ReorderableList.displayAdd = false;
                m_ReorderableList.displayRemove = false;
                m_ReorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x+15, rect.y , rect.width/2, 18),"Path");
                    EditorGUI.LabelField(new Rect(rect.x+15 + rect.width / 2, rect.y, rect.width / 2, 18), "Sprite");
                };
            }
            m_ReorderableList.DoLayoutList();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // SerializedProperty keys = property.FindPropertyRelative("m_Keys");
            // int height = 0;
            // if (keys.arraySize>0)
            // {
            //     height = keys.arraySize > 10 ? 300 : keys.arraySize * 30;
            // }
            return 0;
        }
        
        [Serializable]
        private class Data
        {
            [SerializeField] private string m_Key;

            public Data(string key, Sprite value)
            {
                m_Key = key;
                m_Value = value;
            }

            public string Key
            {
                get => m_Key;
                set => m_Key = value;
            }

            public Sprite Value
            {
                get => m_Value;
                set => m_Value = value;
            }

            [SerializeField] private Sprite m_Value;
        }
    }
}