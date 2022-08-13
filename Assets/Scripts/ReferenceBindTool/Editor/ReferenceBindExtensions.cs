using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ReferenceData = ReferenceBindTool.ReferenceBindComponent.ReferenceData;

namespace ReferenceBindTool.Editor
{
    /// <summary>
    /// 引用绑定工具类
    /// </summary>
    public static class ReferenceBindExtensions
    {
        /// <summary>
        /// 排序
        /// </summary>
        public static void Sort(this ReferenceBindComponent self)
        {
            self.ReferenceDataList.Sort((x, y) =>
                string.Compare(x.BindReference.name, y.BindReference.name, StringComparison.Ordinal));
            foreach (ReferenceData reference in self.ReferenceDataList)
            {
                reference.BindObjects.Sort((x, y) =>
                    string.Compare(x.FieldName, y.FieldName, StringComparison.Ordinal));
            }

            self.SyncBindObjects();
        }

        /// <summary>
        /// 同步绑定数据
        /// </summary>
        public static void SyncBindObjects(this ReferenceBindComponent self)
        {
            self.BindObjects.Clear();
            foreach (var bindData in self.ReferenceDataList)
            {
                foreach (ReferenceBindComponent.BindObjectData bindComponentData in bindData.BindObjects)
                {
                    self.BindObjects.Add(bindComponentData.BindObject);
                }
            }

            EditorUtility.SetDirty(self);
        }


        /// <summary>
        /// 删除Missing Or Null
        /// </summary>
        public static void RemoveNull(this ReferenceBindComponent self)
        {
            for (var i = 0; i < self.ReferenceDataList.Count; i++)
            {
                ReferenceData reference = self.ReferenceDataList[i];
                if (reference.BindReference == null)
                {
                    self.ReferenceDataList.RemoveAt(i);
                    continue;
                }

                for (var j = 0; j < reference.BindObjects.Count; j++)
                {
                    ReferenceBindComponent.BindObjectData bindObjectData = reference.BindObjects[j];
                    if (bindObjectData.BindObject == null)
                    {
                        reference.BindObjects.RemoveAt(j);
                    }
                }
            }

            self.SyncBindObjects();
        }

        /// <summary>
        /// 全部删除
        /// </summary>
        public static void RemoveAll(this ReferenceBindComponent self)
        {
            self.ReferenceDataList.Clear();
            self.SyncBindObjects();
        }


        /// <summary>
        /// 添加引用
        /// </summary>
        public static void AddReferenceData(this ReferenceBindComponent self, UnityEngine.Object bindObject)
        {
            if (!ReferenceBindUtility.CheckIsCanAdd(bindObject))
            {
                Debug.LogError("不能添加目录!");
                return;
            }
            foreach (var referenceData in self.ReferenceDataList)
            {
                if (referenceData.BindReference == bindObject)
                {
                    Debug.LogWarning($"{bindObject.name} 已经添加。");
                    return;
                }
            }

            ReferenceData reference = new ReferenceData(bindObject);
            self.ReferenceDataList.Add(reference);
            if (reference.IsOnlyBindSelf)
            {
                self.AddBindObject(reference,bindObject);
                self.SyncBindObjects();
            }
        }

