using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
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
            var select = referenceBindComponent.BindComponents.Where(_=>_.BindObject != null).ToDictionary(_ => _.BindObject.GetInstanceID(), _ => true);

            void OnComplete()
            {
                var tempList = new List<ReferenceBindComponent.BindObjectData>(referenceBindComponent.BindComponents.Count);
                tempList.AddRange(referenceBindComponent.BindComponents);
                referenceBindComponent.BindComponents.Clear();
                foreach (var selectItem in select)
                {
                    if (selectItem.Value)
                    {
                        var bindData = tempList.Find(_ => _.BindObject.GetInstanceID() == selectItem.Key);
                        Component component = GetComponent(selectItem.Key);
                        string name = bindData == null ? referenceBindComponent.BindComponentsRuleHelper.GetDefaultFieldName(component) : bindData.FieldName;
                        referenceBindComponent.AddBindComponent(name, component, false);
                    }
                }
                referenceBindComponent.SyncBindObjects();
            }

            m_PopWindow.Show(referenceBindComponent.transform, select, OnComplete);
        }
        
        Component GetComponent (int instanceID)
        {
            return (Component)EditorUtility.InstanceIDToObject(instanceID);
        }
    }
}