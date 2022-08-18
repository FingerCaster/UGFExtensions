using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReferenceBindTool
{
    /// <summary>
    /// 引用绑定组件
    /// </summary>
    public class ReferenceBindComponent : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// 绑定对象数据类
        /// </summary>
        [Serializable]
        public class BindObjectData
        {
            public BindObjectData()
            {
            }

            public BindObjectData(bool isRepeatName, string name, Object bindObject)
            {
                m_IsRepeatName = isRepeatName;
                FieldName = name;
                m_BindObject = bindObject;
            }

            /// <summary>
            /// 生成字段名
            /// </summary>
            [SerializeField] private string m_FieldName;

            /// <summary>
            /// 命名是否重复
            /// </summary>
            [SerializeField] private bool m_IsRepeatName;

            /// <summary>
            /// 字段名称是否无效
            /// </summary>
            [SerializeField] private bool m_FiedNameIsInvalid;

            /// <summary>
            /// 绑定对象(组件 资源 预制体)
            /// </summary>
            [SerializeField] private Object m_BindObject;

            /// <summary>
            /// 生成字段名
            /// </summary>
            public string FieldName
            {
                get => m_FieldName;
                set
                {
                    m_FiedNameIsInvalid = CheckFieldNameIsInvalid(value);
                    m_FieldName = value;
                }
            }

            /// <summary>
            /// 命名是否重复
            /// </summary>
            public bool IsRepeatName => m_IsRepeatName;

            /// <summary>
            /// 字段名称是否无效
            /// </summary>
            public bool FileNameIsInvalid
            {
                get => m_FiedNameIsInvalid;
            }
            
            /// <summary>
            /// 绑定对象(组件 资源 预制体)
            /// </summary>
            public Object BindObject => m_BindObject;

            /// <summary>
            /// 检查字段名是否无效
            /// </summary>
            /// <param name="filedName">字段名</param>
            /// <returns>字段名是否无效</returns>
            public static bool CheckFieldNameIsInvalid(string filedName)
            {
                string regex = "^[a-zA-Z_][a-zA-Z0-9_]*$";
                return !Regex.IsMatch(filedName, regex);
            }
        }

        /// <summary>
        /// 自动绑定设置数据
        /// </summary>
        [SerializeField] private AutoBindSettingData m_SettingData;

        /// <summary>
        /// 所有绑定组件
        /// </summary>
        [SerializeField] private List<BindObjectData> m_BindComponents = new List<BindObjectData>();

        /// <summary>
        /// 所有绑定资源和预制体
        /// </summary>
        [SerializeField] private List<BindObjectData> m_BindAssetsOrPrefabs = new List<BindObjectData>();

        /// <summary>
        /// 生成代码文件名称
        /// </summary>
        [SerializeField] private string m_GeneratorCodeName;

        /// <summary>
        /// 自动绑定设置可搜索配置
        /// </summary>
        [SerializeField] private SearchableData m_SettingDataSearchable;

        /// <summary>
        /// 自动绑定组件规则类型名称
        /// </summary>
        [SerializeField] private string m_RuleHelperTypeName;
        /// <summary>
        /// 所有绑定组件
        /// </summary>
        public List<BindObjectData> BindComponents
        {
            get => m_BindComponents;
            set => m_BindComponents = value;
        }

        /// <summary>
        /// 所有绑定资源和预制体
        /// </summary>
        public List<BindObjectData> BindAssetsOrPrefabs
        {
            get => m_BindAssetsOrPrefabs;
            set => m_BindAssetsOrPrefabs = value;
        }

        /// <summary>
        /// 自动绑定设置可搜索配置
        /// </summary>
        public SearchableData SettingDataSearchable
        {
            get => m_SettingDataSearchable;
            set => m_SettingDataSearchable = value;
        }

        /// <summary>
        /// 生成代码文件名称
        /// </summary>
        public string GeneratorCodeName
        {
            get => m_GeneratorCodeName;
            set => m_GeneratorCodeName = value;
        }

        /// <summary>
        /// 自动绑定设置数据
        /// </summary>
        public AutoBindSettingData SettingData
        {
            get => m_SettingData;
            set => m_SettingData = value;
        }


    
        /// <summary>
        /// 获取所有绑定对象的数量
        /// </summary>
        /// <returns>所有绑定对象的数量</returns>
        public int GetAllBindObjectsCount()
        {
            return m_BindAssetsOrPrefabs.Count + m_BindComponents.Count;
        }
        
        /// <summary>
        /// 所有绑定对象
        /// </summary>
        public List<Object> BindObjects => m_BindObjects;
        
        /// <summary>
        /// 自动绑定组件规则
        /// </summary>
        public IAutoBindRuleHelper RuleHelper
        {
            get;
            set;
        }

        /// <summary>
        /// 自动绑定组件规则类型名称
        /// </summary>
        public string RuleHelperTypeName
        {
            get => m_RuleHelperTypeName;
            set => m_RuleHelperTypeName = value;
        }
#endif


        /// <summary>
        /// 所有绑定对象
        /// </summary>
        [SerializeField] private List<Object> m_BindObjects = new List<UnityEngine.Object>();

        /// <summary>
        /// 获取绑定对象
        /// </summary>
        /// <param name="index">索引</param>
        /// <typeparam name="T">绑定对象类型</typeparam>
        /// <returns>绑定对象</returns>
        public T GetBindObject<T>(int index) where T : UnityEngine.Object
        {
            if (index >= m_BindObjects.Count)
            {
                Debug.LogError("索引无效");
                return null;
            }

            T bindCom = m_BindObjects[index] as T;

            if (bindCom == null)
            {
                Debug.LogError($"类型无效,传入类型:{typeof(T)} 对象类型：{m_BindObjects[index].GetType()}");
                return null;
            }

            return bindCom;
        }
    }
}