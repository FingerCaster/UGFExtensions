using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extensions.DataTableExtension.Editor;
using GameFramework;
using UnityEngine;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        public static class DataProcessorUtility
        {
            private static readonly IDictionary<string, DataProcessor> s_DataProcessors =
                new SortedDictionary<string, DataProcessor>();

            static DataProcessorUtility()
            {
                var dataProcessorBaseType = typeof(DataProcessor);

                var types = Assembly.GetExecutingAssembly().GetTypes();
                var addList = new List<DataProcessor>();
                for (var i = 0; i < types.Length; i++)
                {
                    if (!types[i].IsClass || types[i].IsAbstract || types[i].ContainsGenericParameters) continue;

                    if (dataProcessorBaseType.IsAssignableFrom(types[i]))
                    {
                        DataProcessor dataProcessor = null;
                        dataProcessor = (DataProcessor) Activator.CreateInstance(types[i]);
                        if (dataProcessor.IsEnum)
                        {
                            continue;
                        }
                        foreach (var typeString in dataProcessor.GetTypeStrings())
                            s_DataProcessors.Add(typeString.ToLower(), dataProcessor);

                        addList.Add(dataProcessor);
                    }
                }
                AddEnumType(addList);
                AddListType(addList);
                AddArrayType(addList);
                AddDictionary(addList);
                
            }
            
            private static void AddEnumType(List<DataProcessor> addList)
            {
                foreach (var assemblyName in DataTableConfig.GetDataTableConfig().AssemblyNames)
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

                    if (assembly == null) continue;

                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsEnum)
                        {
                            Type enumProcessorType = typeof(EnumProcessor<>).MakeGenericType(type);
                            DataProcessor dataProcessor =  (DataProcessor) Activator.CreateInstance(enumProcessorType);
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            {
                                if (s_DataProcessors.ContainsKey(typeString))
                                {
                                    StringBuilder stringBuilder = new StringBuilder(256);
                                    stringBuilder.AppendLine($"程序集:{type.Assembly.GetName().Name} 存在同名枚举:{type.FullName}");
                                    DataProcessor repeatProcessor = s_DataProcessors[typeString];
                                    if (repeatProcessor.GetType().GetProperty("EnumType")?.GetValue(repeatProcessor) is Type repeatType)
                                        stringBuilder.AppendLine($"程序集:{repeatType.Assembly.GetName().Name} 存在同名枚举:{type.FullName}");
                                    throw new Exception("不同程序集中存在同名枚举,请修改后重试.\n" + stringBuilder);
                                    
                                }
                                s_DataProcessors.Add(typeString.ToLower(), dataProcessor);
                            }
                            addList.Add(dataProcessor);
                        }
                    }
                }
            }

            private static void AddArrayType(List<DataProcessor> addList)
            {
                var dataProcessorBaseType = typeof(DataProcessor);

                var type = typeof(ArrayProcessor<,>);

                for (var i = 0; i < addList.Count; i++)
                {
                    Type dataProcessorType = addList[i].GetType();
                    if (!dataProcessorType.HasImplementedRawGeneric(typeof(GenericDataProcessor<>))) continue;

                    var memberInfo = dataProcessorType.BaseType;

                    if (memberInfo != null)
                    {
                        Type[] typeArgs =
                        {
                            dataProcessorType,
                            memberInfo.GenericTypeArguments[0]
                        };
                        var arrayType = type.MakeGenericType(typeArgs);
                        if (dataProcessorBaseType.IsAssignableFrom(arrayType))
                        {
                            var dataProcessor = (DataProcessor) Activator.CreateInstance(arrayType);
                            var tDataProcessor = addList[i];
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            foreach (var tTypeString in tDataProcessor.GetTypeStrings())
                            {
                                var key = Utility.Text.Format(typeString.ToLower(), tTypeString);
                                s_DataProcessors.Add(key, dataProcessor);
                            }
                        }
                    }
                }
            }

            private static void AddListType(List<DataProcessor> addList)
            {
                var dataProcessorBaseType = typeof(DataProcessor);

                var type = typeof(ListProcessor<,>);

                for (var i = 0; i < addList.Count; i++)
                {
                    Type dataProcessorType = addList[i].GetType();

                    if (!dataProcessorType.HasImplementedRawGeneric(typeof(GenericDataProcessor<>))) continue;

                    var memberInfo = dataProcessorType.BaseType;

                    if (memberInfo != null)
                    {
                        Type[] typeArgs =
                        {
                            dataProcessorType,
                            memberInfo.GenericTypeArguments[0]
                        };
                        var listType = type.MakeGenericType(typeArgs);
                        if (dataProcessorBaseType.IsAssignableFrom(listType))
                        {
                            var dataProcessor =
                                (DataProcessor) Activator.CreateInstance(listType);
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            foreach (var tTypeString in addList[i].GetTypeStrings())
                            {
                                var key = Utility.Text.Format(typeString.ToLower(), tTypeString);
                                s_DataProcessors.Add(key, dataProcessor);
                            }
                        }
                    }
                }
            }

            private static void AddDictionary(List<DataProcessor> addList)
            {
                var dataProcessorBaseType = typeof(DataProcessor);
                var type = typeof(DictionaryProcessor<,,,>);
                var list = new List<DataProcessor>();
                for (var i = 0; i < addList.Count; i++)
                {
                    Type dataProcessorType = addList[i].GetType();
            
                    if (!dataProcessorType.HasImplementedRawGeneric(typeof(GenericDataProcessor<>))) continue;
                    var memberInfo = dataProcessorType.BaseType;
            
                    if (memberInfo != null) list.Add(addList[i]);
                }
            
            
                var keyValueList = PermutationAndCombination<DataProcessor>.GetCombination(list.ToArray(), 2).ToList();
                var reverseList = keyValueList.Select(types => new[] {types[1], types[0]}).ToList();
                keyValueList.AddRange(reverseList);
                foreach (var value in list) keyValueList.Add(new[] {value, value});
                foreach (var keyValue in keyValueList)
                {
                    var keyType = keyValue[0].GetType().BaseType;
                    var valueType = keyValue[1].GetType().BaseType;
                    if (keyType != null && valueType != null)
                    {
                        
                        Type[] typeArgs =
                        {
                            keyValue[0].GetType(),
                            keyValue[1].GetType(),
                            keyType.GenericTypeArguments[0],
                            valueType.GenericTypeArguments[0]
                        };
                        var dictionaryType = type.MakeGenericType(typeArgs);
                        if (dataProcessorBaseType.IsAssignableFrom(dictionaryType))
                        {
                            var dataProcessor = (DataProcessor) Activator.CreateInstance(dictionaryType);
                            foreach (var typeString in dataProcessor.GetTypeStrings())
                            {
                                foreach (var key in keyValue[0].GetTypeStrings())
                                {
                                    foreach (var value in keyValue[1].GetTypeStrings())
                                    {
                                        var str = Utility.Text.Format(typeString.ToLower(), key, value);
                                        s_DataProcessors.Add(str, dataProcessor);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public static DataProcessor GetDataProcessor(string type)
            {
                if (type == null) type = string.Empty;
                DataProcessor dataProcessor = null;
                if (s_DataProcessors.TryGetValue(type.ToLower(), out dataProcessor)) return dataProcessor;

                throw new GameFrameworkException(Utility.Text.Format("Not supported data processor type '{0}'.", type));
            }
        }
    }
}