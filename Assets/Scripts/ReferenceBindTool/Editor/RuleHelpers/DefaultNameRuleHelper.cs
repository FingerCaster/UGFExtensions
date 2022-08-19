using System.Text.RegularExpressions;
using UnityEngine;

namespace ReferenceBindTool.Editor.RuleHelpers
{
    public class DefaultNameRuleHelper : INameRuleHelper
    {
        public string GetDefaultFieldName(Object component)
        {
            string filedName = $"{component.GetType().Name}_{component.name}".Replace(' ', '_');
            return filedName;
        }

        public bool CheckFieldNameIsInvalid(string fieldName)
        {
            string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            return !Regex.IsMatch(fieldName, regex);
        }

    }
}