using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReferenceBindTool.Editor
{
    internal interface IBindRule
    {
        /// <summary>
        /// 前缀名
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// 获取绑定数据
        /// </summary>
        /// <param name="ruleHelper"></param>
        /// <param name="target"></param>
        /// <param name="filedNames"></param>
        /// <param name="components"></param>
        void GetBindData(DefaultBindComponentsRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components);
    }

    public class DefaultBindRule : IBindRule
    {
        public string Prefix => "BD_";

        private List<Type> BindTypes => new List<Type>()
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(Animation),
            typeof(Animation),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(VerticalLayoutGroup),
            typeof(HorizontalLayoutGroup),
            typeof(GridLayoutGroup),
            typeof(ToggleGroup),
            typeof(Button),
            typeof(Image),
            typeof(RawImage),
            // 取消bind Text 改为使用TMP
            // typeof(Text),
            typeof(TMP_Text),
            typeof(InputField),
            typeof(TMP_InputField),
            typeof(Slider),
            typeof(Mask),
            typeof(RectMask2D),
            typeof(Toggle),
            typeof(Scrollbar),
            typeof(ScrollRect),
            typeof(Dropdown),
            typeof(TMP_Dropdown),
            typeof(Camera),
            // typeof(EnhancedScroller),
            // typeof(EnhancedScrollerCellView),
            typeof(EventTrigger),
        };
        
        public void GetBindData(DefaultBindComponentsRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components)
        {
            if (target == null || string.IsNullOrEmpty(target.name) || !target.name.StartsWith(Prefix))
            {
                return;
            }

            for (int i = 0; i < BindTypes.Count; i++)
            {
                Component component = target.GetComponent(BindTypes[i]);
                if (component != null)
                {
                    filedNames.Add(ruleHelper.GetDefaultFieldName(component));
                    components.Add(component);
                }
            }
        }
    }

    public class DefaultBindComponentsRuleHelper : IBindComponentsRuleHelper
    {
        private List<IBindRule> m_BindRules = new List<IBindRule>()
        {
            new DefaultBindRule(),
        };

        public void GetBindComponents(Transform target, List<string> filedNames, List<Component> components)
        {
            for (int i = 0; i < m_BindRules.Count; i++)
            {
                if (!target.name.StartsWith(m_BindRules[i].Prefix))
                {
                    continue;
                }

                m_BindRules[i].GetBindData(this,target, filedNames, components);
            }
        }

        public string GetDefaultFieldName(Component component)
        {
            string gameObjectName = component.gameObject.name;
            for (int i = 0; i < m_BindRules.Count; i++)
            {
                if (!gameObjectName.StartsWith(m_BindRules[i].Prefix)) continue;
                gameObjectName = gameObjectName.Substring(m_BindRules[i].Prefix.Length);
                break;
            }
            return $"{component.GetType().Name}_{gameObjectName}".Replace(' ', '_');
        }

        public bool CheckFieldNameIsInvalid(string fieldName)
        {
            string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            return !Regex.IsMatch(fieldName, regex);
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