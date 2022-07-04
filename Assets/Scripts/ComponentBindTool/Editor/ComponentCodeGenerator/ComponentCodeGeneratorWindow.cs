using System.Linq;
using RoboRyanTron.SearchableEnum;
using UnityEditor;
using UnityEngine;

namespace ComponentCodeGenerator
{
    public class ComponentCodeGeneratorWindow : EditorWindow
    {
        private ComponentCodeGenData m_ComponentCodeGenData;

        private string[] m_HelperTypeNames;
        private int m_HelperTypeNameIndex;


        private AutoBindSettingConfig m_SettingConfig;
        private bool m_SettingDataExpanded = true;
        private int m_LastSettingDataNameIndex;
        private bool m_SettingDataError;

        [SerializeField] private SearchableData m_Searchable;
        private SerializedProperty m_SearchableSerializedProperty;


        [MenuItem("Tools/ComponentCodeGenerator")]
        private static void ShowWindow()
        {
            var window = GetWindow<ComponentCodeGeneratorWindow>();
            window.titleContent = new GUIContent("Component CodeGenerator");
            window.Show();
        }

        private void OnEnable()
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
            m_SettingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
            if (m_SettingConfig.Settings.Count == 0)
            {
                Debug.LogError("不存在AutoBindSettingData");
                return;
            }

            var settingDataNames = m_SettingConfig.Settings.Select(_ => _.Name).ToArray();
            m_Searchable = new SearchableData() {Select = 0, Names = settingDataNames};
            m_SearchableSerializedProperty = new SerializedObject(this).FindProperty("m_Searchable");
        }

        private void Init(GameObject gameObject)
        {
            m_ComponentCodeGenData = new ComponentCodeGenData();
            m_ComponentCodeGenData.GameObject = gameObject;
            m_HelperTypeNames = ComponentAutoBindToolUtility.GetTypeNames();
            m_SettingDataError = false;


            if (m_ComponentCodeGenData.SettingData == null)
            {
                m_ComponentCodeGenData.SetSettingData(m_SettingConfig.Settings[0]);
            }
            else
            {
                var data = m_SettingConfig.GetSettingData(m_ComponentCodeGenData.SettingData.Name);
                if (data == null)
                {
                    Debug.LogError($"不存在名为‘{m_ComponentCodeGenData.SettingData.Name}’的AutoBindSettingData");
                    m_SettingDataError = true;
                    return;
                }

                m_ComponentCodeGenData.SetSettingData(
                    m_SettingConfig.GetSettingData(m_ComponentCodeGenData.SettingData.Name));
            }

            m_ComponentCodeGenData.SetClassName(string.IsNullOrEmpty(m_ComponentCodeGenData.ClassName)
                ? m_ComponentCodeGenData.GameObject.name
                : m_ComponentCodeGenData.ClassName);
            if (string.IsNullOrEmpty(m_ComponentCodeGenData.RuleHelperTypeName))
            {
                m_ComponentCodeGenData.SetRuleHelperTypeName(nameof(DefaultAutoBindRuleHelper));
            }
            else
            {
                m_ComponentCodeGenData.SetRuleHelperTypeName(m_ComponentCodeGenData.RuleHelperTypeName);
            }
            SetPage();
        }

        private GameObject m_LastGameObject;
        private GameObject m_CurrentGameObject;


        private void OnGUI()
        {
            m_CurrentGameObject = (GameObject) EditorGUILayout.ObjectField("GameObject",
                m_CurrentGameObject, typeof(GameObject), true);

            if (m_CurrentGameObject != m_LastGameObject)
            {
                m_LastGameObject = m_CurrentGameObject;
                Init(m_CurrentGameObject);
            }

            if (m_ComponentCodeGenData == null)
            {
                if (m_CurrentGameObject != null)
                {
                    Init(m_CurrentGameObject);
                }
                return;
            }

            DrawButtons();
            DrawHelperSelect();
            DrawSetting();

            DrawComponents();
            DrawPage();
            
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("PreView Components"))
            {
                m_ComponentCodeGenData.AutoBindComponent();
                SetPage();
            }

