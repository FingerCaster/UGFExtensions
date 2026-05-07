using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameFramework;
using GameFramework.FileSystem;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace UGFExtensions
{
    public sealed class FileSystemHelper : IFileSystemHelper
    {
        public FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            return new CommonFileSystemStream(fullPath, access, createNew);
        }
    }
    public static class MergeAssetUtility
    {
        public static void Merge(string fileSystemPath, List<AssetData> assetDataList, string searchPatterns)
        {
            fileSystemPath = ValidateFileSystemPath(fileSystemPath);
            List<AssetData> tempList = new List<AssetData>(assetDataList.Count);

            foreach (AssetData assetData in assetDataList)
            {
                ValidateAssetData(assetData);
                if (assetData.AssetType == AssetType.Folder)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(GetFullAssetPath(assetData.AssetPath));
                    var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                        .Where(_ => !_.FullName.EndsWith(".meta"))
                        .Select(_ => Utility.Path.GetRegularPath(_.FullName))
                        .Select(_ => ToAssetPath(_))
                        .ToArray();

                    if (files.Length == 0)
                    {
                        continue;
                    }

                    string[] patterns = (searchPatterns ?? string.Empty).Split(';', ',', '|');
                    
                    foreach (var pattern in patterns)
                    {
                        var assetPaths = files.Where(_ =>
                        {
                            if (string.IsNullOrEmpty(pattern))
                            {
                                return true;
                            }

                            return _.EndsWith(pattern);
                        }).ToArray();

                        foreach (string assetPath in assetPaths)
                        {
                            ValidateAssetPath(assetPath);
                            var newAssetData = new AssetData
                            {
                                AssetPath = assetPath
                            };
                            newAssetData.AssetName =
                                newAssetData.AssetPath.Substring(newAssetData.AssetPath.LastIndexOf('/') + 1);
                            newAssetData.Asset = AssetDatabase.LoadAssetAtPath(newAssetData.AssetPath, typeof(Object));
                            newAssetData.AssetType = GetAssetType(newAssetData.Asset);
                            tempList.Add(newAssetData);
                        }
                    }

                    continue;
                }

                if (assetData.Asset != null)
                {
                    tempList.Add(assetData);
                }
            }

            var fileSystemManager = GameFrameworkEntry.GetModule<IFileSystemManager>();
            fileSystemManager.SetFileSystemHelper(new FileSystemHelper());
            var fileSystem = fileSystemManager.CreateFileSystem(fileSystemPath, FileSystemAccess.ReadWrite,
                tempList.Count, tempList.Count * 8);
            foreach (var assetData in tempList)
            {
                ValidateAssetData(assetData);
                fileSystem.WriteFile(assetData.AssetPath, Asset2Bytes(assetData));
            }

            fileSystemManager.DestroyFileSystem(fileSystem, false);
            AssetDatabase.Refresh();
        }

        public static byte[] Asset2Bytes(AssetData assetData)
        {
            switch (assetData.AssetType)
            {
                case AssetType.None:
                case AssetType.Folder:
                    return null;
                case AssetType.Text:
                    return ((TextAsset) assetData.Asset).bytes;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static AssetType GetAssetType(UnityEngine.Object obj)
        {
            if (obj is TextAsset)
            {
                return AssetType.Text;
            }

            if (obj is DefaultAsset && ProjectWindowUtil.IsFolder(obj.GetInstanceID()))
            {
                return AssetType.Folder;
            }

            return AssetType.None;
        }
        
        public static string GetTypeTips()
        {
            string tips = string.Empty;
            var typeNames = Enum.GetNames(typeof(AssetType));
            for (var index = 1; index < typeNames.Length; index++)
            {
                string assetTypeName = typeNames[index];
                if (index == typeNames.Length-1)
                {
                    tips += assetTypeName;
                }
                else
                {
                    tips += assetTypeName + ", ";
                }
            }

            return tips;
        }
        
        /// <summary>
        /// 选择VFS文件生成位置
        /// </summary>
        /// <returns>VFS文件生成位置</returns>
        public static string ChooseVfsFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Save VFS Folder", Application.dataPath,"");
            return path;
        }
        
        public static void SaveScriptableObject(MergeAssetScriptableObject mergeAssetScriptableObject,string path)
        {
            path = ValidateConfigAssetPath(path);
            if (mergeAssetScriptableObject == null)
            {
                throw new Exception("MergeAssetScriptableObject can not be null.");
            }
            if (AssetDatabase.LoadAssetAtPath<MergeAssetScriptableObject>(path) == null)
            {
                AssetDatabase.CreateAsset(mergeAssetScriptableObject, path);
            }
            else
            {
                EditorUtility.SetDirty(mergeAssetScriptableObject);
            }
            AssetDatabase.SaveAssets();
        }

        private static string ValidateFileSystemPath(string path)
        {
            if (string.IsNullOrEmpty(path) || Path.GetExtension(path) != ".dat")
            {
                throw new Exception("VFS output path is invalid.");
            }

            string fullPath = Path.GetFullPath(path);
            string fullDataPath = Path.GetFullPath(Application.dataPath);
            string fullProjectPath = Directory.GetParent(fullDataPath).FullName;
            if (!IsPathInRoot(fullPath, fullDataPath) && !IsPathInRoot(fullPath, fullProjectPath))
            {
                throw new Exception("VFS output path must be inside project path.");
            }

            return Utility.Path.GetRegularPath(fullPath);
        }

        private static string ValidateConfigAssetPath(string path)
        {
            path = Utility.Path.GetRegularPath(path);
            ValidateAssetPath(path);
            if (Path.GetExtension(path) != ".asset")
            {
                throw new Exception("MergeAssetScriptableObject save path must be an asset path.");
            }

            return path;
        }

        private static void ValidateAssetData(AssetData assetData)
        {
            if (assetData == null)
            {
                throw new Exception("AssetData can not be null.");
            }

            ValidateAssetPath(assetData.AssetPath);
            if (assetData.AssetType != AssetType.Folder && assetData.Asset == null)
            {
                throw new Exception($"Asset '{assetData.AssetPath}' is invalid.");
            }
        }

        private static void ValidateAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                assetPath.Contains("..") || Path.IsPathRooted(assetPath))
            {
                throw new Exception($"Asset path '{assetPath}' is invalid.");
            }

            string fullPath = GetFullAssetPath(assetPath);
            if (!IsPathInRoot(fullPath, Application.dataPath))
            {
                throw new Exception($"Asset path '{assetPath}' is outside Assets.");
            }
        }

        private static string GetFullAssetPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length)));
        }

        private static string ToAssetPath(string fullPath)
        {
            string regularFullPath = Utility.Path.GetRegularPath(Path.GetFullPath(fullPath));
            string regularDataPath = Utility.Path.GetRegularPath(Path.GetFullPath(Application.dataPath));
            if (!regularFullPath.StartsWith(regularDataPath + "/", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Asset path '{fullPath}' is outside Assets.");
            }

            return "Assets/" + regularFullPath.Substring(regularDataPath.Length + 1);
        }

        private static bool IsPathInRoot(string path, string root)
        {
            string fullPath = Path.GetFullPath(path);
            string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }
        
    }
}
