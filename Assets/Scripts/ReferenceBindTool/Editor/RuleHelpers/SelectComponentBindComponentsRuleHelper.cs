using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    public class SelectComponentBindComponentsRuleHelper : IBindComponentsRuleHelper
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
        
        public void GetBindComponents(Transform target, List<string> filedNames, List<Component> components)
        {
            BindDataSelect bindDataSelect = target.GetComponent<BindDataSelect>();
            if (bindDataSelect == null)
            {
                return;
            }

            foreach (Component component in bindDataSelect.BindComponents)
            {
                filedNames.Add(GetDefaultFieldName(component));
                components.Add(component);
            }
        }

        public void BindComponents(ReferenceBindComponent referenceBindComponent)
        {
            Transform transform = referenceBindComponent.transform;
            var tempList = new List<ReferenceBindComponent.BindObjectData>(referenceBindComponent.BindComponents.Count);
            tempList.AddRange(referenceBindComponent.BindComponents);
            referenceBindComponent.BindComponents.Clear();
            Transform[] children = transform.GetComponentsInChildren<Transform>(true);
            List<string> tempFiledNames = new List<string>();
            List<Component> tempComponents = new List<Component>();
            foreach (Transform child in children)
            {
                tempFiledNames.Clear();
                tempComponents.Clear();
                GetBindComponents(child, tempFiledNames, tempComponents);
                for (int i = 0; i < tempFiledNames.Count; i++)
                {
                    var bindData = tempList.Find(_ => _.BindObject == tempComponents[i]);
                    string fieldName = bindData == null ? tempFiledNames[i] : bindData.FieldName;
                    referenceBindComponent.AddBindComponent(fieldName, tempComponents[i]);
                }
            }
        }
    }
}