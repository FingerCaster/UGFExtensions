using UnityEngine;
using UnityEditor;
using System.Linq;
using RoboRyanTron.SearchableEnum;

[CustomEditor(typeof(ComponentAutoBindTool))]
public class ComponentAutoBindToolInspector : Editor
{
    private ComponentAutoBindTool m_Target;
    private string[] m_HelperTypeNames;
    private string m_HelperTypeName;
    private int m_HelperTypeNameIndex;


    private AutoBindSettingConfig m_SettingConfig;
    private bool m_SettingDataExpanded = true;
    private SerializedProperty m_Searchable;
    private int m_LastSettingDataNameIndex;
    private void OnEnable()
    {
        m_Target = (ComponentAutoBindTool)target;
        m_HelperTypeNames = ComponentAutoBindToolUtility.GetTypeNames();

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
                return;
            }
            m_Target.SetSettingData(m_SettingConfig.GetSettingData(m_Target.SettingData.Name));
            m_LastSettingDataNameIndex = m_SettingConfig.Settings.FindIndex(_ => _.Name == m_Target.SettingData.Name);
        }
        m_Target.SetSearchable(new SearchableData()
        {
            Select = m_LastSettingDataNameIndex,
            Names = settingDataNames
        });
        m_Searchable = serializedObject.FindProperty("m_Searchable");
       
        m_Target.SetClassName(string.IsNullOrEmpty(m_Target.ClassName) ? m_Target.gameObject.name : m_Target.ClassName);
        serializedObject.ApplyModifiedProperties();
        SetPage();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawTopButton();

        DrawHelperSelect();

        DrawSetting();

        DrawKvData();

        DrawPage();
        serializedObject.ApplyModifiedProperties();
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

        if (GUILayout.Button("全部删除"))
        {
            RemoveAll();
        }

        if (GUILayout.Button("删除空引用"))
        {
            RemoveNull();
        }

        if (GUILayout.Button("自动绑定组件"))
        {
            AutoBindComponent();
        }

        if (GUILayout.Button("生成绑定代码"))
        {
            string className = !string.IsNullOrEmpty(m_Target.ClassName)
                ? m_Target.ClassName
                : m_Target.gameObject.name;
            
            ComponentAutoBindToolUtility.GenAutoBindCode(m_Target, className, m_Target.SettingData.CodePath);
        }

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 排序
    /// </summary>
    private void Sort()
    {
        m_Target.Sort();
        SetPage();
    }

    /// <summary>
    /// 全部删除
    /// </summary>
    private void RemoveAll()
    {
        m_Target.RemoveAll();
        SetPage();
    }

    /// <summary>
    /// 删除空引用
    /// </summary>
    private void RemoveNull()
    {
        m_Target.RemoveNull();
        SetPage();
    }

    /// <summary>
    /// 自动绑定组件
    /// </summary>
    private void AutoBindComponent()
    {
        m_Target.AutoBindComponent();
        SetPage();
    }

    /// <summary>
    /// 绘制辅助器选择框
    /// </summary>
    private void DrawHelperSelect()
    {
        m_HelperTypeName = m_HelperTypeNames[0];

        if (m_Target.RuleHelper != null)
        {
            m_HelperTypeName = m_Target.RuleHelper.GetType().Name;

            for (int i = 0; i < m_HelperTypeNames.Length; i++)
            {
                if (m_HelperTypeName == m_HelperTypeNames[i])
                {
                    m_HelperTypeNameIndex = i;
                }
            }
        }
        else
        {
            IAutoBindRuleHelper helper =
                (IAutoBindRuleHelper)ComponentAutoBindToolUtility.CreateHelperInstance(m_HelperTypeName);
            m_Target.SetRuleHelper(helper);
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();
            if (autoBindTool == null)
            {
                continue;
            }

            if (autoBindTool.RuleHelper == null)
            {
                IAutoBindRuleHelper helper =
                    (IAutoBindRuleHelper)ComponentAutoBindToolUtility.CreateHelperInstance(m_HelperTypeName);
                autoBindTool.SetRuleHelper(helper);
            }
        }

        int selectedIndex = EditorGUILayout.Popup("AutoBindRuleHelper", m_HelperTypeNameIndex, m_HelperTypeNames);
        if (selectedIndex != m_HelperTypeNameIndex)
        {
            m_HelperTypeNameIndex = selectedIndex;
            m_HelperTypeName = m_HelperTypeNames[selectedIndex];
            IAutoBindRuleHelper helper =
                (IAutoBindRuleHelper)ComponentAutoBindToolUtility.CreateHelperInstance(m_HelperTypeName);
            m_Target.SetRuleHelper(helper);
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
        
        EditorGUILayout.PropertyField(m_Searchable);
        if (m_Target.Searchable.Select!= m_LastSettingDataNameIndex)
        {
            m_Target.SetSettingData(m_SettingConfig.Settings[m_Target.Searchable.Select]);
            m_Target.SetClassName(string.IsNullOrEmpty(m_Target.ClassName)
                ? m_Target.gameObject.name
                : m_Target.ClassName);
        }

        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.PrefixLabel("命名空间：");
        EditorGUILayout.LabelField(m_Target.SettingData.Namespace);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        m_Target.SetClassName(EditorGUILayout.TextField(new GUIContent("类名："), m_Target.ClassName));

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
            EditorGUILayout.HelpBox("代码保存路径不能为空!",MessageType.Error);
        }
    }

    /// <summary>
    /// 绘制键值对数据
    /// </summary>
    private void DrawKvData()
    {
        //绘制key value数据

        int needDeleteIndex = -1;

        EditorGUILayout.BeginVertical();
        int i = m_CurrentPage * m_ShowCount;
        int count = i + m_ShowCount;
        if (count > m_Target.BindDatas.Count)
        {
            count = m_Target.BindDatas.Count;
        }

        for (; i < count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(50));
            string lastName = m_Target.BindDatas[i].Name;
            Component lastComponent = m_Target.BindDatas[i].BindCom;
            m_Target.BindDatas[i].Name = EditorGUILayout.TextField(m_Target.BindDatas[i].Name, GUILayout.Width(150));
            m_Target.BindDatas[i].BindCom =
                (Component)EditorGUILayout.ObjectField(m_Target.BindDatas[i].BindCom, typeof(Component), true);
            if (m_Target.BindDatas[i].Name != lastName || m_Target.BindDatas[i].BindCom != lastComponent)
            {
                EditorUtility.SetDirty(m_Target);
            }

            if (GUILayout.Button("X"))
            {
                //将元素下标添加进删除list
                needDeleteIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        //删除data
        if (needDeleteIndex != -1)
        {
            m_Target.BindDatas.RemoveAt(needDeleteIndex);
            m_Target.SyncBindComponents();
            SetPage();
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
        m_AllPage = m_Target.BindDatas.Count / m_ShowCount;
        if (m_Target.BindDatas.Count % m_ShowCount != 0)
        {
            m_AllPage += 1;
        }

        m_CurrentPage = 0;
        m_PageField = m_CurrentPage.ToString();
    }
}