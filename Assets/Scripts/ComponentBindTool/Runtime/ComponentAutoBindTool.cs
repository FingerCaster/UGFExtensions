using System;
using System.Collections.Generic;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 组件自动绑定工具
/// </summary>
public class ComponentAutoBindTool : MonoBehaviour
{
#if UNITY_EDITOR
    [Serializable]
    public class BindData
    {
        public BindData()
        {
        }
        
        public BindData(bool isRepeatName, string name, Component bindCom)
        {
            IsRepeatName = isRepeatName;
            Name = name;
            BindCom = bindCom;
        }

        public bool IsRepeatName;
        public string Name;
        public Component BindCom;
    }

    public List<BindData> BindDatas = new List<BindData>();

    [SerializeField]
    private string m_ClassName;

    [SerializeField]
    private AutoBindSettingData m_SettingData;
    
    [SerializeField]
    private SearchableData m_Searchable;

    public SearchableData Searchable
    {
        get => m_Searchable;
        set => m_Searchable = value;
    }

    public AutoBindSettingData SettingData
    {
        get => m_SettingData;
        set => m_SettingData = value;
    }

    public string ClassName
    {
        get => m_ClassName;
        set => m_ClassName = value;
    }
    public IAutoBindRuleHelper RuleHelper
    {
        get;
        set;
    }
#endif
    [SerializeField]
    public List<Component> m_BindComs = new List<Component>();
    public T GetBindComponent<T>(int index) where T : Component
    {
        if (index >= m_BindComs.Count)
        {
            Debug.LogError("索引无效");
            return null;
        }

        T bindCom = m_BindComs[index] as T;

        if (bindCom == null)
        {
            Debug.LogError("类型无效");
            return null;
        }

        return bindCom;
    }
}
