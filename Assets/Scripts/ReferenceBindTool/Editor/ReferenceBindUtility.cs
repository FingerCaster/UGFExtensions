using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public static class ReferenceBindUtility
    {
        /// <summary>
        /// 设置代码生成配置
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="path"></param>
        /// <param name="isAutoCreateDir"></param>
        public static void SetBindSetting(string name, string nameSpace, string path, bool isAutoCreateDir)
        {
            string[] paths = AssetDatabase.FindAssets($"t:{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            if (paths.Length == 0)
            {
                Debug.LogError($"不存在{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
                return;
            }

            if (paths.Length > 1)
            {
                Debug.LogError($"{nameof(ReferenceBindCodeGeneratorSettingConfig)}数量大于1");
                return;
            }

            string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);

            if (!Directory.Exists(path) && isAutoCreateDir)
            {
                Directory.CreateDirectory(path);
            }

            var setting = AssetDatabase.LoadAssetAtPath<ReferenceBindCodeGeneratorSettingConfig>(settingPath);
            var settingData = setting.GetSettingData(name);
            settingData.Namespace = nameSpace;
            settingData.CodePath = path;

            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        ///   获取代码生成配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ReferenceBindCodeGeneratorSettingData GetAutoBindSetting(string name)
        {
            string[] paths = AssetDatabase.FindAssets($"t:{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            if (paths.Length == 0)
            {
                throw new Exception($"不存在{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            }

            if (paths.Length > 1)
            {
                throw new Exception($"{nameof(ReferenceBindCodeGeneratorSettingConfig)}数量大于1");
            }

            string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            var setting = AssetDatabase.LoadAssetAtPath<ReferenceBindCodeGeneratorSettingConfig>(settingPath);
            return setting.GetSettingData(name);
        }

        /// <summary>
        /// 添加代码生成配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static bool AddAutoBindSetting(string name, string folder, string nameSpace)
        {
            string[] paths = AssetDatabase.FindAssets($"t:{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            if (paths.Length == 0)
            {
                throw new Exception($"不存在{nameof(ReferenceBindCodeGeneratorSettingConfig)}");
            }

            if (paths.Length > 1)
            {
                throw new Exception($"{nameof(ReferenceBindCodeGeneratorSettingConfig)}数量大于1");
            }

            string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            var setting = AssetDatabase.LoadAssetAtPath<ReferenceBindCodeGeneratorSettingConfig>(settingPath);
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            bool result = setting.AddSettingData(new ReferenceBindCodeGeneratorSettingData(name, folder, nameSpace));
            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssets();
            return result;
        }
    }
}