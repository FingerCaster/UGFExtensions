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
        public List<DataTableRowData> RowDataList { get; private set; }

        public List<DataTableRowData> RowDataShowList;

        private List<DataTableRowData> RowDataTempList;
#if UNITY_2020_1_OR_NEWER
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

        private int m_CurrentPage = 0;
        private int m_ShowCount;
        private int m_AllPage = 0;

        private string m_PageField;

        private float m_LastHeight = 0;
        public void OpenWindow(string path, Encoding encoding)
        {
            m_encoding = encoding;
            m_codePage = encoding.CodePage;
            FilePath = path;
            RowDataList = DataTableUtility.LoadDataTableFile(FilePath, m_encoding);

            if (RowDataList == null)
                return;

            RowDataTempList = new List<DataTableRowData>();

            for (int i = 0; i < RowDataList.Count; i++)
            {
                DataTableRowData data = new DataTableRowData();

                for (int j = 0; j < RowDataList[i].Data.Count; j++)
                {
                    data.Data.Add(RowDataList[i].Data[j]);
                }

                RowDataTempList.Add(data);
            }
            

            if (RowDataList == null)
                return;
            
            LightMode = EditorPrefs.GetInt("DataTableEditor_" + Application.productName + "_LightMode", 0);

            RowDataShowList = new List<DataTableRowData>();
            SetPage();
            SkipToPage(0);
        }
        

        private void SetPage()
        {
            m_AllPage = RowDataList.Count / m_ShowCount;
            if (RowDataList.Count % m_ShowCount != 0)
            {
                m_AllPage += 1;
            }

            m_CurrentPage = 0;
            m_PageField = m_CurrentPage.ToString();
        }
        private void SkipToPage(int page)
        {
            if (page * m_ShowCount >= RowDataList.Count)
            {
                page--;
            }

            m_CurrentPage = page;
            int i = page * m_ShowCount;
            int count = i + m_ShowCount;
            if (count >RowDataList.Count)
            {
                count = RowDataList.Count;
            }
            RowDataShowList.Clear();
            for (; i < count; i++)
            {
                RowDataShowList.Add(RowDataList[i]);
            }
        }
        
        private void OnGUI()
        {
            if (Math.Abs(position.height - m_LastHeight) > 0.000001f)
            {
                m_ShowCount = (int)((this.position.height - 60) / 21);
                SetPage();
                var index = RowDataList.IndexOf(RowDataShowList[0]);
                
                SkipToPage(index/m_ShowCount);
                m_LastHeight = position.height;
            }
            
            m_scrollViewPos = GUILayout.BeginScrollView(m_scrollViewPos);
            if (RowDataList == null || RowDataList.Count == 0)
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
#if UNITY_2020_1_OR_NEWER
                reorderableList =
                    new UnityInternalBridge.ReorderableList(RowDataShowList, typeof(List<DataTableRowData>), true, false, true,
                        true);

#else
                reorderableList =
                    new ReorderableList(RowDatas, typeof(List<DataTableRowData>), true, false, true, true);

#endif
                reorderableList.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    for (int i = 0; i < RowDataShowList[index].Data.Count; i++)
                    {
                        if (RowDataShowList[index].Data.Count > 10)
                        {
                            rect.width =
                                (this.position.width - 20) /
                                10;
                        }
                        else
                        {
                            rect.width =
                                (this.position.width - 20) /
                                RowDataShowList[index].Data.Count;
                        }

                        rect.x = rect.width * i + 20;
                        RowDataShowList[index].Data[i] =
                            EditorGUI.TextField(rect, "", RowDataShowList[index].Data[i],
                                this.Theme);
                    }
                };
                

                reorderableList.onAddCallback = list =>
                {
                    bool result =
                        EditorUtility.DisplayDialog("提示", "添加 行 或 列", "行", "列");

                    if (result)
                    {
                        if (RowDataList.Count == 0)
                        {
                            RowDataList.Add(new DataTableRowData()
                            {
                                Data = new List<string>() {"", "", "", ""}
                            });
                        }
                        else
                        {
                            DataTableRowData data = new DataTableRowData();

                            for (int i = 0; i < RowDataList[0].Data.Count - 1; i++)
                            {
                                data.Data.Add("");
                            }

                            RowDataList.Add(data);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < RowDataList.Count; i++)
                            RowDataList[i].Data.Add("");
                    }
                    SetPage();
                    SkipToPage(m_AllPage);
                    Focus();
                };

                reorderableList.onRemoveCallback = list =>
                {
                    bool result =
                        EditorUtility.DisplayDialog("提示", "移除 行 或 列", "行", "列");
                    if (result)
                    {
                        int index = m_CurrentPage * m_ShowCount + list.index;
                        RowDataList.RemoveAt(index);
                    }
                    else
                    {
                        for (int i = 0; i < RowDataList.Count; i++)
                        {
                            RowDataList[i].Data.RemoveAt(RowDataList[i].Data.Count - 1);
                        }
                    }
                    SkipToPage(m_CurrentPage);
                    Focus();
                };

                reorderableList.drawHeaderCallback = (Rect rect) =>
                {
                    var filePathRect = rect;
                    filePathRect.width = rect.width/3;
                    EditorGUI.LabelField(filePathRect, FilePath);

                    var pageRect = rect;
                    pageRect.x = rect.width/3*2-50;
                    pageRect.width = 80;
                    
                    
                    if (m_CurrentPage <= 0)
                    {
                        GUI.enabled = false;
                    }

                    if (GUI.Button(pageRect,"上一页"))
                    {
                        SkipToPage(--m_CurrentPage);
                    }

                    GUI.enabled = true;

                    // m_PageField = m_CurrentPage.ToString();
                    pageRect.x += 80;
                    pageRect.width = 50;
                    m_PageField = EditorGUI.TextField(pageRect, (m_CurrentPage+1).ToString(),
                        new GUIStyle("TextField") {alignment = TextAnchor.MiddleCenter});
                    pageRect.x += 50;
                    pageRect.width = 50;
                    EditorGUI.LabelField(pageRect,$"/{m_AllPage}");
                    if (int.TryParse(m_PageField, out int page) && page <= m_AllPage && page>0 && page!= (m_CurrentPage+1) )
                    {
                        SkipToPage(page-1);
                    }
                    else
                    {
                        m_PageField = (m_CurrentPage+1).ToString();
                    }

                    if (m_CurrentPage >= (m_AllPage-1))
                    {
                        GUI.enabled = false;
                    }
                    pageRect.x += 50;
                    pageRect.width = 80;
                    if (GUI.Button(pageRect,"下一页"))
                    {
                        SkipToPage(++m_CurrentPage);
                    }

                    GUI.enabled = true;

                    var highLightRect = rect;
                    highLightRect.x = rect.width - 70;
                    EditorGUI.LabelField(highLightRect, "高亮模式");
                    highLightRect.x = rect.width - 20;

                    LightMode =
                        EditorGUI.Toggle(highLightRect, LightMode == 0 ? true : false)
                            ? 0
                            : 1;

                    EditorPrefs
                        .SetInt("DataTableEditor_" + Application.productName + "_LightMode",
                            LightMode);
                  
                    // Debug.Log($"滑动条 x:{m_scrollViewPos.x} rect: {rect}  比例 {m_scrollViewPos.x / rect.width}");
                };
            }

            reorderableList.DoLayoutList();

            if (RowDataList != null && RowDataList.Count > 0)
            {
                if (RowDataList[0].Data.Count > 10)
                {
                    float listItemWidth = 0f;
                    float listX = 0f;
                    listItemWidth = (position.width - 20) / 10;
                    listX = listItemWidth * (RowDataList[0].Data.Count - 1) + 20;
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

            RowDataTempList = new List<DataTableRowData>();
            for (int i = 0; i < RowDataList.Count; i++)
            {
                DataTableRowData data = new DataTableRowData();

                for (int j = 0; j < RowDataList[i].Data.Count; j++)
                {
                    data.Data.Add(RowDataList[i].Data[j]);
                }

                RowDataTempList.Add(data);
            }

            if (m_encoding == null)
            {
                m_encoding = Encoding.GetEncoding(m_codePage);
            }

            DataTableUtility.SaveDataTableFile(FilePath, RowDataList, m_encoding);
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
            if (RowDataList == null || RowDataList.Count == 0)
                return;

            int count = RowDataList[0].Data.Count;

            for (int i = 0; i < RowDataList.Count; i++)
            {
                int need = count - RowDataList[i].Data.Count;

                if (need > 0)
                    for (int j = 0; j < need; j++)
                        RowDataList[i].Data.Add("");
                else if (need < 0)
                    for (int j = 0; j < Mathf.Abs(need); j++)
                        RowDataList[i].Data.RemoveAt(RowDataList[i].Data.Count - 1);
            }
        }

        /// <summary>
        /// 检查表格是否进行更改
        /// </summary>
        /// <returns></returns>
        private bool CheckDirty()
        {
            if (RowDataTempList == null || RowDataList == null)
            {
                return false;
            }

            if (RowDataTempList.Count != RowDataList.Count)
                return true;

            for (int i = 0; i < RowDataList.Count; i++)
            {
                if (RowDataTempList[i].Data.Count != RowDataList[i].Data.Count)
                    return true;

                for (int j = 0; j < RowDataList[i].Data.Count; j++)
                {
                    if (RowDataList[i].Data[j] != RowDataTempList[i].Data[j])
                        return true;
                }
            }

            return false;
        }
    }
}