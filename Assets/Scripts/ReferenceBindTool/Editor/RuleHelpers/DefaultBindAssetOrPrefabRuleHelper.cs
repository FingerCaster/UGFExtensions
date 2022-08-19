using System.ComponentModel.Design;
using UnityEditor;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class DefaultBindAssetOrPrefabRuleHelper : IBindAssetOrPrefabRuleHelper
    {
        /// <summary>
        /// 检查引用是否可以添加
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckIsCanAdd(UnityEngine.Object obj)
        {
            bool isFolder = obj is DefaultAsset && ProjectWindowUtil.IsFolder(obj.GetInstanceID());
            return !isFolder;
        }
        public void BindAssetOrPrefab(ReferenceBindComponent referenceBindComponent, string fieldName, Object obj)
        {
            if (!CheckIsCanAdd(obj))
            {
                Debug.LogError("不能添加目录!");
                return;
            }    
            referenceBindComponent.AddBindAssetsOrPrefabs(fieldName,obj);
        }
    }
}