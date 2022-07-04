using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SelectComponentBindRuleHelper : IAutoBindRuleHelper
{
    private string GetFiledName(Transform target, string componentName)
    {
        string filedName = $"{componentName}_{target.name}".Replace(' ', '_');
        string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
        if (!Regex.IsMatch(filedName, regex))
        {
#if UNITY_EDITOR
            UnityEditor.Selection.activeTransform = target;
#endif
            throw new Exception($"FiledName : \"{target.name}\" is invalid.Please check it!");
        }

        return filedName;
    }
    
    public void GetBindData(Transform target, List<string> filedNames, List<Component> components)
    {
#if UNITY_EDITOR
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
#endif
    }
}