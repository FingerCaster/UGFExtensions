using System.Text.RegularExpressions;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class SelectTreeBindComponentsRuleHelper : IBindComponentsRuleHelper
    {
        public string GetDefaultFieldName(Component component)
        {
            string filedName = $"{component.GetType().Name}_{component.name}".Replace(' ', '_');
            return filedName;
        }

        public bool CheckFieldNameIsInvalid(string fieldName)
        {
            string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            return !Regex.IsMatch(fieldName, regex);
        }
        
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