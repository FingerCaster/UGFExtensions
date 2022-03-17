using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ComponentAutoBindToolUtility
{
    private static readonly string[] s_AssemblyNames = {"Assembly-CSharp"};

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
        GameObject go = target.gameObject;

        StringBuilder stringBuilder = new StringBuilder(2048);

        stringBuilder.AppendLine("using UnityEngine;");
        stringBuilder.AppendLine("using UnityEngine.UI;");
        stringBuilder.AppendLine("");

        stringBuilder.AppendLine("//自动生成于：" + DateTime.Now);

        if (!string.IsNullOrEmpty(target.Namespace))
        {
            //命名空间
            stringBuilder.AppendLine("namespace " + target.Namespace);
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

        if (!string.IsNullOrEmpty(target.Namespace))
        {
            stringBuilder.AppendLine("}");
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 生成自动绑定代码
    /// </summary>
    public static bool GenAutoBindCode(ComponentAutoBindTool target, string className, string codePath)
    {
        GameObject go = target.gameObject;

        if (!Directory.Exists(codePath))
        {
            Debug.LogError($"{go.name}的代码保存路径{codePath}无效");
            return false;
        }

        using (StreamWriter sw = new StreamWriter($"{codePath}/{className}.BindComponents.cs"))
        {
            sw.WriteLine("using UnityEngine;");
            sw.WriteLine("using UnityEngine.UI;");
            sw.WriteLine("");

            sw.WriteLine("//自动生成于：" + DateTime.Now);

            if (!string.IsNullOrEmpty(target.Namespace))
            {
                //命名空间
                sw.WriteLine("namespace " + target.Namespace);
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

            if (!string.IsNullOrEmpty(target.Namespace))
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
    public static void AutoBindComponents(GameObject target,IAutoBindRuleHelper ruleHelper = null)
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
                bindTool.RuleHelper = (IAutoBindRuleHelper)CreateHelperInstance(GetTypeNames()[0]);
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
                bindTool.RuleHelper = (IAutoBindRuleHelper)CreateHelperInstance(GetTypeNames()[0]);
            }
            bindTool.AutoBindComponent();
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }

    }

    /// <summary>
    /// 设置全局设置
    /// </summary>
    /// <param name="nameSpace"></param>
    /// <param name="path"></param>
    /// <param name="isAutoCreateDir"></param>
    public static void SetAutoBindGlobeSetting(string nameSpace, string path, bool isAutoCreateDir)
    {
        string[] paths = AssetDatabase.FindAssets("t:AutoBindGlobalSetting");
        if (paths.Length == 0)
        {
            Debug.LogError("不存在AutoBindGlobalSetting");
            return;
        }
        if (paths.Length > 1)
        {
            Debug.LogError("AutoBindGlobalSetting数量大于1");
            return;
        }
        string settingPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        
        if (!Directory.Exists(path) && isAutoCreateDir)
        {
            Directory.CreateDirectory(path);
        }

        var setting = AssetDatabase.LoadAssetAtPath<AutoBindGlobalSetting>(settingPath);
        setting.Namespace = nameSpace;
        setting.CodePath = path;

        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
    }
}