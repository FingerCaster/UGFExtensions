using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoboRyanTron.SearchableEnum;
using UnityEditor;
using UnityEngine;
using static DataTableEditor.Utility;

namespace DataTableEditor
{
    public class LauncherEditorWindow : EditorWindow
    {
        public static float ButtonHeight = 50;
        private Encoding m_CurrentEncoding;
        
        private int m_EncodingSelectionIndex = 0;

        [SerializeField] private SearchableData m_Data;
        private SerializedProperty m_DataProperty;

        [MenuItem("DataTable/DataTableEditor &1", priority = 0)]
        public static void OpenWindow()
        {
            LauncherEditorWindow window = GetWindow<LauncherEditorWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            m_EncodingSelectionIndex = EditorPrefs.GetInt("DataTableEditor_" + Application.productName + "_EncodingSelectionIndex", 0);
            m_Data = new SearchableData
            {
                Select = m_EncodingSelectionIndex,
                Names = Encoding.GetEncodings().Select(_ => _.Name).ToArray()
            };
            m_DataProperty = new SerializedObject(this).FindProperty("m_Data");
            
            m_CurrentEncoding = Encoding.GetEncoding(m_Data.Names[m_EncodingSelectionIndex]);

            EncodingCheck();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            //m_EncodingSelectionIndex = EditorGUILayout.Popup("数据表编码", m_EncodingSelectionIndex, m_EncodingSelectionStrings);
            EditorGUILayout.PropertyField(m_DataProperty,new GUIContent("数据表编码"));
            if (m_EncodingSelectionIndex != m_Data.Select)
            {
                EncodingCheck();
            }

            if (GUILayout.Button("新建", GUILayout.Height(ButtonHeight)))
                ButtonNew();

            if (GUILayout.Button("加载", GUILayout.Height(ButtonHeight)))
                ButtonLoad();
        }
        

        private void EncodingCheck()
        {
            try
            {
                m_EncodingSelectionIndex = m_Data.Select;
                m_CurrentEncoding = Encoding.GetEncoding(m_Data.Names[m_EncodingSelectionIndex]);
                EditorPrefs.SetInt("DataTableEditor_" + Application.productName + "_EncodingSelectionIndex", m_EncodingSelectionIndex);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void ButtonNew()
        {
            var m_DataTableEditingWindow = new DataTableEditingWindowInstance();
            m_DataTableEditingWindow.SetData(DataTableUtility.NewDataTableFile(m_CurrentEncoding), m_CurrentEncoding);
        }

        private void ButtonLoad()
        {
            var m_DataTableEditingWindow = new DataTableEditingWindowInstance();
            string filePath = EditorUtility.OpenFilePanel("加载数据表格文件", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(filePath))
            {
                m_DataTableEditingWindow.SetData(filePath, m_CurrentEncoding);
            }
        }
    }
}