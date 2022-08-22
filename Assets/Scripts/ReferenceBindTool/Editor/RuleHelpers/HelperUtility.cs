using System;
using System.Collections.Generic;
using System.Reflection;

namespace ReferenceBindTool.Editor
{
    public static class HelperUtility
    {
        private static readonly string[] s_AssemblyNames =
        {
#if UNITY_2017_3_OR_NEWER
            //asmdef
#endif
            "Assembly-CSharp",
            "Assembly-CSharp-Editor",
            "Assembly-CSharp-firstpass",
            "Assembly-CSharp-Editor-firstpass"
        };

        /// <summary>
        /// 获取指定基类在指定程序集中的所有子类名称
        /// </summary>
        public static string[] GetTypeNames(Type baseType)
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
                    if (type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
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

                object instance = assembly.CreateInstance(helperTypeName);
                if (instance != null)
                {
                    return instance;
                }
            }

            return null;
        }
    }
}