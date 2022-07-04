using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ComponentCodeGenerator
{
    public static class ComponentCodeGenDataExtensions
    {
        /// <summary>
        /// 添加绑定数据
        /// </summary>
        public static void AddBindData(this ComponentCodeGenData self, string name, Component bindCom)
        {
            bool isRepeat = false;
            for (int j = 0; j < self.BindDataList.Count; j++)
            {
                if (self.BindDataList[j].Name == name)
                {
                    isRepeat = true;
                    break;
                }
            }

            self.BindDataList.Add(new ComponentCodeGenData.BindData(isRepeat, name, bindCom));
            
        }

        
        /// <summary>
        /// 自动绑定组件
        /// </summary>
        public static void AutoBindComponent(this ComponentCodeGenData self)
        {
            self.BindDataList.Clear();
            List<string> tempFiledNames = new List<string>();
            List<Component> tempComponentTypeNames = new List<Component>();
            Transform[] children = self.GameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                tempFiledNames.Clear();
                tempComponentTypeNames.Clear();
                self.RuleHelper.GetBindData(child, tempFiledNames, tempComponentTypeNames);
                for (int i = 0; i < tempFiledNames.Count; i++)
                {
                    self.AddBindData(tempFiledNames[i], tempComponentTypeNames[i]);
                }
            }

        }
        
        /// <summary>
        /// 设置脚本名称
        /// </summary>
        /// <param name="self"></param>
        /// <param name="className"></param>
        public static void SetClassName(this ComponentCodeGenData self, string className)
        {
            if (self.ClassName == className)
            {
                return;
            }

            self.ClassName = className;
            
        }

        /// <summary>
        /// 设置生成规则帮助类
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ruleHelper"></param>
        public static void SetRuleHelperTypeName(this ComponentCodeGenData self, string ruleHelperName)
        {
            if (self.RuleHelperTypeName == ruleHelperName)
            {
                return;
            }

            self.RuleHelperTypeName = ruleHelperName;
            IAutoBindRuleHelper helper =
                (IAutoBindRuleHelper) ComponentAutoBindToolUtility.CreateHelperInstance(self.RuleHelperTypeName);
            self.RuleHelper = helper;
            
        }
        
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void SetSettingData(this ComponentCodeGenData self, string name)
        {
            self.SettingData = ComponentAutoBindToolUtility.GetAutoBindSetting(name);
        }
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void SetSettingData(this ComponentCodeGenData self, AutoBindSettingData data)
        {
            if (self.SettingData == data)
            {
                return;
            }

            self.SettingData = data;
        }
    }
}