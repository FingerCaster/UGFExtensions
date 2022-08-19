using UnityEngine;

namespace ReferenceBindTool
{
    /// <summary>
    /// 自动绑定规则辅助器接口
    /// </summary>
    public interface IBindComponentsRuleHelper
    {
        /// <summary>
        /// 获取默认字段名
        /// </summary>
        /// <param name="component">组件</param>
        /// <returns>默认字段名</returns>
        string GetDefaultFieldName(Component component);

        /// <summary>
        /// 检查字段名称是否无效
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns>字段名是否无效</returns>
        bool CheckFieldNameIsInvalid(string fieldName);
        
        /// <summary>
        /// 绑定符合规则的所有组件
        /// </summary>
        /// <param name="referenceBindComponent">引用绑定组件</param>
        void BindComponents(ReferenceBindComponent referenceBindComponent);
    }
}