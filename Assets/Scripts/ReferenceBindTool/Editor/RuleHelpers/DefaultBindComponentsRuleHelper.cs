using System;
using System.Collections.Generic;
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
        void GetBindData(INameRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components);
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
        
        public void GetBindData(INameRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components)
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
                    filedNames.Add(ruleHelper.GetDefaultFieldName(target));
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

        public void GetBindComponents(INameRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components)
        {
            for (int i = 0; i < m_BindRules.Count; i++)
            {
                if (!target.name.StartsWith(m_BindRules[i].Prefix))
                {
                    continue;
                }

                m_BindRules[i].GetBindData(ruleHelper,target, filedNames, components);
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