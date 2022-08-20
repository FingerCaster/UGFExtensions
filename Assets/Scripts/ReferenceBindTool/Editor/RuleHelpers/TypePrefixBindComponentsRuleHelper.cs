using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

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
        
        public void GetBindComponents(Transform target, List<string> filedNames, List<Component> components)
        {
            string[] strArray = target.name.Split('_');

            if (strArray.Length == 1)
            {
                return;
            }
            
            for (int i = 0; i < strArray.Length - 1; i++)
            {
                string str = strArray[i];
                if (m_PrefixesDict.TryGetValue(str, out var componentType))
                {
                    var component = target.GetComponent(componentType);
                    filedNames.Add(GetDefaultFieldName(component));
                    components.Add(component);
                }
                else
                {
                    Debug.LogWarning($"{target.name}的命名中{str}不存在对应的组件类型，绑定失败");
                }
            }
        }

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
                    referenceBindComponent.AddBindComponent(fieldName, tempComponents[i],false);
                }
            }
        }
    }
}