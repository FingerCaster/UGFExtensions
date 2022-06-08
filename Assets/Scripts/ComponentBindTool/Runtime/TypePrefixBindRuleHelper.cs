using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 默认自动绑定规则辅助器
/// </summary>
public class TypePrefixBindRuleHelper : IAutoBindRuleHelper
{
    /// <summary>
    /// 命名前缀与类型的映射
    /// </summary>
    private Dictionary<string, string> m_PrefixesDict = new Dictionary<string, string>()
    {
        {"Trans","Transform" },
        {"OldAnim","Animation"}, 
        {"NewAnim","Animator"},

        {"Rect","RectTransform"},
        {"Canvas","Canvas"},
        {"Group","CanvasGroup"},
        {"VGroup","VerticalLayoutGroup"},
        {"HGroup","HorizontalLayoutGroup"},
        {"GGroup","GridLayoutGroup"},
        {"TGroup","ToggleGroup"},

        {"Btn","Button"},
        {"Img","Image"},
        {"RImg","RawImage"},
        {"Txt","Text"},
        {"Input","InputField"},
        {"Slider","Slider"},
        {"Mask","Mask"},
        {"Mask2D","RectMask2D"},
        {"Tog","Toggle"},
        {"Sbar","Scrollbar"},
        {"SRect","ScrollRect"},
        {"Drop","Dropdown"},
    };
    private string GetFiledName(Transform target, string componentName,string noPrefixName)
    {
        string filedName = $"{componentName}_{noPrefixName}".Replace(' ', '_');
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
        string[] strArray = target.name.Split('_');

        if (strArray.Length == 1)
        {
            return;
        }

        string filedName = strArray[strArray.Length - 1];

        for (int i = 0; i < strArray.Length - 1; i++)
        {
            string str = strArray[i];
            string comName;
            if (m_PrefixesDict.TryGetValue(str,out comName))
            {
                filedNames.Add(GetFiledName(target,str,filedName));
                components.Add(target.GetComponent(comName));
            }
            else
            {
                Debug.LogError($"{target.name}的命名中{str}不存在对应的组件类型，绑定失败");
            }
        }
    }
}