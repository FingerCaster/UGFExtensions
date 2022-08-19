namespace ReferenceBindTool
{
    /// <summary>
    /// 自动绑定规则辅助器接口
    /// </summary>
    public interface IBindComponentsRuleHelper
    {
        /// <summary>
        /// 绑定符合规则的所有组件
        /// </summary>
        /// <param name="referenceBindComponent">引用绑定组件</param>
        void BindComponents(ReferenceBindComponent referenceBindComponent);
    }
}