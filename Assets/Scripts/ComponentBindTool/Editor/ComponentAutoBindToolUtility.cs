using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ComponentAutoBindToolUtility
{
    private static readonly string[] s_AssemblyNames =
    {
#if UNITY_2017_3_OR_NEWER
        //asmdef
#endif
        "Assembly-CSharp"
    };

    /// <summary>
    /// 获取指定基类在指定程序集中的所有子类名称
    /// </summary>
    public static string[] GetTypeNames()
    {
        List<string> typeNames = new List<string>();
        foreach (string assemblyName in s_AssemblyNames)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch
            {
                continue;
            }

            if (assembly == null)
            {
                continue;
            }

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.IsClass && !type.IsAbstract && typeof(IAutoBindRuleHelper).IsAssignableFrom(type))
                {
                    typeNames.Add(type.FullName);
                }
            }
        }

        typeNames.Sort();
        return typeNames.ToArray();
    }

    /// <summary>
    /// 获取绑定的组件使用到的命名空间
    /// </summary>
    /// <param name="target">组件绑定工具</param>
    /// <returns>绑定的组件使用到的命名空间</returns>
    private static List<string> GetNameSpaces(ComponentAutoBindTool target)
    {
        List<string> nameSpaces = new List<string>();
        foreach (var bindCom in target.m_BindComs)
        {
            nameSpaces.Add(bindCom.GetType().Namespace);
        }

        return nameSpaces.Distinct().ToList();
    }

    /// <summary>
    /// 创建辅助器实例
    /// </summary>
    public static object CreateHelperInstance(string helperTypeName)
    {
        foreach (string assemblyName in s_AssemblyNames)
        {
            Assembly assembly = Assembly.Load(assemblyName);

            object instance = assembly.CreateInstance(helperTypeName);
            if (instance != null)
            {
                return instance;
            }
        }

        return null;
    }

    /// <summary>
    /// 生成自动绑定代码
    /// </summary>
    public static string GenAutoBindCode(ComponentAutoBindTool target, string className)
    {
        if (target.BindDatas.Find(_=>_.IsRepeatName)!= null)
        {
            throw new Exception("绑定组件中存在同名组件,请修改后重新生成。");
        }
        if (target.BindDatas == null || target.BindDatas.Count == 0)
        {
            throw new Exception("没有绑定组件数据。");
        }
        GameObject go = target.gameObject;

        StringBuilder stringBuilder = new StringBuilder(2048);

        List<string> nameSpaces = GetNameSpaces(target);
        foreach (var nameSpace in nameSpaces)
        {
            stringBuilder.AppendLine($"using {nameSpace};");
        }

        stringBuilder.AppendLine("");

        stringBuilder.AppendLine("//自动生成于：" + DateTime.Now);

        if (!string.IsNullOrEmpty(target.SettingData.Namespace))
        {
            //命名空间
            stringBuilder.AppendLine("namespace " + target.SettingData.Namespace);
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("");
        }

        //类名
        stringBuilder.AppendLine($"\tpublic partial class {className}");
        stringBuilder.AppendLine("\t{");
        stringBuilder.AppendLine("");

        //组件字段
        foreach (ComponentAutoBindTool.BindData data in target.BindDatas)
        {
            stringBuilder.AppendLine($"\t\tprivate {data.BindCom.GetType().Name} m_{data.Name};");
        }

        stringBuilder.AppendLine("");

        stringBuilder.AppendLine("\t\tprivate void GetBindComponents(GameObject go)");
        stringBuilder.AppendLine("\t\t{");

        //获取autoBindTool上的Component
        stringBuilder.AppendLine(
            $"\t\t\tComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();");
        stringBuilder.AppendLine("");

        //根据索引获取

        for (int i = 0; i < target.BindDatas.Count; i++)
        {
            ComponentAutoBindTool.BindData data = target.BindDatas[i];
            string filedName = $"m_{data.Name}";
            stringBuilder.AppendLine(
                $"\t\t\t{filedName} = autoBindTool.GetBindComponent<{data.BindCom.GetType().Name}>({i});");
        }

        stringBuilder.AppendLine("\t\t}");

        stringBuilder.AppendLine("\t}");

        if (!string.IsNullOrEmpty(target.SettingData.Namespace))
        {
            stringBuilder.AppendLine("}");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 生成自动绑定代码
    /// </summary>
    public static bool GenAutoBindCode(ComponentAutoBindTool target, string className, string codeFolderPath)
    {
        if (target.BindDatas.Find(_=>_.IsRepeatName)!= null)
        {
            throw new Exception("绑定组件中存在同名组件,请修改后重新生成。");
        }
        if (target.BindDatas == null || target.BindDatas.Count == 0)
        {
            throw new Exception("没有绑定组件数据。");
        }
        GameObject go = target.gameObject;

        if (!Directory.Exists(codeFolderPath))
        {
            Debug.LogError($"{go.name}的代码保存路径{codeFolderPath}无效");
            return false;
        }

        using (StreamWriter sw = new StreamWriter($"{codeFolderPath}/{className}.BindComponents.cs"))
        {
            List<string> nameSpaces = GetNameSpaces(target);
            foreach (var nameSpace in nameSpaces)
            {
                sw.WriteLine($"using {nameSpace};");
            }
            sw.WriteLine("");

            sw.WriteLine("//自动生成于：" + DateTime.Now);

            if (!string.IsNullOrEmpty(target.SettingData.Namespace))
            {
                //命名空间
                sw.WriteLine("namespace " + target.SettingData.Namespace);
                sw.WriteLine("{");
                sw.WriteLine("");
            }

            //类名
            sw.WriteLine($"\tpublic partial class {className}");
            sw.WriteLine("\t{");
            sw.WriteLine("");

            //组件字段
            foreach (ComponentAutoBindTool.BindData data in target.BindDatas)
            {
                sw.WriteLine($"\t\tprivate {data.BindCom.GetType().Name} m_{data.Name};");
            }

            sw.WriteLine("");

            sw.WriteLine("\t\tprivate void GetBindComponents(GameObject go)");
            sw.WriteLine("\t\t{");

            //获取autoBindTool上的Component
            sw.WriteLine(
                $"\t\t\tComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();");
            sw.WriteLine("");

            //根据索引获取

            for (int i = 0; i < target.BindDatas.Count; i++)
            {
                ComponentAutoBindTool.BindData data = target.BindDatas[i];
                string filedName = $"m_{data.Name}";
                sw.WriteLine(
                    $"\t\t\t{filedName} = autoBindTool.GetBindComponent<{data.BindCom.GetType().Name}>({i});");
            }

            sw.WriteLine("\t\t}");

            sw.WriteLine("\t}");

            if (!string.IsNullOrEmpty(target.SettingData.Namespace))
            {
                sw.WriteLine("}");
            }
        }

        AssetDatabase.Refresh();
        return true;
    }

    /// <summary>
    /// 自动绑定
    /// </summary>
    /// <param name="target"></param>
    /// <param name="ruleHelper"></param>
    public static void AutoBindComponents(GameObject target, IAutoBindRuleHelper ruleHelper = null)
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage == null)
        {
            ComponentAutoBindTool bindTool = target.GetOrAddComponent<ComponentAutoBindTool>();
            if (ruleHelper != null)
            {
                bindTool.RuleHelper = ruleHelper;
            }

            if (bindTool.RuleHelper == null)
            {
                bindTool.RuleHelper = (IAutoBindRuleHelper) CreateHelperInstance(GetTypeNames()[0]);
            }

            bindTool.AutoBindComponent();
            PrefabInstanceStatus status = PrefabUtility.GetPrefabInstanceStatus(target);
            if (status == PrefabInstanceStatus.Connected)
            {
                PrefabUtility.ApplyPrefabInstance(target, InteractionMode.AutomatedAction);
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        else
        {
            ComponentAutoBindTool bindTool = prefabStage.prefabContentsRoot.GetOrAddComponent<ComponentAutoBindTool>();
            if (ruleHelper != null)
            {
                bindTool.RuleHelper = ruleHelper;
            }

            if (bindTool.RuleHelper == null)
            {
                bindTool.RuleHelper = (IAutoBindRuleHelper) CreateHelperInstance(GetTypeNames()[0]);
            }

            bindTool.AutoBindComponent();
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
    }

    /// <summary>
    /// 设置代码生成配置
    /// </summary>
    /// <param name="nameSpace"></param>
    /// <param name="path"></param>
    /// <param name="isAutoCreateDir"></param>
    public static void SetAutoBindSetting(string name, string nameSpace, string path, bool isAutoCreateDir)
    {
        string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
        if (paths.Length == 0)
        {
            Debug.LogError("不存在AutoBindSettingConfig");
            return;
        }

        if (paths.Length > 1)
        {
            Debug.LogError("AutoBindSettingConfig数量大于1");
            return;
        }

        string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);

        if (!Directory.Exists(path) && isAutoCreateDir)
        {
            Directory.CreateDirectory(path);
        }

        var setting = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(settingPath);
        var settingData = setting.GetSettingData(name);
        settingData.Namespace = nameSpace;
        settingData.CodePath = path;

        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    ///   获取代码生成配置
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static AutoBindSettingData GetAutoBindSetting(string name)
    {
        string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
        if (paths.Length == 0)
        {
            throw new Exception("不存在AutoBindSettingConfig");
        }

        if (paths.Length > 1)
        {
            throw new Exception("AutoBindSettingConfig数量大于1");
        }

        string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        var setting = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(settingPath);
        return setting.GetSettingData(name);
    }

    /// <summary>
    ///   获取代码生成配置
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool AddAutoBindSetting(string name, string folder, string nameSpace)
    {
        string[] paths = AssetDatabase.FindAssets("t:AutoBindSettingConfig");
        if (paths.Length == 0)
        {
            throw new Exception("不存在AutoBindSettingConfig");
        }

        if (paths.Length > 1)
        {
            throw new Exception("AutoBindSettingConfig数量大于1");
        }

        string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        var setting = AssetDatabase.LoadAssetAtPath<AutoBindSettingConfig>(settingPath);
        bool result =setting.AddSettingData(new AutoBindSettingData(name, folder, nameSpace));
        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
        return result;
    }
}