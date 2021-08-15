using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameFramework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGFExtensions.Editor
{
    [CustomEditor(typeof(SpriteCollection))]
    public class SpriteCollectionEditor : UnityEditor.Editor
    {
        private SpriteCollection Target => target as SpriteCollection;
        private SerializedProperty m_Sprites;
        private SerializedProperty m_Objects;
        private SerializedProperty m_Keys;
        private SerializedProperty m_Values;
        private ReorderableList m_ReorderableList;
        private readonly int m_SelectorHash = "ObjectSelector".GetHashCode();
        private bool m_PackableListExpanded = true;

        private void OnEnable()
        {
            m_Sprites = serializedObject.FindProperty("m_Sprites");
            m_Objects = serializedObject.FindProperty("m_Objects");
            m_Keys = m_Sprites.FindPropertyRelative("m_keys");
            m_Values = m_Sprites.FindPropertyRelative("m_values");
            m_ReorderableList = new ReorderableList(serializedObject, m_Objects, true, true, true, true)
            {
                onAddCallback = AddPackable,
                onRemoveCallback = RemovePackable,
                drawElementCallback = DrawPackableElement,
                elementHeight = EditorGUIUtility.singleLineHeight,
                headerHeight = 0f,
            };
        }

        void AddPackable(ReorderableList list)
        {
            EditorGUIUtility.ShowObjectPicker<Object>(null, false, "t:sprite t:texture2d t:folder", m_SelectorHash);
        }

        void RemovePackable(ReorderableList list)
        {
            var index = list.index;
            if (index != -1)
            {
                Object obj = m_Objects.GetArrayElementAtIndex(index).objectReferenceValue;
                string path = AssetDatabase.GetAssetPath(obj);
                if (obj is Sprite)
                {
                    RemoveDictionary(path);
                }
                else if (obj is Texture2D)
                {
                    var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                    if (objs.Length == 2)
                    {
                        RemoveDictionary(path);
                    }
                    else
                    {
                        for (int i = 1; i < objs.Length; i++)
                        {
                            if (objs[i] is Sprite)
                            {
                                string regularPath = Utility.Path.GetRegularPath(Path.Combine(path, objs[i].name));
                                RemoveDictionary(regularPath);
                            }
                        }
                    }
                }
                else if ((obj is DefaultAsset && ProjectWindowUtil.IsFolder(obj.GetInstanceID())))
                {
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(f => !f.EndsWith(".meta", StringComparison.Ordinal)).ToArray();
                    for (int i = 0; i < files.Length; i++)
                    {
                        var objs = AssetDatabase.LoadAllAssetsAtPath(files[i]);
                        if (objs.Length == 2)
                        {
                            string regularPath = Utility.Path.GetRegularPath(files[i]);
                            RemoveDictionary(regularPath);
                        }
                        else
                        {
                            for (int j = 1; j < objs.Length; j++)
                            {
                                if (objs[j] is Sprite)
                                {
                                    string regularPath =
                                        Utility.Path.GetRegularPath(Path.Combine(files[i], objs[j].name));
                                    RemoveDictionary(regularPath);
                                }
                            }
                        }
                    }
                }

                m_Objects.GetArrayElementAtIndex(index).objectReferenceValue = null;
                m_Objects.DeleteArrayElementAtIndex(index);
                // ReorderableList.defaultBehaviours.DoRemoveButton(list);
                // ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Sprites);
            GUI.enabled = true;

            HandlePackableListUI();
            DrawPackUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void HandlePackableListUI()
        {
            var currentEvent = Event.current;
            var usedEvent = false;
            Rect rect = EditorGUILayout.GetControlRect();
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (rect.Contains(currentEvent.mousePosition) && GUI.enabled)
                    {
                        // Check each single object, so we can add multiple objects in a single drag.
                        var didAcceptDrag = false;
                        var references = DragAndDrop.objectReferences;
                        foreach (var obj in references)
                        {
                            if (IsPackable(obj))
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                if (currentEvent.type == EventType.DragPerform)
                                {
                                    AddObject(obj);
                                    didAcceptDrag = true;
                                    DragAndDrop.activeControlID = 0;
                                }
                                else
                                    DragAndDrop.activeControlID = controlID;
                            }
                        }

                        if (didAcceptDrag)
                        {
                            GUI.changed = true;
                            DragAndDrop.AcceptDrag();
                            usedEvent = true;
                        }
                    }

                    break;
                case EventType.ExecuteCommand:
                    if (currentEvent.commandName == "ObjectSelectorClosed" &&
                        EditorGUIUtility.GetObjectPickerControlID() == m_SelectorHash)
                    {
                        var obj = EditorGUIUtility.GetObjectPickerObject();
                        if (IsPackable(obj))
                        {
                            AddObject(obj);
                            m_ReorderableList.index = m_Objects.arraySize - 1;
                        }
                    }

                    usedEvent = true;
                    break;
            }

            if (usedEvent)
                currentEvent.Use();
            m_PackableListExpanded = EditorGUI.Foldout(rect, m_PackableListExpanded,
                EditorGUIUtility.TrTextContent("Objects for Packing",
                    "Only accept Folder, Sprite Sheet(Texture) and Sprite."), true);

            if (m_PackableListExpanded)
            {
                EditorGUI.indentLevel++;
                m_ReorderableList.DoLayoutList();
                EditorGUI.indentLevel--;
            }
        }

        private void AddObject(Object obj)
        {
            m_Objects.InsertArrayElementAtIndex(m_Objects.arraySize);
            m_Objects.GetArrayElementAtIndex(m_Objects.arraySize - 1).objectReferenceValue = obj;
            AddSprite(obj);
        }

        private void AddSprite(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (obj is Sprite sprite)
            {
                AddToDictionary(path, sprite);
            }
            else if (obj is Texture2D)
            {
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                if (objs.Length == 2)
                {
                    AddToDictionary(path, objs[1] as Sprite);
                }
                else
                {
                    for (int i = 1; i < objs.Length; i++)
                    {
                        if (objs[i] is Sprite)
                        {
                            string regularPath = Utility.Path.GetRegularPath(Path.Combine(path, objs[i].name));
                            AddToDictionary(regularPath, objs[i] as Sprite);
                        }
                    }
                }
            }
            else if ((obj is DefaultAsset && ProjectWindowUtil.IsFolder(obj.GetInstanceID())))
            {
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".meta", StringComparison.Ordinal)).ToArray();
                for (int i = 0; i < files.Length; i++)
                {
                    var objs = AssetDatabase.LoadAllAssetsAtPath(files[i]);
                    if (objs.Length == 2)
                    {
                        string regularPath = Utility.Path.GetRegularPath(files[i]);
                        AddToDictionary(regularPath, objs[1] as Sprite);
                        AddToDictionary(regularPath, objs[1] as Sprite);
                    }
                    else
                    {
                        for (int j = 1; j < objs.Length; j++)
                        {
                            if (objs[j] is Sprite)
                            {
                                string regularPath = Utility.Path.GetRegularPath(Path.Combine(files[i], objs[j].name));
                                AddToDictionary(regularPath, objs[1] as Sprite);
                            }
                        }
                    }
                }
            }
        }

        private void AddToDictionary(string path, Sprite sprite)
        {
            m_Keys.InsertArrayElementAtIndex(m_Keys.arraySize);
            m_Keys.GetArrayElementAtIndex(m_Keys.arraySize - 1).stringValue = path;
            m_Values.InsertArrayElementAtIndex(m_Values.arraySize);
            m_Values.GetArrayElementAtIndex(m_Values.arraySize - 1).objectReferenceValue = sprite;
        }

        private void RemoveDictionary(string path)
        {
            int index = -1;
            for (int i = 0; i < m_Keys.arraySize; i++)
            {
                if (m_Keys.GetArrayElementAtIndex(i).stringValue == path)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                m_Keys.DeleteArrayElementAtIndex(index);
                m_Values.DeleteArrayElementAtIndex(index);
                Target.Sprites.Remove(path);
            }
        }

        void DrawPackableElement(Rect rect, int index, bool selected, bool focused)
        {
            var property = m_Objects.GetArrayElementAtIndex(index);
            var controlID = GUIUtility.GetControlID(FocusType.Passive, rect);
            EditorGUI.BeginChangeCheck();
            var changedObject = EditorGUI.ObjectField(rect, property.objectReferenceValue, typeof(Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = changedObject;
            }

            if (GUIUtility.keyboardControl == controlID && !selected)
                m_ReorderableList.index = index;
        }

        static bool IsPackable(Object o)
        {
            return o != null && (o is Sprite || o is Texture2D ||
                                 (o is DefaultAsset && ProjectWindowUtil.IsFolder(o.GetInstanceID())));
        }

        string DictionaryToString<K, V>(IDictionary<K, V> dictionary)
        {
            var stringBuilder = new StringBuilder();
            var comma = ",";
            var index = 0;
            foreach (var keyValue in dictionary)
            {
                var separator = index < dictionary.Count - 1 ? comma : string.Empty;
                stringBuilder.Append($"{{{keyValue.Key.ToString()},{keyValue.Value.ToString()}}}{separator}");
            }

            return stringBuilder.ToString();
        }

        private void DrawPackUI()
        {
            if (GUILayout.Button("Package", GUILayout.ExpandWidth(false)))
            {
                Target.Sprites.Clear();
                m_Keys.ClearArray();
                m_Values.ClearArray();
                for (int i = 0; i < m_Objects.arraySize; i++)
                {
                    Object obj = m_Objects.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (obj == null)
                    {
                        m_Objects.DeleteArrayElementAtIndex(i);
                        m_Objects.DeleteArrayElementAtIndex(i);
                    }

                    AddSprite(obj);
                }
            }
        }

        private void OnDisable()
        {
            Target.Sprites.Clear();
            m_Keys.ClearArray();
            m_Values.ClearArray();
            for (int i = 0; i < m_Objects.arraySize; i++)
            {
                Object obj = m_Objects.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj == null)
                {
                    m_Objects.DeleteArrayElementAtIndex(i);
                    m_Objects.DeleteArrayElementAtIndex(i);
                }

                AddSprite(obj);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}