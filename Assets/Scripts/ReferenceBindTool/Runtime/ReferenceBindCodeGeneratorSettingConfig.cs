#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ReferenceBindTool
{
    /// <summary>
    /// 自动绑定全局设置
    /// </summary>
    public class ReferenceBindCodeGeneratorSettingConfig : ScriptableObject
    {
        private const string DefaultStr = "Default";

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

        public ReferenceBindCodeGeneratorSettingData GetSettingData(string settingName)
        {
            int index = m_Settings.FindIndex(_ => _.Name == settingName);
            if (index == -1)
            {
                return null;
            }

            return m_Settings[index];
        }

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

        [MenuItem("Tools/AutoBindTools/CreateAutoBindSettingConfig")]
        public static void CreateAutoBindSettingConfig()
        {
            string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
            if (paths.Length >= 1)
            {
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                EditorUtility.DisplayDialog("警告", $"已存在AutoBindSettingConfig，路径:{path}", "确认");
                return;
            }

            ReferenceBindCodeGeneratorSettingConfig codeGeneratorSettingConfig =
                CreateInstance<ReferenceBindCodeGeneratorSettingConfig>();
            codeGeneratorSettingConfig.m_Settings.Add(new ReferenceBindCodeGeneratorSettingData(DefaultStr));

            AssetDatabase.CreateAsset(codeGeneratorSettingConfig, "Assets/AutoBindSettingConfig.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}

#endif