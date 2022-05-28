using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace ComponentBindTool
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
        /// <param name="target"></param>
        /// <param name="filedNames"></param>
        /// <param name="components"></param>
        void GetBindData(Transform target, List<string> filedNames, List<Component> components);
    }

    public class DefaultBindRule : IBindRule
    {
        public string Prefix => "BD_";

        private List<Type> BindTypes => new List<Type>()
        {
            typeof(Transform),
        };

        private string GetFiledName(Transform target, string componentName)
        {
            string filedName = $"{componentName}_{target.name.Substring(Prefix.Length)}".Replace(' ', '_');
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
            if (target == null || string.IsNullOrEmpty(target.name) || !target.name.StartsWith(Prefix))
            {
                return;
            }

            for (int i = 0; i < BindTypes.Count; i++)
            {
                Component component = target.GetComponent(BindTypes[i]);
                if (component != null)
                {
                    filedNames.Add(GetFiledName(target, BindTypes[i].Name));
                    components.Add(component);
                }
            }
        }
    }

    public class DefaultAutoBindRuleHelper : IAutoBindRuleHelper
    {
        private List<IBindRule> m_BindRules = new List<IBindRule>()
        {
            new DefaultBindRule(),
        };

        public void GetBindData(Transform target, List<string> filedNames, List<Component> components)
        {
            for (int i = 0; i < m_BindRules.Count; i++)
            {
                if (!target.name.StartsWith(m_BindRules[i].Prefix))
                {
                    continue;
                }

                m_BindRules[i].GetBindData(target, filedNames, components);
            }
        }
    }
}