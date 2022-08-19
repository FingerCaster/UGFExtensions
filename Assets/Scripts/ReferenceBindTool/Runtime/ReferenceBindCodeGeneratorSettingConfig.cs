#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ReferenceBindTool
{
    /// <summary>
    /// 引用绑定绑定全局设置
    /// </summary>
    public class ReferenceBindCodeGeneratorSettingConfig : ScriptableObject
    {
        /// <summary>
        /// 默认配置名称
        /// </summary>
        private const string DefaultStr = "Default";

        /// <summary>
        /// 所有设置
        /// </summary>
        [SerializeField]
        private List<ReferenceBindCodeGeneratorSettingData> m_Settings =
            new List<ReferenceBindCodeGeneratorSettingData>();

        public List<ReferenceBindCodeGeneratorSettingData> Settings
        {
            get => m_Settings;
        }

        public ReferenceBindCodeGeneratorSettingData Default
        {
            get
            {
                var data = GetSettingData(DefaultStr);
                if (data == null)
                {
                    data = new ReferenceBindCodeGeneratorSettingData(DefaultStr);
                    m_Settings.Add(data);
                }

                return data;
            }
        }
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="settingName">配置名称</param>
        /// <returns></returns>
        public ReferenceBindCodeGeneratorSettingData GetSettingData(string settingName)
        {
            int index = m_Settings.FindIndex(_ => _.Name == settingName);
            if (index == -1)
            {
                return null;
            }

            return m_Settings[index];
        }
        
        /// <summary>
        /// 添加配置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool AddSettingData(ReferenceBindCodeGeneratorSettingData data)
        {
            int index = m_Settings.FindIndex(_ => _.Name == data.Name);
            if (index == -1)
            {
                m_Settings.Add(data);
                return true;
            }

            return false;
        }

        [MenuItem("Tools/ReferenceBindTools/CreateBindSettingConfig")]
        public static void CreateBindSettingConfig()
        {
            string[] paths = AssetDatabase.FindAssets($"t:{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在{nameof(ReferenceBindCodeGeneratorSettingConfig)}，路径:{path}", "确认");
                return;
            }

            ReferenceBindCodeGeneratorSettingConfig codeGeneratorSettingConfig =
                CreateInstance<ReferenceBindCodeGeneratorSettingConfig>();
            codeGeneratorSettingConfig.m_Settings.Add(new ReferenceBindCodeGeneratorSettingData(DefaultStr));

            AssetDatabase.CreateAsset(codeGeneratorSettingConfig, "Assets/BindSettingConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

#endif