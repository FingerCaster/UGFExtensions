using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using static DataTableEditor.Utility;

namespace DataTableEditor
{
    public class LauncherEditorWindow : EditorWindow
    {
        public static float ButtonHeight = 50;
        private Encoding m_CurrentEncoding;
        private bool m_IsValidEncode;

        private EncodingInfo[] m_AllEncodingTypeList;
        private string[] m_EncodingSelectionStrings;

        private int m_EncodingSelectionIndex = 0;

        [MenuItem("DataTable/DataTableEditor &1", priority = 2)]
        public static void OpenWindow()
        {
            LauncherEditorWindow window = GetWindow<LauncherEditorWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            m_EncodingSelectionIndex = EditorPrefs.GetInt("DataTableEditor_" + Application.productName + "_EncodingSelectionIndex", 0);

            m_AllEncodingTypeList = Encoding.GetEncodings();

            m_EncodingSelectionStrings = new string[m_AllEncodingTypeList.Length];

            for (int i = 0; i < m_AllEncodingTypeList.Length; i++)
            {
                m_EncodingSelectionStrings[i] = m_AllEncodingTypeList[i].Name;
            }

            if (m_EncodingSelectionIndex > m_AllEncodingTypeList.Length - 1)
            {
                m_EncodingSelectionIndex = 0;
            }

            m_CurrentEncoding = Encoding.GetEncoding(m_AllEncodingTypeList[m_EncodingSelectionIndex].Name);

            EncodingCheck();
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            m_EncodingSelectionIndex = EditorGUILayout.Popup("数据表编码", m_EncodingSelectionIndex, m_EncodingSelectionStrings);
            if (EditorGUI.EndChangeCheck())
            {
                EncodingCheck();
            }

            if (!m_IsValidEncode)
            {
                return;
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
                m_CurrentEncoding = Encoding.GetEncoding(m_AllEncodingTypeList[m_EncodingSelectionIndex].Name);
                EditorPrefs.SetInt("DataTableEditor_" + Application.productName + "_EncodingSelectionIndex", m_EncodingSelectionIndex);
                m_IsValidEncode = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                m_IsValidEncode = false;
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