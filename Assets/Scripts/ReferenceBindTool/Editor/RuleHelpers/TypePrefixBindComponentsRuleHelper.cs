using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ReferenceBindTool;
using UnityEngine;

namespace ReferenceBindTool.Editor
{
    /// <summary>
    /// 默认自动绑定规则辅助器
    /// </summary>
    public class TypePrefixBindComponentsRuleHelper : IBindComponentsRuleHelper
    {
        /// <summary>
        /// 命名前缀与类型的映射
        /// </summary>
        private Dictionary<string, string> m_PrefixesDict = new Dictionary<string, string>()
        {
            {"Trans", "Transform"},
            {"OldAnim", "Animation"},
            {"NewAnim", "Animator"},

            {"Rect", "RectTransform"},
            {"Canvas", "Canvas"},
            {"Group", "CanvasGroup"},
            {"VGroup", "VerticalLayoutGroup"},
            {"HGroup", "HorizontalLayoutGroup"},
            {"GGroup", "GridLayoutGroup"},
            {"TGroup", "ToggleGroup"},

            {"Btn", "Button"},
            {"Img", "Image"},
            {"RImg", "RawImage"},
            {"Txt", "Text"},
            {"Input", "InputField"},
            {"Slider", "Slider"},
            {"Mask", "Mask"},
            {"Mask2D", "RectMask2D"},
            {"Tog", "Toggle"},
            {"Sbar", "Scrollbar"},
            {"SRect", "ScrollRect"},
            {"Drop", "Dropdown"},
        };
        
        public void GetBindComponents(INameRuleHelper ruleHelper,Transform target, List<string> filedNames, List<Component> components)
        {
            string[] strArray = target.name.Split('_');

            if (strArray.Length == 1)
            {
                return;
            }
            
            for (int i = 0; i < strArray.Length - 1; i++)
            {
                string str = strArray[i];
                if (m_PrefixesDict.TryGetValue(str, out var comName))
                {
                    filedNames.Add(ruleHelper.GetDefaultFieldName(target));
                    components.Add(target.GetComponent(comName));
                }
                else
                {
                    Debug.LogError($"{target.name}的命名中{str}不存在对应的组件类型，绑定失败");
                }
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