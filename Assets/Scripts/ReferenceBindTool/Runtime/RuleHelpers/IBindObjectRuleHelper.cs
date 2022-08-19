
using UnityEngine;

namespace ReferenceBindTool
{
    public interface IBindAssetOrPrefabRuleHelper
    {
        /// <summary>
        /// 绑定符合规则的资源或预制体
        /// </summary>
        /// <param name="referenceBindComponent">引用绑定组件</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="obj">对象</param>
        void BindAssetOrPrefab(ReferenceBindComponent referenceBindComponent,string fieldName,Object obj);
    }
}