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
        private string[] m_HelperTypeNames;
        private int m_HelperTypeNameIndex;
        private Page m_Page;
        private SerializedProperty m_Searchable;
        private AutoBindSettingConfig m_SettingConfig;
        private bool m_SettingDataExpanded = true;
        private int m_LastSettingDataNameIndex;
        private bool m_SettingDataError;
        
        private void OnEnable()
        {
            m_Target = (ReferenceBindComponent) target;
            m_HelperTypeNames = ReferenceBindUtility.GetTypeNames();

            m_Page = new Page(10, m_Target.GetAllBindObjectsCount());
            if (!CheckAutoBindSettingData())
            {
                return;
            }

            InitSearchable();
            m_Target.SetClassName(string.IsNullOrEmpty(m_Target.GeneratorCodeName)
                ? m_Target.gameObject.name
                : m_Target.GeneratorCodeName);
            
            if (string.IsNullOrEmpty(m_Target.RuleHelperTypeName))
            {
                m_Target.SetRuleHelperTypeName(nameof(DefaultAutoBindRuleHelper));
            }
            else
            {
                m_Target.SetRuleHelperTypeName(m_Target.RuleHelperTypeName);
            }
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
            DrawHelperSelect();
            EditorGUILayout.Space();
            DrawBindAssetOrPrefab();
            EditorGUILayout.Space();
            DrawSetting();
            EditorGUILayout.Space();
            DrawBindObjects();
            m_Page.Draw();
            serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 绘制辅助器选择框
        /// </summary>
        private void DrawHelperSelect()
        {
            if (m_Target.RuleHelper != null)
            {
                for (int i = 0; i < m_HelperTypeNames.Length; i++)
                {
                    if (m_Target.RuleHelperTypeName == m_HelperTypeNames[i])
                    {
                        m_HelperTypeNameIndex = i;
                    }
                }
            }
            else
            {
                m_Target.SetRuleHelperTypeName(m_Target.RuleHelperTypeName);
            }

            foreach (GameObject go in Selection.gameObjects)
            {
                ReferenceBindComponent autoBindTool = go.GetComponent<ReferenceBindComponent>();
                if (autoBindTool == null)
                {
                    continue;
                }

                if (autoBindTool.RuleHelper == null)
                {
                    m_Target.SetRuleHelperTypeName(m_Target.RuleHelperTypeName);
                }
            }

            int selectedIndex = EditorGUILayout.Popup("AutoBindRuleHelper", m_HelperTypeNameIndex, m_HelperTypeNames);
            if (selectedIndex != m_HelperTypeNameIndex)
            {
                m_HelperTypeNameIndex = selectedIndex;
                m_Target.SetRuleHelperTypeName(m_HelperTypeNames[selectedIndex]);
            }
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
            if (GUILayout.Button("自动绑定组件"))
            {
                AutoBindComponent();
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
            m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
        }

        private void Refresh()
        {
            m_Target.Refresh();
            m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
        }

        private UnityEngine.Object m_NeedBindObject = null;

        /// <summary>
        /// 绘制拖拽区域
        /// </summary>
        private void DrawBindAssetOrPrefab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("绑定资源或预制体");
            m_NeedBindObject = EditorGUILayout.ObjectField(m_NeedBindObject, typeof(UnityEngine.Object), false);
            GUI.enabled = m_NeedBindObject != null;
            if (GUILayout.Button("绑定", GUILayout.Width(50)))
            {
                m_Target.AddBindAssetsOrPrefabs(ReferenceBindUtility.GetFiledName(m_NeedBindObject),m_NeedBindObject);
                m_NeedBindObject = null;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制键值对数据
        /// </summary>
        private void DrawBindObjects()
        {
            //绘制key value数据

            int needDeleteIndex = -1;

            EditorGUILayout.BeginVertical();
            int i = m_Page.CurrentPage * m_Page.ShowCount;
            int index = 0;

            if (i < m_Target.BindAssetsOrPrefabs.Count)
            {
                EditorGUILayout.LabelField("绑定的资源或预制体");
            }

            for (; i < m_Target.BindAssetsOrPrefabs.Count; i++,index++)
            {
                if (DrawBindObjectData(m_Target.BindAssetsOrPrefabs[i],index))
                {
                    needDeleteIndex = i;
                }
            }

            if (i < m_Target.BindComponents.Count)
            {
                EditorGUILayout.LabelField("绑定的组件");
            }

            for (i=0; i < m_Target.BindComponents.Count; i++,index++)
            {
                if (DrawBindObjectData(m_Target.BindComponents[i],index))
                {
                    needDeleteIndex = i;
                }
            }

            //删除data
            if (needDeleteIndex != -1)
            {
                if (index < m_Target.BindAssetsOrPrefabs.Count)
                {
                    m_Target.BindAssetsOrPrefabs.RemoveAt(needDeleteIndex);
                }
                else
                {
                    m_Target.BindComponents.RemoveAt(needDeleteIndex);
                }

                m_Target.SyncBindObjects();
                m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
            }

            EditorGUILayout.EndVertical();
        }

        private bool DrawBindObjectData(ReferenceBindComponent.BindObjectData bindObjectData, int index)
        {
            bool isDelete = false;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{index}]", GUILayout.Width(40));

            EditorGUI.BeginChangeCheck();
            string fieldName = EditorGUILayout.TextField(bindObjectData.FieldName);
            if (EditorGUI.EndChangeCheck())
            {
                bindObjectData.FieldName = fieldName;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(bindObjectData.BindObject, typeof(UnityEngine.Object), true);
            GUI.enabled = true;

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                //将元素下标添加进删除list
                isDelete = true;
            }

            EditorGUILayout.EndHorizontal();


            if (bindObjectData.FileNameIsInvalid)
            {
                EditorGUILayout.HelpBox("绑定对象命名无效 不符合规则。 请修改!", MessageType.Error);
            }

            if (bindObjectData.IsRepeatName)
            {
                EditorGUILayout.HelpBox("绑定对象命名不能相同 请修改!", MessageType.Error);
            }

            return isDelete;
        }

        /// <summary>
        /// 排序
        /// </summary>
        private void Sort()
        {
            m_Target.Sort();
            m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        private void RemoveAll()
        {
            m_Target.RemoveAll();
            m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
        }

        /// <summary>
        /// 删除Missing Or Null
        /// </summary>
        private void RemoveNull()
        {
            m_Target.RemoveNull();
            m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
        }
        
        /// <summary>
        /// 自动绑定组件
        /// </summary>
        private void AutoBindComponent()
        {
            m_Target.AutoBindComponent();
            m_Page.SetAllCount(m_Target.GetAllBindObjectsCount());
        }
    }
}