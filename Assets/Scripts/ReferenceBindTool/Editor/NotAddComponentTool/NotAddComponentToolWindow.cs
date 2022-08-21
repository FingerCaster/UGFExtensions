using System;
using System.Linq;
using RoboRyanTron.SearchableEnum;
using UnityEditor;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class NotAddComponentToolWindow : EditorWindow
    {
        [MenuItem("Tools/ReferenceBindTools/NotAddComponentToolWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<NotAddComponentToolWindow>();
            window.titleContent = new GUIContent("NotAddComponentToolWindow");
            window.Show();
        }

        private NotAddComponentData m_NotAddComponentData = new NotAddComponentData();
        private Page m_Page;
        [SerializeField] private SearchableData m_SettingDataSearchable;
        private SerializedProperty m_SearchableSerializedProperty;
        private ReferenceBindCodeGeneratorSettingConfig m_CodeGeneratorSettingConfig;
        private bool m_SettingDataExpanded = true;
        private int m_LastSettingDataNameIndex;
        private bool m_SettingDataError;

        private HelperInfo<IBindComponentsRuleHelper> m_BindComponentsHelperInfo;

        private bool m_IsInitError = false;

        private void SetGameObject(GameObject gameObject)
        {
            m_NotAddComponentData.GameObject = gameObject;
            m_SettingDataError = false;
            
            if (m_NotAddComponentData.CodeGeneratorSettingData == null)
            {
                m_NotAddComponentData.SetSettingData(m_CodeGeneratorSettingConfig.Settings[0]);
            }
            else
            {
                var data = m_CodeGeneratorSettingConfig.GetSettingData(m_NotAddComponentData.CodeGeneratorSettingData.Name);
                if (data == null)
                {
                    Debug.LogError($"不存在名为‘{m_NotAddComponentData.CodeGeneratorSettingData.Name}’的AutoBindSettingData");
                    m_SettingDataError = true;
                    return;
                }

                m_NotAddComponentData.SetSettingData(
                    m_CodeGeneratorSettingConfig.GetSettingData(m_NotAddComponentData.CodeGeneratorSettingData.Name));
            }

            m_NotAddComponentData.SetClassName(string.IsNullOrEmpty(m_NotAddComponentData.GeneratorCodeName)
                ? m_NotAddComponentData.GameObject.name
                : m_NotAddComponentData.GeneratorCodeName);
     
           
        }
        private void OnEnable()
        {
            try
            {
                m_Page = new Page(10, 0);
                if (!CheckCodeGeneratorSettingData())
                {
                    m_IsInitError = true;
                    return;
                }
                InitSearchable();
                InitHelperInfos();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                m_IsInitError = true;
            }
        }
        private GameObject m_CurrentGameObject;
        public void OnGUI()
        {
            if (m_IsInitError)
            {
                return;
            }

            if (m_IsCompiling && !EditorApplication.isCompiling)
            {
                m_IsCompiling = false;
                OnCompileComplete();
            }
            EditorGUI.BeginChangeCheck();
            var currentGameObject = (GameObject) EditorGUILayout.ObjectField("GameObject", m_CurrentGameObject, typeof(GameObject), true);

            if (EditorGUI.EndChangeCheck())
            {
                m_CurrentGameObject = currentGameObject;
                SetGameObject(m_CurrentGameObject);
            }

            if (m_NotAddComponentData.GameObject == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                DrawTopButton();
                EditorGUILayout.Space();
                DrawHelperSelect();
                EditorGUILayout.Space();
                DrawSetting();
                EditorGUILayout.Space();
                DrawBindObjects();
                m_Page.Draw();
            }

        }

        #region 规则帮助类

        private bool m_IsCompiling = false;

        private void InitHelperInfos()
        {
            m_NotAddComponentData.SetRuleHelperTypeName(string.IsNullOrEmpty(m_NotAddComponentData.RuleHelperTypeName)
                ? typeof(DefaultBindComponentsRuleHelper).FullName
                : m_NotAddComponentData.RuleHelperTypeName);
            
            m_BindComponentsHelperInfo = new HelperInfo<IBindComponentsRuleHelper>("m_BindComponentsRule");

            m_BindComponentsHelperInfo.Init(m_NotAddComponentData.RuleHelperTypeName, typeName =>
            {
                m_NotAddComponentData.SetRuleHelperTypeName(typeName);
                return m_NotAddComponentData.RuleHelperTypeName;
            });

            RefreshHelperTypeNames();
        }

        void OnCompileComplete()
        {
            RefreshHelperTypeNames();
        }

        void RefreshHelperTypeNames()
        {
            m_BindComponentsHelperInfo.Refresh();
        }

        /// <summary>
        /// 绘制辅助器选择框
        /// </summary>
        private void DrawHelperSelect()
        {
            m_BindComponentsHelperInfo.Draw();
        }

        #endregion

        #region 设置项

        /// <summary>
        /// 初始化代码生成配置查找工具
        /// </summary>
        private void InitSearchable()
        {
            var settingDataNames = m_CodeGeneratorSettingConfig.Settings.Select(_ => _.Name).ToArray();
            m_SettingDataSearchable = new SearchableData() {Select = 0, Names = settingDataNames};
            m_SearchableSerializedProperty = new SerializedObject(this).FindProperty("m_SettingDataSearchable");
        }

        private void ReloadSearchableData(string currentName)
        {
            string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
            if (paths.Length == 0)
            {
                Debug.LogError("不存在AutoBindSettingConfig");
                return;
            }

            if (paths.Length > 1)
            {
                Debug.LogError("AutoBindSettingConfig数量大于1");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            m_CodeGeneratorSettingConfig = AssetDatabase.LoadAssetAtPath<ReferenceBindCodeGeneratorSettingConfig>(path);
            if (m_CodeGeneratorSettingConfig.Settings.Count == 0)
            {
                Debug.LogError("不存在AutoBindSettingData");
                return;
            }
            var settingDataNames = m_CodeGeneratorSettingConfig.Settings.Select(_ => _.Name).ToList();

            int findIndex = m_CodeGeneratorSettingConfig.Settings.FindIndex(_ => _ == m_NotAddComponentData.CodeGeneratorSettingData);
            if (findIndex == -1)
            {
                m_SettingDataError = true;
            }
            else
            {
                m_SettingDataSearchable.Select = findIndex;
                m_SettingDataSearchable.Names = settingDataNames.ToArray();
                m_SearchableSerializedProperty = new SerializedObject(this).FindProperty("m_SettingDataSearchable");
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
                EditorGUILayout.HelpBox($"不存在名为‘{m_NotAddComponentData.CodeGeneratorSettingData.Name}’的AutoBindSettingData",
                    MessageType.Error);
                if (!string.IsNullOrEmpty(m_NotAddComponentData.CodeGeneratorSettingData.Name))
                {
                    if (GUILayout.Button($"创建 {m_NotAddComponentData.CodeGeneratorSettingData.Name} 绑定配置"))
                    {
                        bool result =
                            ReferenceBindUtility.AddAutoBindSetting(m_NotAddComponentData.CodeGeneratorSettingData.Name, "", "");
                        if (!result)
                        {
                            EditorUtility.DisplayDialog("创建配置", "创建代码自动生成配置失败，请检查配置信息！", "确定");
                            return;
                        }

                        m_NotAddComponentData.SetSettingData(m_NotAddComponentData.CodeGeneratorSettingData.Name);
                        m_SettingDataError = false;
                    }
                }

                if (GUILayout.Button("使用默认配置"))
                {
                    m_NotAddComponentData.SetSettingData(m_CodeGeneratorSettingConfig.Default);
                    m_SettingDataError = false;
                }

                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_SearchableSerializedProperty);
            if (m_SettingDataSearchable.Select != m_LastSettingDataNameIndex)
            {
                if (m_SettingDataSearchable.Select >= m_CodeGeneratorSettingConfig.Settings.Count)
                {
                    m_SettingDataError = true;
                    return;
                }

                m_NotAddComponentData.SetSettingData(m_CodeGeneratorSettingConfig.Settings[m_SettingDataSearchable.Select]);
                m_NotAddComponentData.SetClassName(string.IsNullOrEmpty(m_NotAddComponentData.GeneratorCodeName)
                    ? m_NotAddComponentData.GameObject.name
                    : m_NotAddComponentData.GeneratorCodeName);
                m_LastSettingDataNameIndex = m_SettingDataSearchable.Select;
            }
            if (GUILayout.Button("Reload",GUILayout.Width(80)))
            {
                ReloadSearchableData(m_SettingDataSearchable.Names[m_SettingDataSearchable.Select]);
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel("命名空间：");
            EditorGUILayout.LabelField(m_NotAddComponentData.CodeGeneratorSettingData.Namespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_NotAddComponentData.SetClassName(EditorGUILayout.TextField(new GUIContent("类名："), m_NotAddComponentData.GeneratorCodeName));

            if (GUILayout.Button("物体名"))
            {
                m_NotAddComponentData.SetClassName(m_NotAddComponentData.GameObject.name);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("代码保存路径：");
            EditorGUILayout.LabelField(m_NotAddComponentData.CodeGeneratorSettingData.CodePath);
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrEmpty(m_NotAddComponentData.CodeGeneratorSettingData.CodePath))
            {
                EditorGUILayout.HelpBox("代码保存路径不能为空!", MessageType.Error);
            }
        }

        /// <summary>
        /// 检查代码生成配置数据
        /// </summary>
        /// <returns></returns>
        private bool CheckCodeGeneratorSettingData()
        {
            string[] paths = AssetDatabase.FindAssets($"t:{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            if (paths.Length == 0)
            {
                Debug.LogError($"不存在{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
                return false;
            }

            if (paths.Length > 1)
            {
                Debug.LogError($"{nameof(ReferenceBindCodeGeneratorSettingConfig)}数量大于1");
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            m_CodeGeneratorSettingConfig = AssetDatabase.LoadAssetAtPath<ReferenceBindCodeGeneratorSettingConfig>(path);
            if (m_CodeGeneratorSettingConfig.Settings.Count == 0)
            {
                Debug.LogError($"不存在{nameof(ReferenceBindCodeGeneratorSettingData)}");
                return false;
            }

            return true;
        }

        #endregion

        #region 顶部功能按钮

        /// <summary>
        /// 绘制顶部按钮
        /// </summary>
        private void DrawTopButton()
        {
            EditorGUILayout.BeginHorizontal();

            // if (GUILayout.Button("排序"))
            // {
            //     Sort();
            // }
            //
            // if (GUILayout.Button("刷新对象"))
            // {
            //     Refresh();
            // }
            //
            // if (GUILayout.Button("重置组建字段名"))
            // {
            //     ResetAllFieldName();
            // }
            //
            // if (GUILayout.Button("删除空对象"))
            // {
            //     RemoveNull();
            // }
            //
            // if (GUILayout.Button("全部删除"))
            // {
            //     RemoveAll();
            // }

            if (GUILayout.Button("自动绑定组件"))
            {
                AutoBindComponent();
            }

            // if (GUILayout.Button("生成绑定代码"))
            // {
            //     string className = !string.IsNullOrEmpty(m_NotAddComponentData.GeneratorCodeName)
            //         ? m_NotAddComponentData.GeneratorCodeName
            //         : m_NotAddComponentData.GameObject.name;
            //
            //     ReferenceBindUtility.GenAutoBindCode(m_NotAddComponentData, className, m_NotAddComponentData.CodeGeneratorSettingData.CodePath);
            // }

            EditorGUILayout.EndHorizontal();
        }

        // /// <summary>
        // /// 重置所有字段名
        // /// </summary>
        // private void ResetAllFieldName()
        // {
        //     m_NotAddComponentData.ResetAllFieldName();
        //     m_Page.SetAllCount(m_NotAddComponentData.BindComponents.Count);
        // }
        //
        // /// <summary>
        // /// 刷新数据
        // /// </summary>
        // private void Refresh()
        // {
        //     m_NotAddComponentData.Refresh();
        //     m_Page.SetAllCount(m_NotAddComponentData.BindComponents.Count);
        // }
        //
        // /// <summary>
        // /// 排序
        // /// </summary>
        // private void Sort()
        // {
        //     m_NotAddComponentData.Sort();
        //     m_Page.SetAllCount(m_NotAddComponentData.BindComponents.Count);
        // }
        //
        // /// <summary>
        // /// 全部删除
        // /// </summary>
        // private void RemoveAll()
        // {
        //     m_NotAddComponentData.RemoveAll();
        //     m_Page.SetAllCount(m_NotAddComponentData.BindComponents.Count);
        // }
        //
        // /// <summary>
        // /// 删除Missing Or Null
        // /// </summary>
        // private void RemoveNull()
        // {
        //     m_NotAddComponentData.RemoveNull();
        //     m_Page.SetAllCount(m_NotAddComponentData.BindComponents.Count);
        // }

        /// <summary>
        /// 自动绑定组件
        /// </summary>
        private void AutoBindComponent()
        {
            m_NotAddComponentData.RuleBindComponents();
            m_Page.SetAllCount(m_NotAddComponentData.BindComponents.Count);
        }

        #endregion
        
        #region 绑定对象信息

        private void DrawBindObjects()
        {
            int bindComNeedDeleteIndex = -1;

            EditorGUILayout.BeginVertical();
            int i = m_Page.CurrentPage * m_Page.ShowCount;


            for (i = 0; i < m_NotAddComponentData.BindComponents.Count; i++)
            {
                if (DrawBindObjectData(m_NotAddComponentData.BindComponents[i],i))
                {
                    bindComNeedDeleteIndex = i;
                }
            }
            
            if (bindComNeedDeleteIndex != -1)
            {
                m_NotAddComponentData.BindComponents.RemoveAt(bindComNeedDeleteIndex);
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
                //Refresh();
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

        #endregion
    }
}