        /// <summary>
        /// 添加绑定对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="referenceData"></param>
        /// <param name="bindObject"></param>
        /// <param name="fieldName"></param>
        public static void AddBindObject(this ReferenceBindComponent self, ReferenceData referenceData, Object bindObject,string fieldName = null)
        {
            var isRepeat = false;
            string filedName =fieldName ?? ReferenceBindUtility.GetFiledName(bindObject);

            foreach (var temp in self.ReferenceDataList)
            {
                foreach (var bindComponent in temp.BindObjects)
                {
                    if (bindComponent.FieldName == filedName)
                    {
                        isRepeat = true;
                        break;
                    }
                }

                if (isRepeat)
                {
                    break;
                }
            }

            referenceData.BindObjects.Add(new ReferenceBindComponent.BindObjectData(isRepeat, filedName, bindObject));
            self.SyncBindObjects();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="self"></param>
        public static void Refresh(this ReferenceBindComponent self)
        {
            var tempList = new List<ReferenceData>(self.ReferenceDataList.Count);
            tempList.AddRange(self.ReferenceDataList);
            self.ReferenceDataList.Clear();
            for (var i = 0; i < tempList.Count; i++)
            {
                var tempData = tempList[i];

                ReferenceData reference = new ReferenceData(tempData.BindReference);
             
                for (int j = 0; j < tempData.BindObjects.Count; j++)
                {
                    self.AddBindObject(reference,tempData.BindObjects[j].BindObject,tempData.BindObjects[j].FieldName);
                }

                self.ReferenceDataList.Add(reference);
            }

            self.SyncBindObjects();
        }
        /// <summary>
        /// 重置所有字段名
        /// </summary>
        /// <param name="self"></param>
        public static void ResetAllFieldName(this ReferenceBindComponent self)
        {
            var tempList = new List<ReferenceData>(self.ReferenceDataList.Count);
            tempList.AddRange(self.ReferenceDataList);
            self.ReferenceDataList.Clear();
            for (var i = 0; i < tempList.Count; i++)
            {
                var tempData = tempList[i];

                ReferenceData reference = new ReferenceData(tempData.BindReference);
             
                for (int j = 0; j < tempData.BindObjects.Count; j++)
                {
                    self.AddBindObject(reference,tempData.BindObjects[j].BindObject,ReferenceBindUtility.GetFiledName(tempData.BindObjects[j].BindObject));
                }

                self.ReferenceDataList.Add(reference);
            }

            self.SyncBindObjects();
        }
        

        /// <summary>
        /// 设置脚本名称
        /// </summary>
        /// <param name="self"></param>
        /// <param name="className"></param>
        public static void SetClassName(this ReferenceBindComponent self, string className)
        {
            if (self.GeneratorCodeName == className)
            {
                return;
            }

            self.GeneratorCodeName = className;
            EditorUtility.SetDirty(self);
        }
        
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="data"></param>
        public static void SetSettingData(this ReferenceBindComponent self, AutoBindSettingData data)
        {
            if (self.SettingData == data)
            {
                return;
            }

            self.SettingData = data;
            if (self.SettingDataSearchable != null)
            {
                int findIndex = self.SettingDataSearchable.Names.ToList().FindIndex(_ => _ == data.Name);
                if (findIndex == -1)
                {
                    string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
                    string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                    var settingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
                    var settingDataNames = settingConfig.Settings.Select(_ => _.Name).ToList();
                    self.SettingDataSearchable.Select = settingDataNames.FindIndex(_ => _ == data.Name);
                    self.SettingDataSearchable.Names = settingDataNames.ToArray();
                }
                else
                {
                    self.SettingDataSearchable.Select = findIndex;
                }
            }

            EditorUtility.SetDirty(self);
        }
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        public static void SetSettingData(this ReferenceBindComponent self, string name)
        {
            self.SettingData = ComponentAutoBindToolUtility.GetAutoBindSetting(name: name);
            if (self.SettingDataSearchable != null)
            {
                int findIndex = self.SettingDataSearchable.Names.ToList().FindIndex(_ => _ == name);
                if (findIndex == -1)
                {
                    string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
                    string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                    var settingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
                    var settingDataNames = settingConfig.Settings.Select(_ => _.Name).ToList();

                    self.SettingDataSearchable.Select = settingDataNames.FindIndex(_ => _ == name);
                    self.SettingDataSearchable.Names = settingDataNames.ToArray();
                }
                else
                {
                    self.SettingDataSearchable.Select = findIndex;
                }
            }

            EditorUtility.SetDirty(self);
        }
        /// <summary>
        /// 设置生成代码配置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="names"></param>
        /// <param name="select"></param>
        public static void SetSearchable(this ReferenceBindComponent self, string[] names, int select)
        {
            self.SettingDataSearchable.Select = select;
            self.SettingDataSearchable.Names = names;
            EditorUtility.SetDirty(self);
        }

    }
}