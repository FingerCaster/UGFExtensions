using System;
using System.Collections.Generic;
using UnityEngine;
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

        public BindData(string name, Component bindCom)
        {
            Name = name;
            BindCom = bindCom;
        }

        public string Name;
        public Component BindCom;
    }

    public List<BindData> BindDatas = new List<BindData>();

    [SerializeField]
    private string m_ClassName;

    [SerializeField]
    private string m_Namespace;

    [SerializeField]
    private string m_CodePath;

    public string ClassName
    {
        get => m_ClassName;
        set => m_ClassName = value;
    }

    public string Namespace
    {
        get => m_Namespace;
        set => m_Namespace = value;
    }

    public string CodePath
    {
        get => m_CodePath;
        set => m_CodePath = value;
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
