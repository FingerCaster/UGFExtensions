#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AutoBindSettingData
{
    [SerializeField]
    private string m_Name;
    [SerializeField]
    private string m_CodeFolderPath;
    [SerializeField] 
    private string m_Namespace;
    [SerializeField] 
    private bool m_IsShow = false;

    public AutoBindSettingData(string name)
    {
        m_Name = name;
    }

    public AutoBindSettingData(string name, string codeFolderPath, string ns)
    {
        m_Name = name;
        m_CodeFolderPath = codeFolderPath;
        m_Namespace = ns;
    }

    public string Name => m_Name;
    
    public string CodePath
    {
        get => m_CodeFolderPath;

        set => m_CodeFolderPath = value;
    }

    public string Namespace
    {
        get => m_Namespace;

        set => m_Namespace = value;
    }
}
#endif