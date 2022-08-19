using UnityEngine;

namespace ReferenceBindTool
{
    public interface INameRuleHelper
    {
        /// <summary>
        /// 获取默认字段名
        /// </summary>
        /// <param name="component">组件</param>
        /// <returns>默认字段名</returns>
        string GetDefaultFieldName(Object component);

        /// <summary>
        /// 检查字段名称是否无效
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>字段名是否无效</returns>
        bool CheckFieldNameIsInvalid(string fieldName);
    }
}