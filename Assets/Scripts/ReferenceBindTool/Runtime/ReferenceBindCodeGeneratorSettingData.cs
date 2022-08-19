#if UNITY_EDITOR
using System;
using UnityEngine;

namespace ReferenceBindTool
{
    [Serializable]
    public class ReferenceBindCodeGeneratorSettingData
    {
        [SerializeField] private string m_Name;
        [SerializeField] private string m_CodeFolderPath;
        [SerializeField] private string m_Namespace;
        [SerializeField] private bool m_IsShow = false;

        public ReferenceBindCodeGeneratorSettingData(string name)
        {
            m_Name = name;
        }

        public ReferenceBindCodeGeneratorSettingData(string name, string codeFolderPath, string nameSpace)
        {
            m_Name = name;
            m_CodeFolderPath = codeFolderPath;
            m_Namespace = nameSpace;
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
}

#endif