            if (GUILayout.Button("Generator"))
            {
                string className = !string.IsNullOrEmpty(m_ComponentCodeGenData.ClassName)
                    ? m_ComponentCodeGenData.ClassName
                    : m_ComponentCodeGenData.GameObject.name;
                m_ComponentCodeGenData.AutoBindComponent();
                ComponentCodeGeneratorUtility.GenAutoBindCode(m_ComponentCodeGenData, className, m_ComponentCodeGenData.SettingData.CodePath);
            }
        }

        private void DrawComponents()
        {
            EditorGUILayout.BeginVertical();
            int i = m_CurrentPage * m_ShowCount;
            int count = i + m_ShowCount;
            if (count > m_ComponentCodeGenData.BindDataList.Count)
            {
                count = m_ComponentCodeGenData.BindDataList.Count;
            }

            for (; i < count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(50));
                GUI.enabled = false;
                EditorGUILayout.TextField(m_ComponentCodeGenData.BindDataList[i].Name, GUILayout.Width(150));
                m_ComponentCodeGenData.BindDataList[i].BindCom =
                    (Component) EditorGUILayout.ObjectField(m_ComponentCodeGenData.BindDataList[i].BindCom,
                        typeof(Component), true);
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();
                if (m_ComponentCodeGenData.BindDataList[i].IsRepeatName)
                {
                    EditorGUILayout.HelpBox("组件命名不能相同 请修改!", MessageType.Error);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private int m_CurrentPage = 0;
        private int m_ShowCount = 10;
        private int m_AllPage = 0;

        private string m_PageField;

        private void DrawPage()
        {
            EditorGUILayout.BeginHorizontal();
            if (m_CurrentPage <= 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("上一页"))
            {
                m_CurrentPage--;
            }

            GUI.enabled = true;

            m_PageField = m_CurrentPage.ToString();
            m_PageField = EditorGUILayout.TextField(m_PageField, new GUIStyle("TextField")
            {
                alignment = TextAnchor.MiddleCenter
            }, GUILayout.MaxWidth(50));

            if (int.TryParse(m_PageField, out int page) && page < m_AllPage - 1)
            {
                m_CurrentPage = page;
            }
            else
            {
                m_PageField = m_CurrentPage.ToString();
            }

            if (m_CurrentPage >= (m_AllPage - 1))
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("下一页"))
            {
                m_CurrentPage++;
            }

            GUI.enabled = true;


            EditorGUILayout.EndHorizontal();
        }

        private void SetPage()
        {
            m_AllPage = m_ComponentCodeGenData.BindDataList.Count / m_ShowCount;
            if (m_ComponentCodeGenData.BindDataList.Count % m_ShowCount != 0)
            {
                m_AllPage += 1;
            }

            m_CurrentPage = 0;
            m_PageField = m_CurrentPage.ToString();
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
                EditorGUILayout.HelpBox($"不存在名为‘{m_ComponentCodeGenData.SettingData.Name}’的AutoBindSettingData",
                    MessageType.Error);
                if (GUILayout.Button($"创建 {m_ComponentCodeGenData.SettingData.Name} 绑定配置"))
                {
                    bool result = ComponentAutoBindToolUtility.AddAutoBindSetting(m_ComponentCodeGenData.SettingData.Name, "", "");
                    if (!result)
                    {
                        EditorUtility.DisplayDialog("创建配置", "创建代码自动生成配置失败，请检查配置信息！", "确定");
                        return;
                    }

                    m_ComponentCodeGenData.SetSettingData(m_ComponentCodeGenData.SettingData.Name);
                    ReloadSearchableData(m_ComponentCodeGenData.SettingData.Name);
                    m_SettingDataError = false;
                }

                if (GUILayout.Button("使用默认配置"))
                {
                    m_ComponentCodeGenData.SetSettingData(m_SettingConfig.Default);
                    ReloadSearchableData(m_SettingConfig.Default.Name);
                    m_SettingDataError = false;
                }

                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_SearchableSerializedProperty);
            if (m_Searchable.Select != m_LastSettingDataNameIndex)
            {
                if (m_Searchable.Select >= m_SettingConfig.Settings.Count)
                {
                    m_SettingDataError = true;
                    return;
                }

                m_ComponentCodeGenData.SetSettingData(m_SettingConfig.Settings[m_Searchable.Select]);
                m_ComponentCodeGenData.SetClassName(string.IsNullOrEmpty(m_ComponentCodeGenData.ClassName)
                    ? m_ComponentCodeGenData.GameObject.name
                    : m_ComponentCodeGenData.ClassName);
                m_LastSettingDataNameIndex = m_Searchable.Select;
            }

            if (GUILayout.Button("Reload",GUILayout.Width(80)))
            {
                ReloadSearchableData(m_Searchable.Names[m_Searchable.Select]);
            }
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("命名空间：");
            EditorGUILayout.LabelField(m_ComponentCodeGenData.SettingData.Namespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_ComponentCodeGenData.SetClassName(EditorGUILayout.TextField(new GUIContent("类名："),
                m_ComponentCodeGenData.ClassName));
            if (GUILayout.Button("物体名"))
            {
                m_ComponentCodeGenData.SetClassName(m_ComponentCodeGenData.GameObject.name);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("代码保存路径：");
            EditorGUILayout.LabelField(m_ComponentCodeGenData.SettingData.CodePath);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(m_ComponentCodeGenData.SettingData.CodePath))
            {
                EditorGUILayout.HelpBox("代码保存路径不能为空!", MessageType.Error);
            }
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
            m_SettingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
            if (m_SettingConfig.Settings.Count == 0)
            {
                Debug.LogError("不存在AutoBindSettingData");
                return;
            }
            var settingDataNames = m_SettingConfig.Settings.Select(_ => _.Name).ToList();

            int findIndex = m_SettingConfig.Settings.FindIndex(_ => _ == m_ComponentCodeGenData.SettingData);
            if (findIndex == -1)
            {
                m_SettingDataError = true;
            }
            else
            {
                m_Searchable.Select = findIndex;
                m_Searchable.Names = settingDataNames.ToArray();
                m_SearchableSerializedProperty = new SerializedObject(this).FindProperty("m_Searchable");
            }
        }

        /// <summary>
        /// 绘制辅助器选择框
        /// </summary>
        private void DrawHelperSelect()
        {
            if (m_ComponentCodeGenData.RuleHelper != null)
            {
                for (int i = 0; i < m_HelperTypeNames.Length; i++)
                {
                    if (m_ComponentCodeGenData.RuleHelperTypeName == m_HelperTypeNames[i])
                    {
                        m_HelperTypeNameIndex = i;
                    }
                }
            }
            else
            {
                m_ComponentCodeGenData.SetRuleHelperTypeName(m_ComponentCodeGenData.RuleHelperTypeName);
            }
            
            int selectedIndex = EditorGUILayout.Popup("AutoBindRuleHelper", m_HelperTypeNameIndex, m_HelperTypeNames);
            if (selectedIndex != m_HelperTypeNameIndex)
            {
                m_HelperTypeNameIndex = selectedIndex;
                m_ComponentCodeGenData.SetRuleHelperTypeName(m_HelperTypeNames[selectedIndex]);
            }
        }
    }
}