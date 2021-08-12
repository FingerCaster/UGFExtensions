using System;
using System.IO;
using System.Reflection;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor;
using UnityGameFramework.Editor.ResourceTools;
namespace  UGFExtensions.Build.Editor
{

    public class BuildEventHandle : IBuildEventHandler
    {
        public bool ContinueOnFailure
        {
            get { return false; }
        }
        

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath,
            bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath,
            bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            string streamingAssetsPath = GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
            }
            
            string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains(".gitkeep"))
                {
                    continue;
                }
            
                File.Delete(fileName);
            }

            GameFramework.Utility.Path.RemoveEmptyDirectory(streamingAssetsPath);
            if (!Directory.Exists(streamingAssetsPath))
            {
                Directory.CreateDirectory(streamingAssetsPath);
            }
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath,
            bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
           
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength,
            int versionListHashCode, int versionListZipLength, int versionListZipHashCode)
        {
            Debug.Log(
                $"platform:{platform}  workingPath:{versionListPath}  outputPackageSelected:{versionListLength}  " +
                $"outputPackagePath:{versionListHashCode}  versionListZipLength:{versionListZipLength} versionListZipHashCode:{versionListZipHashCode}");

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

            VersionInfoData versionInfoData =
                AssetDatabase.LoadAssetAtPath<VersionInfoData>("Assets/Res/Configs/VersionInfo.asset");
            if (versionInfoData == null)
            {
                versionInfoData = ScriptableObject.CreateInstance<VersionInfoData>();
                AssetDatabase.CreateAsset(versionInfoData, "Assets/Res/Configs/VersionInfo.asset");
            }
            else
            {
                versionInfoData.AutoIncrementInternalGameVersion();
            }

            versionInfoData.ForceUpdateGame = false;
            versionInfoData.LatestGameVersion = builderController.ApplicableGameVersion;
            versionInfoData.InternalResourceVersion = builderController.InternalResourceVersion;
            versionInfoData.VersionListLength = versionListLength;
            versionInfoData.VersionListHashCode = versionListHashCode;
            versionInfoData.VersionListCompressedLength = versionListZipLength;
            versionInfoData.VersionListCompressedHashCode = versionListZipHashCode;
            AssetDatabase.SaveAssets();

            if (versionInfoData.IsGenerateToFullPath)
            {
                File.WriteAllText(Path.Combine(builderController.OutputFullPath,platform.ToString(),"version.txt"),versionInfoData.ToVersionInfoJson());
            }
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath,
            bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            // if (platform != Platform.Windows)
            // {
            //     return;
            // }

            string streamingAssetsPath =
                GameFramework.Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(outputPackagePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = GameFramework.Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath,
                    fileName.Substring(outputPackagePath.Length)));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }
            
                File.Copy(fileName, destFileName);
            }
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath,
            bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath,
            bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            
        }
        
    }
}