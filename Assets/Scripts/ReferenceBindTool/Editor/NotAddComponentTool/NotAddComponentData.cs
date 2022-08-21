using System;
using System.Collections.Generic;
using UnityEngine;
using BindObjectData = ReferenceBindTool.ReferenceBindComponent.BindObjectData;
namespace ReferenceBindTool.Editor
{
    public class NotAddComponentData
    {
        private GameObject m_GameObject;

        public GameObject GameObject
        {
            get => m_GameObject;
            set => m_GameObject = value;
        }
        

        private List<BindObjectData> m_BindComponents = new List<BindObjectData>();

        public List<BindObjectData> BindComponents
        {
            get => m_BindComponents;
            set => m_BindComponents = value;
        }
        
        private string m_GeneratorCodeName;
        
        private ReferenceBindCodeGeneratorSettingData m_CodeGeneratorSettingData;
        
        public ReferenceBindCodeGeneratorSettingData CodeGeneratorSettingData
        {
            get => m_CodeGeneratorSettingData;
            set => m_CodeGeneratorSettingData = value;
        }

        public string GeneratorCodeName
        {
            get => m_GeneratorCodeName;
            set => m_GeneratorCodeName = value;
        }
        public IBindComponentsRuleHelper RuleHelper
        {
            get;
            set;
        }

        private string m_RuleHelperTypeName  = typeof(DefaultBindComponentsRuleHelper).FullName;

        
        public string RuleHelperTypeName
        {
            get => m_RuleHelperTypeName;
            set => m_RuleHelperTypeName = value;
        }
    }
}