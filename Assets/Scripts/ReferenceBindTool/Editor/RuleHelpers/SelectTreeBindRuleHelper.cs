#if UNITY_EDITOR
namespace ReferenceBindTool
{
    public class SelectTreeBindComponentsRuleHelper : IBindComponentsRuleHelper
    {
        private SelectComponentTreePopWindow m_PopWindow;

        public SelectTreeBindComponentsRuleHelper()
        {
            m_PopWindow = new SelectComponentTreePopWindow();
        }
        public void BindComponents(ReferenceBindComponent referenceBindComponent)
        {
            m_PopWindow.Show(referenceBindComponent);
        }
    }
}
#endif