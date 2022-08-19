
using UnityEngine;

namespace ReferenceBindTool
{
    public interface IBindAssetOrPrefabRuleHelper
    {
        /// <summary>
        /// 获取默认字段名
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>默认字段名</returns>
        string GetDefaultFieldName(Object obj);

        /// <summary>
        /// 检查字段名称是否无效
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>字段名是否无效</returns>
        bool CheckFieldNameIsInvalid(string fieldName);

        /// <summary>
        /// 绑定符合规则的资源或预制体
        /// </summary>
        /// <param name="referenceBindComponent">引用绑定组件</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="obj">对象</param>
        void BindAssetOrPrefab(ReferenceBindComponent referenceBindComponent,string fieldName,Object obj);
    }
}