using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using UnityEditor;
using UnityEngine;

namespace UGFExtensions.Editor
{
    [CustomEditor(typeof(SpriteCollection))]
    public class SpriteCollectionEditor : UnityEditor.Editor
    {
        private SerializedProperty m_Sprites;
        private Sprite m_Sprite;
        private SpriteCollection m_SpriteCollection;

        private void OnEnable()
        {
            m_SpriteCollection = target as SpriteCollection;
            m_Sprites = serializedObject.FindProperty("m_Sprites");
            // m_Sprite = serializedObject.FindProperty("m_Sprite");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Sprites, true);
            // EditorGUILayout.ObjectField(m_Sprite,typeof(Sprite),false);
            if (GUILayout.Button("Collect Sprites"))
            {
                string[] imageType = 
                {
                    "*.JPG",
                    "*.PNG"
                };
                for (int i = 0; i < imageType.Length; i++) {
                    string[] dirs = Directory.GetFiles (Application.dataPath, imageType [i],SearchOption.AllDirectories);  
                    
                    for (int j = 0; j < dirs.Length; j++)
                    {
                        string path = Utility.Path.GetRegularPath(dirs[j]);
                        int index = path.IndexOf(@"Assets/", StringComparison.Ordinal);
                        path =path.Substring(index);
                        m_SpriteCollection.AddSprite(path,AssetDatabase.LoadAssetAtPath<Sprite>(path));
                    }  
                }
                
            }

            if (GUILayout.Button("Clear All"))
            {
                m_SpriteCollection.Clear();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}