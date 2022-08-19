#if UNITY_EDITOR
using System;
using UnityEngine;

namespace ReferenceBindTool
{
    /// <summary>
    /// 引用绑定工具代码生成配置数据类
    /// </summary>
    [Serializable]
    public class ReferenceBindCodeGeneratorSettingData
    {
        /// <summary>
        /// 配置名
        /// </summary>
        [SerializeField] private string m_Name;
        /// <summary>
        /// 代码生成目录
        /// </summary>
        [SerializeField] private string m_CodeFolderPath;
        /// <summary>
        /// 命名空间
        /// </summary>
        [SerializeField] private string m_Namespace;
        /// <summary>
        /// 是否展开
        /// </summary>
        [SerializeField] private bool m_IsExpand = false;

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