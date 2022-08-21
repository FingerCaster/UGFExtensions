using UnityEngine;
using BindObjectData = ReferenceBindTool.ReferenceBindComponent.BindObjectData;

namespace ReferenceBindTool.Editor
{
    public static class NotAddComponentDataExtensions
    {
        /// <summary>
        /// 添加绑定数据
        /// </summary>
        public static void AddBindComponent(this NotAddComponentData self, string fieldName, Component bindComponent)
        {
            bool isRepeat = false;
            for (int j = 0; j < self.BindComponents.Count; j++)
            {
                if (self.BindComponents[j].FieldName == fieldName)
                {
                    isRepeat = true;
                    break;
                }
            }

            self.BindComponents.Add(new BindObjectData(isRepeat, fieldName, bindComponent)
            {
                FileNameIsInvalid = self.RuleHelper.CheckFieldNameIsInvalid(fieldName)
            });
        }

        
        /// <summary>
        /// 规则绑定所有组件
        /// </summary>
        public static void RuleBindComponents(this NotAddComponentData self)
        {
            self.RuleHelper.BindComponents(self.GameObject, self.BindComponents,bindList =>
            {
                self.BindComponents.Clear();
                foreach ((string fieldName, Component bindComponent) item in bindList)
                {
                    self.AddBindComponent(item.fieldName,item.bindComponent);
                }
            });
        }
        
        /// <summary>
        /// 设置脚本名称
        /// </summary>
        /// <param name="self"></param>
        /// <param name="className"></param>
        public static void SetClassName(this NotAddComponentData self, string className)
        {
            if (self.GeneratorCodeName == className)
            {
                return;
            }

            self.GeneratorCodeName = className;
            
        }

        /// <summary>
        /// 设置生成规则帮助类
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ruleHelperName"></param>
        public static void SetRuleHelperTypeName(this NotAddComponentData self, string ruleHelperName)
        {
            if (self.RuleHelperTypeName == ruleHelperName && self.RuleHelper != null)
            {
                return;
            }

            self.RuleHelperTypeName = ruleHelperName;
            IBindComponentsRuleHelper helper = (IBindComponentsRuleHelper) ReferenceBindUtility.CreateHelperInstance(self.RuleHelperTypeName);
            self.RuleHelper = helper;
            
        }
        
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void SetSettingData(this NotAddComponentData self, string name)
        {
            self.CodeGeneratorSettingData = ReferenceBindUtility.GetAutoBindSetting(name);
        }
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void SetSettingData(this NotAddComponentData self, ReferenceBindCodeGeneratorSettingData data)
        {
            if (self.CodeGeneratorSettingData == data)
            {
                return;
            }

            self.CodeGeneratorSettingData = data;
        }
    }
}