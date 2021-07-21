using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static DataTableEditor.Utility;

namespace DataTableEditor
{
    public class DataTableEditingWindowInstance
    {
        public DataTableEditingWindow Instance { get; private set; }

        public void SetData(string path, Encoding encoding)
        {
            FileInfo fileInfo = new FileInfo(path);
            // Instance = DataTableEditingWindow.CreateWindow<DataTableEditingWindow>(fileInfo.Name);
#if UNITY_2019_1_OR_NEWER
            Instance = LauncherEditorWindow.CreateWindow<DataTableEditingWindow>(fileInfo.Name);
#else
            Instance = EditorWindowUtility.CreateWindow<DataTableEditingWindow>(fileInfo.Name);
#endif
            Instance.OpenWindow(path, encoding);
            Instance.Show();
        }
    }


    public class DataTableEditingWindow : EditorWindow
    {
        public List<DataTableRowData> RowDatas { get; private set; }

        private List<DataTableRowData> RowDatasTemp;
#if UNITY_2019_1_OR_NEWER
        private UnityInternalBridge.ReorderableList reorderableList;
#else
        private ReorderableList reorderableList;
#endif
        public string FilePath { get; private set; }

        public int LightMode = 0;

        public string Theme = "LODCameraLine";

        private Vector2 m_scrollViewPos;

        private Encoding m_encoding;
        private int m_codePage;
        public void OpenWindow(string path, Encoding encoding)
        {
            m_encoding = encoding;
            m_codePage = encoding.CodePage;
            FilePath = path;
            RowDatas = DataTableUtility.LoadDataTableFile(FilePath, m_encoding);

            if (RowDatas == null)
                return;

            RowDatasTemp = new List<DataTableRowData>();

            for (int i = 0; i < RowDatas.Count; i++)
            {
                DataTableRowData data = new DataTableRowData();

                for (int j = 0; j < RowDatas[i].Data.Count; j++)
                {
                    data.Data.Add(RowDatas[i].Data[j]);
                }

                RowDatasTemp.Add(data);
            }

            if (RowDatas == null)
                return;

            LightMode = EditorPrefs.GetInt("DataTableEditor_" + Application.productName + "_LightMode", 0);
        }

        private void OnGUI()
        {
            m_scrollViewPos = GUILayout.BeginScrollView(m_scrollViewPos);
            if (RowDatas == null || RowDatas.Count == 0)
            {
                Close();
                GUILayout.EndScrollView();
                return;
            }

            CheckColumnCount();

            if (LightMode == 0)
            {
                Theme = "ScriptText";
            }
            else if (LightMode == 1)
            {
                Theme = "PreferencesSectionBox";
            }

            if (reorderableList == null)
            {
#if UNITY_2019_1_OR_NEWER
                reorderableList =
                    new UnityInternalBridge.ReorderableList(RowDatas, typeof(List<DataTableRowData>), true, false, true,
                        true);

#else
                reorderableList =
                    new ReorderableList(RowDatas, typeof(List<DataTableRowData>), true, false, true, true);

#endif

                reorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    for (int i = 0; i < RowDatas[index].Data.Count; i++)
                    {
                        if (RowDatas[index].Data.Count > 10)
                        {
                            rect.width =
                                (this.position.width - 20) /
                                10;
                        }
                        else
                        {
                            rect.width =
                                (this.position.width - 20) /
                                RowDatas[index].Data.Count;
                        }

                        rect.x = rect.width * i + 20;
                        RowDatas[index].Data[i] =
                            EditorGUI.TextField(rect, "", RowDatas[index].Data[i],
                                this.Theme);
                    }
                };

                reorderableList.onAddCallback = list =>
                {
                    bool result =
                        EditorUtility.DisplayDialog("提示", "添加 行 或 列", "行", "列");

                    if (result)
                    {
                        if (RowDatas.Count == 0)
                        {
                            RowDatas.Add(new DataTableRowData()
                            {
                                Data = new List<string>() {"", "", "", ""}
                            });
                        }
                        else
                        {
                            DataTableRowData data = new DataTableRowData();

                            for (int i = 0; i < RowDatas[0].Data.Count - 1; i++)
                            {
                                data.Data.Add("");
                            }

                            RowDatas.Add(data);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < RowDatas.Count; i++)
                            RowDatas[i].Data.Add("");
                    }
                    Focus();
                };

                reorderableList.onRemoveCallback = list =>
                {
                    bool result =
                        EditorUtility.DisplayDialog("提示", "移除 行 或 列", "行", "列");

                    if (result)
                    {
                        RowDatas.RemoveAt(list.index);
                    }
                    else
                    {
                        for (int i = 0; i < RowDatas.Count; i++)
                        {
                            RowDatas[i].Data.RemoveAt(RowDatas[i].Data.Count - 1);
                        }
                    }
                    Focus();
                };

                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, FilePath);
                    rect.x = rect.width - 70;
                    EditorGUI.LabelField(rect, "高亮模式");
                    rect.x = rect.width - 20;

