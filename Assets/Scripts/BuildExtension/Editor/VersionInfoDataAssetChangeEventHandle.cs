using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGFExtensions.Build.Editor
{
    public class VersionInfoDataAssetChangeEventHandle : UnityEditor.AssetModificationProcessor
    {
        /// <summary>
        /// 监听(VersionInfoData)资源被删除事件
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="option">操作类型</param>
        /// <returns></returns>
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset is VersionInfoData)
            {
                foreach (string enumName in Enum.GetNames(typeof(VersionInfoData.EnvironmentType)))
                {
                    EditorPrefs.DeleteKey($"{enumName}InternalGameVersion");
                    EditorPrefs.DeleteKey($"{enumName}UpdatePrefixUri");
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}