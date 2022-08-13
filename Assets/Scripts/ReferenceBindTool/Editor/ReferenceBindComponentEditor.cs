using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    [CustomEditor(typeof(ReferenceBindComponent))]
    public class ReferenceBindComponentEditor : UnityEditor.Editor
    {
        private ReferenceBindComponent m_Target;
        private Page m_Page;
        private AddBindDataWindow m_AddBindDataWindow = new AddBindDataWindow();
        private SerializedProperty m_Searchable;
        private AutoBindSettingConfig m_SettingConfig;
        private bool m_SettingDataExpanded = true;
        private int m_LastSettingDataNameIndex;
        private bool m_SettingDataError;

        private void OnEnable()
        {
            m_Target = (ReferenceBindComponent) target;
            m_ShowControls = new Dictionary<int, bool>();
            m_Page = new Page(10, m_Target.ReferenceDataList.Count);
            if (!CheckAutoBindSettingData())
            {
                return;
            }

            InitSearchable();
            m_Target.SetClassName(string.IsNullOrEmpty(m_Target.GeneratorCodeName)
                ? m_Target.gameObject.name
                : m_Target.GeneratorCodeName);
            serializedObject.ApplyModifiedProperties();
        }

        private void InitSearchable()
        {
            var settingDataNames = m_SettingConfig.Settings.Select(_ => _.Name).ToArray();
            if (m_Target.SettingData == null)
            {
                m_Target.SetSettingData(m_SettingConfig.Settings[0]);
                m_LastSettingDataNameIndex = 0;
            }
            else
            {
                var data = m_SettingConfig.GetSettingData(m_Target.SettingData.Name);
                if (data == null)
                {
                    Debug.LogError($"不存在名为‘{m_Target.SettingData.Name}’的AutoBindSettingData");
                    m_SettingDataError = true;
                    return;
                }

                m_Target.SetSettingData(m_SettingConfig.GetSettingData(m_Target.SettingData.Name));
                m_LastSettingDataNameIndex =
                    m_SettingConfig.Settings.FindIndex(_ => _.Name == m_Target.SettingData.Name);
            }

            m_Searchable = serializedObject.FindProperty("m_SettingDataSearchable");
            m_Target.SetSearchable(settingDataNames, m_LastSettingDataNameIndex);
        }

        private bool CheckAutoBindSettingData()
        {
            string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
            if (paths.Length == 0)
            {
                Debug.LogError("不存在AutoBindSettingConfig");
                return false;
            }

            if (paths.Length > 1)
            {
                Debug.LogError("AutoBindSettingConfig数量大于1");
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            m_SettingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
            if (m_SettingConfig.Settings.Count == 0)
            {
                Debug.LogError("不存在AutoBindSettingData");
                return false;
            }

            return true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawTopButton();
            EditorGUILayout.Space();
            DrawDragArea();
            EditorGUILayout.Space();
            DrawSetting();
            EditorGUILayout.Space();
            DrawReferences();
            m_Page.Draw();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 绘制设置项
        /// </summary>
        private void DrawSetting()
        {
            m_SettingDataExpanded = EditorGUILayout.Foldout(m_SettingDataExpanded, "SettingData", true);

            if (!m_SettingDataExpanded)
            {
                return;
            }

            if (m_SettingDataError)
            {
                EditorGUILayout.HelpBox($"不存在名为‘{m_Target.SettingData.Name}’的AutoBindSettingData", MessageType.Error);
                if (GUILayout.Button($"创建 {m_Target.SettingData.Name} 绑定配置"))
                {
                    bool result = ComponentAutoBindToolUtility.AddAutoBindSetting(m_Target.SettingData.Name, "", "");
                    if (!result)
                    {
                        EditorUtility.DisplayDialog("创建配置", "创建代码自动生成配置失败，请检查配置信息！", "确定");
                        return;
                    }

                    m_Target.SetSettingData(m_Target.SettingData.Name);
                    m_SettingDataError = false;
                }

                if (GUILayout.Button("使用默认配置"))
                {
                    m_Target.SetSettingData(m_SettingConfig.Default);
                    m_SettingDataError = false;
                }

                return;
            }

            m_Searchable ??= serializedObject.FindProperty("m_SettingDataSearchable");
            EditorGUILayout.PropertyField(m_Searchable);
            if (m_Target.SettingDataSearchable.Select != m_LastSettingDataNameIndex)
            {
                if (m_Target.SettingDataSearchable.Select >= m_SettingConfig.Settings.Count)
                {
                    m_SettingDataError = true;
                    return;
                }

                m_Target.SetSettingData(m_SettingConfig.Settings[m_Target.SettingDataSearchable.Select]);
                m_Target.SetClassName(string.IsNullOrEmpty(m_Target.GeneratorCodeName)
                    ? m_Target.gameObject.name
                    : m_Target.GeneratorCodeName);
                m_LastSettingDataNameIndex = m_Target.SettingDataSearchable.Select;
            }


            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("命名空间：");
            EditorGUILayout.LabelField(m_Target.SettingData.Namespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_Target.SetClassName(EditorGUILayout.TextField(new GUIContent("类名："), m_Target.GeneratorCodeName));

            if (GUILayout.Button("物体名"))
            {
                m_Target.SetClassName(m_Target.gameObject.name);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("代码保存路径：");
            EditorGUILayout.LabelField(m_Target.SettingData.CodePath);
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrEmpty(m_Target.SettingData.CodePath))
            {
                EditorGUILayout.HelpBox("代码保存路径不能为空!", MessageType.Error);
            }
        }

        /// <summary>
        /// 绘制顶部按钮
        /// </summary>
        private void DrawTopButton()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("排序"))
            {
                Sort();
            }

            if (GUILayout.Button("刷新对象"))
            {
                Refresh();
            }
            if (GUILayout.Button("重置组建字段名"))
            {
                ResetAllFieldName();
            }

            if (GUILayout.Button("删除空对象"))
            {
                RemoveNull();
            }

            if (GUILayout.Button("全部删除"))
            {
                RemoveAll();
            }

            if (GUILayout.Button("生成绑定代码"))
            {
                string className = !string.IsNullOrEmpty(m_Target.GeneratorCodeName)
                    ? m_Target.GeneratorCodeName
                    : m_Target.gameObject.name;

                ReferenceBindUtility.GenAutoBindCode(m_Target, className, m_Target.SettingData.CodePath);
            }

            EditorGUILayout.EndHorizontal();
        }
        private void ResetAllFieldName()
        {
            m_Target.ResetAllFieldName();
            m_Page.SetAllCount(m_Target.ReferenceDataList.Count);
        }
        private void Refresh()
        {
            m_Target.Refresh();
            m_Page.SetAllCount(m_Target.ReferenceDataList.Count);
        }

        /// <summary>
        /// 绘制拖拽区域
        /// </summary>
        private void DrawDragArea()
        {
            Rect dragRect = EditorGUILayout.GetControlRect(false, 40);
            var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
            GUI.Box(dragRect, "请将需要绑定的物体拖到此位置", style);
            Event e = Event.current;
            if (dragRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.DragUpdated)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                else if (e.type == EventType.DragPerform)
                {
                    UnityEngine.Object[] objs = DragAndDrop.objectReferences;
                    e.Use();
                    for (int i = 0; objs != null && i < objs.Length; i++)
                    {
                        if (objs[i] != null)
                        {
                            m_Target.AddReferenceData(objs[i]);
                        }
                    }
                }
            }
        }

        private Dictionary<int, bool> m_ShowControls;

        /// <summary>
        /// 绘制键值对数据
        /// </summary>
        private void DrawReferences()
        {
            //绘制key value数据

            int needDeleteIndex = -1;

            EditorGUILayout.BeginVertical();
            int i = m_Page.CurrentPage * m_Page.ShowCount;
            int count = i + m_Page.ShowCount;
            if (count > m_Target.ReferenceDataList.Count)
            {
                count = m_Target.ReferenceDataList.Count;
            }

            for (; i < count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                Rect rect = EditorGUILayout.GetControlRect(false);
                int id = m_Target.ReferenceDataList[i].BindReference.GetInstanceID();
                m_ShowControls.TryGetValue(id, out bool isShow);
                if (!m_Target.ReferenceDataList[i].IsOnlyBindSelf)
                {
                    m_ShowControls[id] = EditorGUI.Foldout(new Rect(rect.x, rect.y, 20, rect.height), isShow, "");
                }

                EditorGUI.LabelField(new Rect(rect.x, rect.y, 40, rect.height), $"[{i}]",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                GUI.enabled = false;
                EditorGUI.ObjectField(new Rect(rect.x + 40, rect.y, rect.width - 40, rect.height),
                    m_Target.ReferenceDataList[i].BindReference, typeof(Component), true);
                GUI.enabled = true;


                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    //将元素下标添加进删除list
                    needDeleteIndex = i;
                }

                EditorGUILayout.EndHorizontal();
                if (isShow)
                {
                    DrawComponents(m_Target.ReferenceDataList[i]);
                }
            }

            //删除data
            if (needDeleteIndex != -1)
            {
                m_Target.ReferenceDataList.RemoveAt(needDeleteIndex);
                m_Target.SyncBindObjects();
                m_Page.SetAllCount(m_Target.ReferenceDataList.Count);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawComponents(ReferenceBindComponent.ReferenceData referenceData)
        {
            EditorGUILayout.BeginVertical();
            int needDeleteIndex = -1;
            bool isChange = false;

            for (int i = 0; i < referenceData.BindObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                string lastName = referenceData.BindObjects[i].FieldName;
                referenceData.BindObjects[i].FieldName = EditorGUILayout.TextField(referenceData.BindObjects[i].FieldName);
                if (referenceData.BindObjects[i].FieldName != lastName)
                {
                    Refresh();
                }
                GUI.enabled = false;
                EditorGUILayout.ObjectField(referenceData.BindObjects[i].BindObject, typeof(Component), true);
                GUI.enabled = true;
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    //将元素下标添加进删除list
                    needDeleteIndex = i;
                    isChange = true;
                }

                EditorGUILayout.EndHorizontal();
                if (referenceData.BindObjects[i].FileNameIsInvalid)
                {
                    EditorGUILayout.HelpBox("绑定对象命名无效 不符合规则。 请修改!", MessageType.Error);
                }
                if (referenceData.BindObjects[i].IsRepeatName)
                {
                    EditorGUILayout.HelpBox("绑定对象命名不能相同 请修改!", MessageType.Error);
                }
            }


            if (GUILayout.Button("AddBindData"))
            {
                m_AddBindDataWindow.Show(EditorGUILayout.GetControlRect(), referenceData);
            }

            EditorGUILayout.EndVertical();

            if (needDeleteIndex != -1)
            {
                referenceData.BindObjects.RemoveAt(needDeleteIndex);
            }

            if (m_AddBindDataWindow.IsChanged)
            {
                if (referenceData.BindReference.GetInstanceID() != m_AddBindDataWindow.ReferenceId)
                {
                    return;
                }

                m_AddBindDataWindow.IsChanged = false;
                var selectObjects = m_AddBindDataWindow.GetSelectObjects();

                for (int i = 0; i < selectObjects.Count; i++)
                {
                    bool isRepeat = false;
                    string filedName = ReferenceBindUtility.GetFiledName(selectObjects[i]);

                    foreach (var reference in m_Target.ReferenceDataList)
                    {
                        foreach (var bindComponent in reference.BindObjects)
                        {
                            if (bindComponent.FieldName == filedName)
                            {
                                isRepeat = true;
                                break;
                            }
                        }

                        if (isRepeat)
                        {
                            break;
                        }
                    }

                    referenceData.BindObjects.Add(
                        new ReferenceBindComponent.BindObjectData(isRepeat, filedName, selectObjects[i]));
                }

                isChange = true;
            }

            if (isChange)
            {
                m_Target.SyncBindObjects();
            }
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void Sort()
        {
            m_Target.Sort();
            m_Page.SetAllCount(m_Target.ReferenceDataList.Count);
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        private void RemoveAll()
        {
            m_Target.RemoveAll();
            m_Page.SetAllCount(m_Target.ReferenceDataList.Count);
        }

        /// <summary>
        /// 删除Missing Or Null
        /// </summary>
        private void RemoveNull()
        {
            m_Target.RemoveNull();
            m_Page.SetAllCount(m_Target.ReferenceDataList.Count);
        }
    }
}