                    LightMode =
                        EditorGUI.Toggle(rect, LightMode == 0 ? true : false)
                            ? 0
                            : 1;

                    EditorPrefs
                        .SetInt("DataTableEditor_" + Application.productName + "_LightMode",
                            LightMode);
                };
            }

            reorderableList.DoLayoutList();

            if (RowDatas != null && RowDatas.Count > 0)
            {
                if (RowDatas[0].Data.Count > 10)
                {
                    float listItemWidth = 0f;
                    float listX = 0f;
                    listItemWidth = (position.width - 20) / 10;
                    listX = listItemWidth * (RowDatas[0].Data.Count - 1) + 20;
                    GUILayout.Label("", new GUIStyle() {fixedWidth = listX});
                }
            }

            GUILayout.EndScrollView();
            if (IsCombinationKey(EventModifiers.Control, KeyCode.S, EventType.KeyDown))
            {
                SaveDataTable();
            }
        }

        private void SaveDataTable()
        {
            if (!CheckDirty())
                return;

            RowDatasTemp = new List<DataTableRowData>();
            for (int i = 0; i < RowDatas.Count; i++)
            {
                DataTableRowData data = new DataTableRowData();

                for (int j = 0; j < RowDatas[i].Data.Count; j++)
                {
                    data.Data.Add(RowDatas[i].Data[j]);
                }

                RowDatasTemp.Add(data);
            }

            if (m_encoding == null)
            {
                m_encoding = Encoding.GetEncoding(m_codePage);
            }

            DataTableUtility.SaveDataTableFile(FilePath, RowDatas, m_encoding);
        }

        private bool IsCombinationKey(EventModifiers preKey, KeyCode postKey, EventType postKeyEvent)
        {
            if (preKey != EventModifiers.None)
            {
                bool eventDown = (Event.current.modifiers & preKey) != 0;
                if (eventDown && Event.current.rawType == postKeyEvent && Event.current.keyCode == postKey)
                {
                    Event.current.Use();
                    return true;
                }
            }
            else
            {
                if (Event.current.rawType == postKeyEvent && Event.current.keyCode == postKey)
                {
                    Event.current.Use();
                    return true;
                }
            }

            return false;
        }

        private void OnDisable()
        {
            if (!CheckDirty())
                return;

            bool result = EditorUtility.DisplayDialog("提示", "你已经对表格进行了修改，是否需要保存？", "是", "否");
            if (result)
            {
                SaveDataTable();
            }
            Focus();
        }

        /// <summary>
        /// 检查列数一致性
        /// </summary>
        private void CheckColumnCount()
        {
            if (RowDatas == null || RowDatas.Count == 0)
                return;

            int count = RowDatas[0].Data.Count;

            for (int i = 0; i < RowDatas.Count; i++)
            {
                int need = count - RowDatas[i].Data.Count;

                if (need > 0)
                    for (int j = 0; j < need; j++)
                        RowDatas[i].Data.Add("");
                else if (need < 0)
                    for (int j = 0; j < Mathf.Abs(need); j++)
                        RowDatas[i].Data.RemoveAt(RowDatas[i].Data.Count - 1);
            }
        }

        /// <summary>
        /// 检查表格是否进行更改
        /// </summary>
        /// <returns></returns>
        private bool CheckDirty()
        {
            if (RowDatasTemp == null || RowDatas == null)
            {
                return false;
            }

            if (RowDatasTemp.Count != RowDatas.Count)
                return true;

            for (int i = 0; i < RowDatas.Count; i++)
            {
                if (RowDatasTemp[i].Data.Count != RowDatas[i].Data.Count)
                    return true;

                for (int j = 0; j < RowDatas[i].Data.Count; j++)
                {
                    if (RowDatas[i].Data[j] != RowDatasTemp[i].Data[j])
                        return true;
                }
            }

            return false;
        }
    }
}