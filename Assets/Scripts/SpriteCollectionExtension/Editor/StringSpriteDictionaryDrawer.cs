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
           // int tempCount = count - m_PageIndex * 10;
           if (count == 0)
           {
               m_ReorderableList = null;
           }
           else
           {
               
               if (m_ReorderableList == null)
               {
                   DrawDictionary(keys, values);
               }

               m_ReorderableList.DoLayoutList();
               position.y = m_ReorderableList.GetHeight()+45;
               position.width = position.width/2;
               position.height += 40;
               if (GUI.Button(position, "上一页"))
               {
                   --m_PageIndex;
                   m_PageIndex = m_PageIndex < 0 ? 0 : m_PageIndex;
                   DrawDictionary(keys, values);
               }
               position.x +=  position.width ;
               if (GUI.Button(position, "下一页"))
               {
                   ++m_PageIndex;
                   int pages = count / 10 + ((count % 10) > 0 ? 1 : 0);
                   m_PageIndex = m_PageIndex < pages  ? m_PageIndex : pages-1;
                   m_PageIndex = m_PageIndex < 0 ? 0 : m_PageIndex;
                   DrawDictionary(keys, values);
               }
           }


            EditorGUILayout.GetControlRect(false, position.height);
        }

        private void DrawDictionary(SerializedProperty keys,SerializedProperty values)
        {
            int count = Math.Min(keys.arraySize, values.arraySize);
            int tempCount = count - m_PageIndex * 10;
            tempCount = tempCount > 10 ? 10 : Mathf.Abs(tempCount);
            m_Sprites = new List<Data>(tempCount);
            int spIndex = m_PageIndex * 10 > 0?m_PageIndex * 10 -1:0;
       
            for (int i = 0;i < tempCount; i++)
            {
                m_Sprites.Add(new Data(keys.GetArrayElementAtIndex(spIndex+i).stringValue,
                    values.GetArrayElementAtIndex(spIndex+i).objectReferenceValue as Sprite));
            }
            m_ReorderableList = new ReorderableList(m_Sprites, typeof(List<Data>), true, false, true, true);

            if (m_Sprites.Count>0)
            {
                m_ReorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    Data data = m_Sprites[index];
                    rect.width /= 2;
                    EditorGUI.LabelField(rect, data.Key);
                    rect.x += rect.width;
                    rect.height = 16;
                    values.GetArrayElementAtIndex(spIndex+index).objectReferenceValue =
                        EditorGUI.ObjectField(rect, data.Value, typeof(Sprite), false) as Sprite;
                };
                m_ReorderableList.displayAdd = false;
                m_ReorderableList.displayRemove = false;
            }
         
            m_ReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(new Rect(rect.x + 15, rect.y, rect.width / 2, 18), "Path");
                EditorGUI.LabelField(new Rect(rect.x + 15 + rect.width / 2, rect.y, rect.width / 2, 18), "Sprite");
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
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