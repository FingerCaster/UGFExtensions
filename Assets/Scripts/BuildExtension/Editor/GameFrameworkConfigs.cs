

using GameFramework;
using System.IO;
using UnityEngine;
using UnityGameFramework.Editor;
using UnityGameFramework.Editor.ResourceTools;

namespace UGFExtensions.Build.Editor
{
    public static class GameFrameworkConfigs
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/BuildSettings.xml"));

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/ResourceCollection.xml"));

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/ResourceEditor.xml"));

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Res/Configs/ResourceBuilder.xml"));
    }
}

