using System.ComponentModel.Design;
using System.Text.RegularExpressions;
using ReferenceBindTool.Editor.RuleHelpers;
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
        
        public string GetDefaultFieldName(Object obj)
        {
            string filedName = $"{obj.GetType().Name}_{obj.name}".Replace(' ', '_');
            return filedName;
        }

        public bool CheckFieldNameIsInvalid(string fieldName)
        {
            string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            return !Regex.IsMatch(fieldName, regex);
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