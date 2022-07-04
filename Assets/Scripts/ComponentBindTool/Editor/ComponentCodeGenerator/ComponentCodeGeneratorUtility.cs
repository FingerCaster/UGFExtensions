using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ComponentCodeGenerator
{
    public class ComponentCodeGeneratorUtility
    {
        /// <summary>
        /// 获取绑定的组件使用到的命名空间
        /// </summary>
        /// <param name="target">组件绑定工具</param>
        /// <returns>绑定的组件使用到的命名空间</returns>
        private static List<string> GetNameSpaces(ComponentCodeGenData target)
        {
            List<string> nameSpaces = new List<string>();
            foreach (var bindData in target.BindDataList)
            {
                nameSpaces.Add(bindData.BindCom.GetType().Namespace);
            }

            return nameSpaces.Distinct().ToList();
        }

        private static string GetPath(Transform root,Transform transform)
        {
            StringBuilder stringBuilder = new StringBuilder(1024);
            while (transform!=root)
            {
                stringBuilder.Insert(0, transform.name);
                if (transform.parent != null)
                {
                    transform = transform.parent;
                    if (transform !=root)
                    {
                        stringBuilder.Insert(0, '/');
                    }
                }
                else
                {
                    throw new Exception($"{transform.name} 不在 {root.name} 的子孙节点中。");
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 生成自动绑定代码
        /// </summary>
        public static string GenAutoBindCode(ComponentCodeGenData target, string className)
        {
            if (target.BindDataList.Find(_ => _.IsRepeatName) != null)
            {
                throw new Exception("绑定组件中存在同名组件,请修改后重新生成。");
            }

            if (target.BindDataList == null || target.BindDataList.Count == 0)
            {
                throw new Exception("没有绑定组件数据。");
            }
            
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
            }

            //类名
            stringBuilder.AppendLine($"\tpublic class {className}");
            stringBuilder.AppendLine("\t{");
            
            stringBuilder.AppendLine($"\t\tprivate Transform m_Transform;");
            stringBuilder.AppendLine($"\t\tpublic {className}(Transform transform)");
            stringBuilder.AppendLine("\t\t{");
            stringBuilder.AppendLine("\t\t\tm_Transform = transform;");
            stringBuilder.AppendLine("\t\t}");
            //组件字段
            foreach (ComponentCodeGenData.BindData data in target.BindDataList)
            {
                stringBuilder.AppendLine($"\t\tprivate {data.BindCom.GetType().Name} m_{data.Name};");
            }
            
            //属性字段
            foreach (ComponentCodeGenData.BindData data in target.BindDataList)
            {
                stringBuilder.AppendLine($"\t\tpublic {data.BindCom.GetType().Name} {data.Name}");
                stringBuilder.AppendLine("\t\t{");
                stringBuilder.AppendLine("\t\t\tget");
                stringBuilder.AppendLine("\t\t\t{");
                stringBuilder.AppendLine($"\t\t\t\tif(m_{data.Name} == null)");
                stringBuilder.AppendLine("\t\t\t\t{");
                string path = GetPath(target.GameObject.transform, data.BindCom.transform);
                if (string.IsNullOrEmpty(path))
                {
                    stringBuilder.AppendLine($"\t\t\t\t\tm_{data.Name} = m_Transform.GetComponent<{data.BindCom.GetType().Name}>();");
                }
                else
                {
                    stringBuilder.AppendLine($"\t\t\t\t\tm_{data.Name} = m_Transform.Find(\"{path}\").GetComponent<{data.BindCom.GetType().Name}>();");
                }

                stringBuilder.AppendLine("\t\t\t\t}");
                stringBuilder.AppendLine($"\t\t\t\treturn m_{data.Name};");
                stringBuilder.AppendLine("\t\t\t}");
                stringBuilder.AppendLine("\t\t}");
            }

            stringBuilder.AppendLine("");

         

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
        public static bool GenAutoBindCode(ComponentCodeGenData target, string className, string codeFolderPath)
        {
            if (!Directory.Exists(codeFolderPath))
            {
                Debug.LogError($"{target.GameObject.name}的代码保存路径{codeFolderPath}无效");
                return false;
            }

            using (StreamWriter sw = new StreamWriter($"{codeFolderPath}/{className}.cs"))
            {
                string str = GenAutoBindCode(target, className);
                sw.Write(str);
            }

            AssetDatabase.Refresh();
            Debug.Log($"代码生成成功,生成路径: {codeFolderPath}/{className}.cs");
            return true;
        }
    }
}