using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using BindObjectData = ReferenceBindTool.ReferenceBindComponent.BindObjectData;

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
        private Dictionary<string, Type> m_PrefixesDict = new Dictionary<string, Type>()
        {
            {"Trans", typeof(Transform)},
            {"OldAnim", typeof(Animation)},
            {"NewAnim", typeof(Animator)},

            {"Rect", typeof(RectTransform)},
            {"Canvas", typeof(Canvas)},
            {"Group",typeof(CanvasGroup)},
            {"VGroup",typeof(VerticalLayoutGroup)},
            {"HGroup",typeof(HorizontalLayoutGroup)},
            {"GGroup",typeof(GridLayoutGroup)},
            {"TGroup",typeof(ToggleGroup)},

            {"Btn",typeof(Button)},
            {"Img",typeof(Image)},
            {"RImg",typeof(RawImage)},
            {"Txt",typeof(Text)},
            {"Input",typeof(InputField)},
            {"Slider",typeof(Slider)},
            {"Mask",typeof(Mask)},
            {"Mask2D",typeof(RectMask2D)},
            {"Tog",typeof(Toggle)},
            {"Sbar",typeof(Scrollbar)},
            {"SRect",typeof(ScrollRect)},
            {"Drop",typeof(Dropdown)},
        };
        
        public string GetDefaultFieldName(Component component)
        {
            string gameObjectName = component.gameObject.name;
            string[] strArray = component.name.Split('_');

            if (strArray.Length != 1)
            {
                for (int i = 0; i < strArray.Length - 1; i++)
                {
                    string str = strArray[i];
                    if (!m_PrefixesDict.ContainsKey(str)) continue;
                    gameObjectName = strArray[strArray.Length - 1];
                    break;
                }
            }
            return $"{component.GetType().Name}_{gameObjectName}".Replace(' ', '_');
        }

        public bool CheckFieldNameIsInvalid(string fieldName)
        {
            string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
            return !Regex.IsMatch(fieldName, regex);
        }

        public void BindComponents(GameObject gameObject, List<BindObjectData> bindComponents, Action<List<(string, Component)>> bindAction)
        {
            List<(string fieldName,Component bindComponent)> bindList = new List<(string,Component)>();
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                string[] strArray = child.name.Split('_');

                if (strArray.Length == 1)
                {
                    continue;
                }
            
                for (int i = 0; i < strArray.Length - 1; i++)
                {
                    string str = strArray[i];
                    if (m_PrefixesDict.TryGetValue(str, out var componentType))
                    {
                        var component = child.GetComponent(componentType);
                        var bindData = bindComponents.Find(_ => _.BindObject == component);
                        string fieldName = bindData == null ? GetDefaultFieldName(component) : bindData.FieldName;
                        bindList.Add((fieldName,component));
                    }
                    else
                    {
                        Debug.LogWarning($"{child.name}的命名中{str}不存在对应的组件类型，绑定失败");
                    }
                }
            }
        }
    }
}