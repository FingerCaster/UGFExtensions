using System;
using System.Collections.Generic;
using System.Linq;
using RoboRyanTron.SearchableEnum;
using UnityEditor;
using UnityEngine;

public static class ComponentAutoBindToolExtensions
{
    /// <summary>
    /// 同步绑定数据
    /// </summary>
    public static void SyncBindComponents(this ComponentAutoBindTool self)
    {
        self.m_BindComs.Clear();
        for (int i = 0; i < self.BindDatas.Count; i++)
        {
            ComponentAutoBindTool.BindData bindData = self.BindDatas[i];
            self.m_BindComs.Add(bindData.BindCom);
        }

        EditorUtility.SetDirty(self);
    }

    /// <summary>
    /// 添加绑定数据
    /// </summary>
    public static void AddBindData(this ComponentAutoBindTool self, string name, Component bindCom)
    {
        bool isRepeat = false;
        for (int j = 0; j < self.BindDatas.Count; j++)
        {
            if (self.BindDatas[j].Name == name)
            {
                isRepeat = true;
                break;
            }
        }

        self.BindDatas.Add(new ComponentAutoBindTool.BindData(isRepeat, name, bindCom));
        EditorUtility.SetDirty(self);
    }

    /// <summary>
    /// 删除空引用
    /// </summary>
    public static void RemoveNull(this ComponentAutoBindTool self)
    {
        for (int i = self.BindDatas.Count - 1; i >= 0; i--)
        {
            ComponentAutoBindTool.BindData bindData = self.BindDatas[i];

            if (bindData.BindCom == null)
            {
                self.BindDatas.RemoveAt(i);
            }
        }

        self.SyncBindComponents();
    }

    /// <summary>
    /// 全部删除
    /// </summary>
    public static void RemoveAll(this ComponentAutoBindTool self)
    {
        self.BindDatas.Clear();
        self.SyncBindComponents();
    }


    /// <summary>
    /// 自动绑定组件
    /// </summary>
    public static void AutoBindComponent(this ComponentAutoBindTool self)
    {
        self.BindDatas.Clear();
        List<string> tempFiledNames = new List<string>();
        List<Component> tempComponentTypeNames = new List<Component>();
        Transform[] children = self.gameObject.GetComponentsInChildren<Transform>(true);
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

        self.SyncBindComponents();
    }

    /// <summary>
    /// 排序
    /// </summary>
    public static void Sort(this ComponentAutoBindTool self)
    {
        List<ComponentAutoBindTool.BindData> tempList = new List<ComponentAutoBindTool.BindData>();
        foreach (ComponentAutoBindTool.BindData data in self.BindDatas)
        {
            tempList.Add(new ComponentAutoBindTool.BindData(data.IsRepeatName, data.Name, data.BindCom));
        }

        tempList.Sort((x, y) => { return string.Compare(x.Name, y.Name, StringComparison.Ordinal); });

        self.BindDatas.Clear();
        foreach (ComponentAutoBindTool.BindData data in tempList)
        {
            self.AddBindData(data.Name, data.BindCom);
        }

        self.SyncBindComponents();
    }

    /// <summary>
    /// 设置脚本名称
    /// </summary>
    /// <param name="self"></param>
    /// <param name="className"></param>
    public static void SetClassName(this ComponentAutoBindTool self, string className)
    {
        if (self.ClassName == className)
        {
            return;
        }

        self.ClassName = className;
        EditorUtility.SetDirty(self);
    }

    /// <summary>
    /// 设置生成规则帮助类
    /// </summary>
    /// <param name="self"></param>
    /// <param name="ruleHelper"></param>
    public static void SetRuleHelper(this ComponentAutoBindTool self, IAutoBindRuleHelper ruleHelper)
    {
        if (self.RuleHelper == ruleHelper)
        {
            return;
        }

        self.RuleHelper = ruleHelper;
        EditorUtility.SetDirty(self);
    }

    /// <summary>
    /// 设置生成代码配置
    /// </summary>
    /// <param name="self"></param>
    /// <param name="data"></param>
    public static void SetSettingData(this ComponentAutoBindTool self, AutoBindSettingData data)
    {
        if (self.SettingData == data)
        {
            return;
        }

        self.SettingData = data;
        if (self.Searchable != null)
        {
            int findIndex = self.Searchable.Names.ToList().FindIndex(_ => _ == data.Name);
            if (findIndex == -1)
            {
                string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                var settingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
                var settingDataNames = settingConfig.Settings.Select(_ => _.Name).ToList();
                self.Searchable.Select = settingDataNames.FindIndex(_ => _ == data.Name);
                self.Searchable.Names = settingDataNames.ToArray();
            }
            else
            {
                self.Searchable.Select = findIndex;
            }
        }

        EditorUtility.SetDirty(self);
    }

    /// <summary>
    /// 设置生成代码配置
    /// </summary>
    /// <param name="self"></param>
    /// <param name="data"></param>
    public static void SetSettingData(this ComponentAutoBindTool self, string name)
    {
        self.SettingData = ComponentAutoBindToolUtility.GetAutoBindSetting(name: name);
        if (self.Searchable != null)
        {
            int findIndex = self.Searchable.Names.ToList().FindIndex(_ => _ == name);
            if (findIndex == -1)
            {
                string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
                string path = AssetDatabase.GUIDToAssetPath(paths[0]);
                var settingConfig = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(path);
                var settingDataNames = settingConfig.Settings.Select(_ => _.Name).ToList();

                self.Searchable.Select = settingDataNames.FindIndex(_ => _ == name);
                self.Searchable.Names = settingDataNames.ToArray();
            }
            else
            {
                self.Searchable.Select = findIndex;
            }
        }

        EditorUtility.SetDirty(self);
    }

    /// <summary>
    /// 设置生成代码配置
    /// </summary>
    /// <param name="self"></param>
    /// <param name="data"></param>
    public static void SetSearchable(this ComponentAutoBindTool self, string[] names, int select)
    {
        self.Searchable.Select = select;
        self.Searchable.Names = names;
        EditorUtility.SetDirty(self);
    }
}