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

            [SerializeField] private bool m_IsRepeatName;
            [SerializeField] private string m_FieldName;
            [SerializeField] private Object m_BindObject;
            [SerializeField] private bool m_FileNameIsInvalid;

            public bool FileNameIsInvalid
            {
                get => m_FileNameIsInvalid;
            }

            public bool IsRepeatName => m_IsRepeatName;

            public string FieldName
            {
                get => m_FieldName;
                set
                {
                    m_FileNameIsInvalid = CheckFieldNameIsInvalid(value);
                    m_FieldName = value;
                }
            }
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
            public Object BindObject => m_BindObject;
        }

        [Serializable]
        public class ReferenceData
        {
            public ReferenceData(Object bindReference)
            {
                m_BindReference = bindReference;
                m_IsOnlyBindSelf = !(bindReference is GameObject);
                m_BindObjects = new List<BindObjectData>();
            }

            [SerializeField] private Object m_BindReference;
            [SerializeField] private List<BindObjectData> m_BindObjects;

            [SerializeField] private bool m_IsOnlyBindSelf;

            public bool IsOnlyBindSelf => m_IsOnlyBindSelf;

            public Object BindReference => m_BindReference;

            public List<BindObjectData> BindObjects => m_BindObjects;
        }

        [SerializeField] private AutoBindSettingData m_SettingData;
        [SerializeField] private List<ReferenceData> m_ReferenceDataList = new List<ReferenceData>();
        [SerializeField] private string m_GeneratorCodeName;
        [SerializeField] private SearchableData m_SettingDataSearchable;

        public SearchableData SettingDataSearchable
        {
            get => m_SettingDataSearchable;
            set => m_SettingDataSearchable = value;
        }

        public string GeneratorCodeName
        {
            get => m_GeneratorCodeName;
            set => m_GeneratorCodeName = value;
        }

        public AutoBindSettingData SettingData
        {
            get => m_SettingData;
            set => m_SettingData = value;
        }

        public List<ReferenceData> ReferenceDataList => m_ReferenceDataList;

        public List<BindObjectData> GetAllBindObjectDataList()
        {
            List<BindObjectData> list = new List<BindObjectData>(10);
            foreach (ReferenceData referenceData in m_ReferenceDataList)
            {
                foreach (var bindObject in referenceData.BindObjects)
                {
                    list.Add(bindObject);
                }
            }

            return list;
        }

        public List<Object> BindObjects => m_BindObjects;
#endif

        [SerializeField] private List<Object> m_BindObjects = new List<UnityEngine.Object>();

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