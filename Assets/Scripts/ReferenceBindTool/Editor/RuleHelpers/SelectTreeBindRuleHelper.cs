namespace ReferenceBindTool.Editor
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