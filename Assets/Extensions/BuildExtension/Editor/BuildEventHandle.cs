using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace UGFExtensions.Build.Editor
{
    public class BuildEventHandle : IBuildEventHandler
    {
        public bool ContinueOnFailure
        {
            get { return false; }
        }


        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            UnityGameFramework.Editor.ResourceTools.Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath,
            bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath,
            bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(UnityGameFramework.Editor.ResourceTools.Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath,
            bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(UnityGameFramework.Editor.ResourceTools.Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(UnityGameFramework.Editor.ResourceTools.Platform platform, string versionListPath, int versionListLength,
            int versionListHashCode, int versionListZipLength, int versionListZipHashCode)
        {
            Type resourceBuilderType =
                Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceBuilder,UnityGameFramework.Editor");
            var window = EditorWindow.GetWindow(resourceBuilderType);
            ResourceBuilderController builderController =
                window.GetType().GetField("m_Controller", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(window) as ResourceBuilderController;
            if (builderController == null)
            {
                return;
            }
            
            VersionInfoEditorData versionInfoEditorData =
                AssetDatabase.LoadAssetAtPath<VersionInfoEditorData>("Assets/Res/Configs/VersionInfoEditorData.asset");
            if (versionInfoEditorData == null)
            {
                versionInfoEditorData = ScriptableObject.CreateInstance<VersionInfoEditorData>();
                versionInfoEditorData.VersionInfos = new List<VersionInfoWrapData>
                    { new VersionInfoWrapData() { Key = "Normal", Value = new VersionInfoData() } };
                AssetDatabase.CreateAsset(versionInfoEditorData, "Assets/Res/Configs/VersionInfoEditorData.asset");
                Debug.Log("CreateVersionInfoEditorData Success!");
                AssetDatabase.Refresh();
                Selection.activeObject = versionInfoEditorData;
            }

            VersionInfoData versionInfoData = versionInfoEditorData.GetActiveVersionInfoData();
            VersionInfoData stagedVersionInfoData = versionInfoData.Clone();
            stagedVersionInfoData.ForceUpdateGame = false;
            stagedVersionInfoData.SetInternalGameVersion(versionInfoData.InternalGameVersion + 1);
            stagedVersionInfoData.ResourceVersion = builderController.ApplicableGameVersion.Replace('.', '_')+ "_"+builderController.InternalResourceVersion;
            stagedVersionInfoData.Platform = (Platform)(int)platform;
            stagedVersionInfoData.LatestGameVersion = builderController.ApplicableGameVersion;
            stagedVersionInfoData.InternalResourceVersion = builderController.InternalResourceVersion;
            stagedVersionInfoData.VersionListLength = versionListLength;
            stagedVersionInfoData.VersionListHashCode = versionListHashCode;
            stagedVersionInfoData.VersionListCompressedLength = versionListZipLength;
            stagedVersionInfoData.VersionListCompressedHashCode = versionListZipHashCode;

            string generatedJson = stagedVersionInfoData.ToVersionInfoJson();

            if (versionInfoEditorData.IsGenerateToFullPath)
            {
                string outputPath = Path.Combine(builderController.OutputFullPath, platform.ToString(), $"{platform}Version.txt");
                string outputDirectory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                File.WriteAllText(outputPath, generatedJson);
            }

            versionInfoData.ForceUpdateGame = stagedVersionInfoData.ForceUpdateGame;
            versionInfoData.SetInternalGameVersion(stagedVersionInfoData.InternalGameVersion);
            versionInfoData.ResourceVersion = stagedVersionInfoData.ResourceVersion;
            versionInfoData.Platform = stagedVersionInfoData.Platform;
            versionInfoData.LatestGameVersion = stagedVersionInfoData.LatestGameVersion;
            versionInfoData.InternalResourceVersion = stagedVersionInfoData.InternalResourceVersion;
            versionInfoData.VersionListLength = stagedVersionInfoData.VersionListLength;
            versionInfoData.VersionListHashCode = stagedVersionInfoData.VersionListHashCode;
            versionInfoData.VersionListCompressedLength = stagedVersionInfoData.VersionListCompressedLength;
            versionInfoData.VersionListCompressedHashCode = stagedVersionInfoData.VersionListCompressedHashCode;

            EditorUtility.SetDirty(versionInfoEditorData);
            AssetDatabase.SaveAssets();
        }

        public void OnPostprocessPlatform(UnityGameFramework.Editor.ResourceTools.Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath,
            bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath,
            bool isSuccess)
        {
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            UnityGameFramework.Editor.ResourceTools.Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath,
            bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath,
            bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }
        
        
    }
}
