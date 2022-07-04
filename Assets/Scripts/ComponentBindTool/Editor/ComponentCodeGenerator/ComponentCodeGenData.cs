using System;
using System.Collections.Generic;
using UnityEngine;

namespace ComponentCodeGenerator
{
    public class ComponentCodeGenData
    {
        private GameObject m_GameObject;

        public GameObject GameObject
        {
            get => m_GameObject;
            set => m_GameObject = value;
        }

        [Serializable]
        public class BindData
        {
            public BindData()
            {
            }

            public BindData(bool isRepeatName, string name, Component bindCom)
            {
                IsRepeatName = isRepeatName;
                Name = name;
                BindCom = bindCom;
            }

            public bool IsRepeatName;
            public string Name;
            public Component BindCom;
        }

        private List<BindData> m_BindDataList = new List<BindData>();

        public List<BindData> BindDataList
        {
            get => m_BindDataList;
            set => m_BindDataList = value;
        }
        
        private string m_ClassName;
        
        private AutoBindSettingData m_SettingData;
        
        public AutoBindSettingData SettingData
        {
            get => m_SettingData;
            set => m_SettingData = value;
        }

        public string ClassName
        {
            get => m_ClassName;
            set => m_ClassName = value;
        }
        public IAutoBindRuleHelper RuleHelper
        {
            get;
            set;
        }

        private string m_RuleHelperTypeName  = null;

        public string RuleHelperTypeName
        {
            get => m_RuleHelperTypeName;
            set => m_RuleHelperTypeName = value;
        }

    }
}