using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ReferenceBindTool;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class SelectComponentBindComponentsRuleHelper : IBindComponentsRuleHelper
    {
        private string GetFiledName(Transform target, string componentName)
        {
            string filedName = $"{componentName}_{target.name}".Replace(' ', '_');
            string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            if (!Regex.IsMatch(filedName, regex))
            {
                UnityEditor.Selection.activeTransform = target;
                throw new Exception($"FiledName : \"{target.name}\" is invalid.Please check it!");
            }

            return filedName;
        }

        public void GetBindData(Transform target, List<string> filedNames, List<Component> components)
        {
            BindDataSelect bindDataSelect = target.GetComponent<BindDataSelect>();
            if (bindDataSelect == null)
            {
                return;
            }

            foreach (Component component in bindDataSelect.BindComponents)
            {
                filedNames.Add(GetFiledName(target, component.GetType().Name));
                components.Add(component);
            }
        }

        public void BindComponents(ReferenceBindComponent referenceBindComponent)
        {
            throw new NotImplementedException();
        }
    }
}