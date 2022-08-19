using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ReferenceBindTool;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class SelectComponentBindComponentsRuleHelper : IBindComponentsRuleHelper
    {
        public void GetBindComponents(INameRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components)
        {
            BindDataSelect bindDataSelect = target.GetComponent<BindDataSelect>();
            if (bindDataSelect == null)
            {
                return;
            }

            foreach (Component component in bindDataSelect.BindComponents)
            {
                filedNames.Add(ruleHelper.GetDefaultFieldName(target));
                components.Add(component);
            }
        }

        public void BindComponents(ReferenceBindComponent referenceBindComponent)
        {
            Transform transform = referenceBindComponent.transform;
            referenceBindComponent.BindComponents.Clear();
            Transform[] children = transform.GetComponentsInChildren<Transform>(true);
            List<string> tempFiledNames = new List<string>();
            List<Component> tempComponentTypeNames = new List<Component>();
            foreach (Transform child in children)
            {
                tempFiledNames.Clear();
                tempComponentTypeNames.Clear();
                GetBindComponents(referenceBindComponent.NameRuleHelper,child, tempFiledNames, tempComponentTypeNames);
                for (int i = 0; i < tempFiledNames.Count; i++)
                {
                    referenceBindComponent.AddBindComponent(tempFiledNames[i], tempComponentTypeNames[i]);
                }
            }
        }
    }
}