using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GameFramework;
using UnityEngine;

namespace DE.Editor.DataTableTools
{
    public sealed class DataTableGenerator
    {
        private static readonly Regex EndWithNumberRegex = new Regex(@"\d+$");
        private static readonly Regex NameRegex = new Regex(@"^[A-Z][A-Za-z0-9_]*$");
        private static List<string> _nameSpace = new List<string>();

        public static DataTableProcessor CreateDataTableProcessor(string dataTableName)
        {
            return new DataTableProcessor(
                Utility.Path.GetRegularPath(Path.Combine(DataTableConfig.DataTableFolderPath, dataTableName + ".txt")),
                Encoding.UTF8, 1, 2,
                null, 3, 4, 1);
        }
        
        public static DataTableProcessor CreateExcelDataTableProcessor(string dataTableName)
        {
            return new DataTableProcessor(
                Utility.Path.GetRegularPath(Path.Combine(DataTableConfig.ExcelsFolder, dataTableName + ".xlsx")),
                1, 2,
                null, 3, 4, 1);
        }

        public static bool CheckRawData(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                var name = dataTableProcessor.GetName(i);
                if (string.IsNullOrEmpty(name) || name == "#") continue;

                if (!NameRegex.IsMatch(name))
                {
                    Debug.LogWarning(Utility.Text.Format("Check raw data failure. DataTableName='{0}' Name='{1}'",
                        dataTableName, name));
                    return false;
                }
            }

            return true;
        }

        public static void GenerateDataFile(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            var binaryDataFileName =
                Utility.Path.GetRegularPath(Path.Combine(DataTableConfig.DataTableFolderPath,
                    dataTableName + ".bytes"));
            if (!dataTableProcessor.GenerateDataFile(binaryDataFileName) && File.Exists(binaryDataFileName))
                File.Delete(binaryDataFileName);
        }

        public static void GenerateCodeFile(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            dataTableProcessor.SetCodeTemplate(DataTableConfig.CSharpCodeTemplateFileName, Encoding.UTF8);
            dataTableProcessor.SetCodeGenerator(DataTableCodeGenerator);
            bool isChanged = CheckIsChanged(dataTableProcessor, dataTableName);
            if (!isChanged)
            {
                Debug.Log($"DR{dataTableName} is not Changed,don't have to regenerate it");
                return;
            }

            var csharpCodeFileName =
                Utility.Path.GetRegularPath(Path.Combine(DataTableConfig.CSharpCodePath, "DR" + dataTableName + ".cs"));
            if (!dataTableProcessor.GenerateCodeFile(csharpCodeFileName, Encoding.UTF8, dataTableName) &&
                File.Exists(csharpCodeFileName))
                File.Delete(csharpCodeFileName);
        }

        private static bool CheckIsChanged(DataTableProcessor dataTableProcessor, string dataTableName)
        {
            string oldCsharpCodePath = Path.Combine(DataTableConfig.CSharpCodePath, "DR" + dataTableName + ".cs");
            if (!File.Exists(oldCsharpCodePath))
            {
                return true;
            }
            var stringBuilder = new StringBuilder(File.ReadAllText(DataTableConfig.CSharpCodeTemplateFileName, Encoding.UTF8));
            DataTableCodeGenerator(dataTableProcessor, stringBuilder, dataTableName);
            string csharpCode = GetNotHeadString(stringBuilder.ToString());
            string oldCsharpCode = GetNotHeadString(File.ReadAllText(oldCsharpCodePath));
            return csharpCode != oldCsharpCode;
        }
        static string GetNotHeadString(string str)
        {
            int index = str.IndexOf("using", StringComparison.Ordinal);
            str = str.Substring(index);
            return str;
        }

        private static void DataTableCodeGenerator(DataTableProcessor dataTableProcessor,
            StringBuilder codeContent, object userData)
        {
            var dataTableName = (string) userData;

            codeContent.Replace("__DATA_TABLE_CREATE_TIME__", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            codeContent.Replace("__DATA_TABLE_NAME_SPACE__", DataTableConfig.NameSpace);
            codeContent.Replace("__DATA_TABLE_CLASS_NAME__", "DR" + dataTableName);
            codeContent.Replace("__DATA_TABLE_COMMENT__", dataTableProcessor.GetValue(0, 1) + "。");
            codeContent.Replace("__DATA_TABLE_ID_COMMENT__",
                "获取" + dataTableProcessor.GetComment(dataTableProcessor.IdColumn) + "。");
            codeContent.Replace("__DATA_TABLE_PROPERTIES__", GenerateDataTableProperties(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_PARSER__", GenerateDataTableParser(dataTableProcessor));
            codeContent.Replace("__DATA_TABLE_PROPERTY_ARRAY__", GenerateDataTablePropertyArray(dataTableProcessor));
            _nameSpace = _nameSpace.Distinct().ToList();
            StringBuilder nameSpaceBuilder = new StringBuilder();
            foreach (string nameSpace in _nameSpace)
            {
                nameSpaceBuilder.AppendLine($"using {nameSpace};");
            }

            codeContent.Replace("__DATA_TABLE_PROPERTIES_NAMESPACE__", nameSpaceBuilder.ToString());
        }

        private static string GenerateDataTableProperties(DataTableProcessor dataTableProcessor)
        {
            var stringBuilder = new StringBuilder();
            var firstProperty = true;
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                if (dataTableProcessor.IsCommentColumn(i))
                    // 注释列
                    continue;

                if (dataTableProcessor.IsIdColumn(i))
                    // 编号列
                    continue;

                if (firstProperty)
                    firstProperty = false;
                else
                    stringBuilder.AppendLine().AppendLine();

                stringBuilder
                    .AppendLine("        /// <summary>")
                    .AppendFormat("        /// 获取{0}。", dataTableProcessor.GetComment(i)).AppendLine()
                    .AppendLine("        /// </summary>")
                    .AppendFormat("        public {0} {1}", dataTableProcessor.GetLanguageKeyword(i),
                        dataTableProcessor.GetName(i)).AppendLine()
                    .AppendLine("        {")
                    .AppendLine("            get;")
                    .AppendLine("            private set;")
                    .Append("        }");
            }

            return stringBuilder.ToString();
        }

        private static string GenerateDataTableParser(DataTableProcessor dataTableProcessor)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine("        public override bool ParseDataRow(string dataRowString, object userData)")
                .AppendLine("        {")
                .AppendLine(
                    "            string[] columnStrings = dataRowString.Split(DataTableExtension.DataSplitSeparators);")
                .AppendLine("            for (int i = 0; i < columnStrings.Length; i++)")
                .AppendLine("            {")
                .AppendLine(
                    "                columnStrings[i] = columnStrings[i].Trim(DataTableExtension.DataTrimSeparators);")
                .AppendLine("            }")
                .AppendLine()
                .AppendLine("            int index = 0;");
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                if (dataTableProcessor.IsCommentColumn(i))
                {
                    // 注释列
                    stringBuilder.AppendLine("            index++;");
                    continue;
                }

                if (dataTableProcessor.IsIdColumn(i))
                {
                    // 编号列
                    stringBuilder.AppendLine("            m_Id = int.Parse(columnStrings[index++]);");
                    continue;
                }

                if (dataTableProcessor.IsSystem(i))
                {
                    var languageKeyword = dataTableProcessor.GetLanguageKeyword(i);

                    if (languageKeyword == "string")
                        stringBuilder
                            .AppendFormat("\t\t\t{0} = columnStrings[index++];", dataTableProcessor.GetName(i))
                            .AppendLine();
                    else
                        stringBuilder.AppendFormat("\t\t\t{0} = {1}.Parse(columnStrings[index++]);",
                            dataTableProcessor.GetName(i), languageKeyword).AppendLine();
                }
                else
                {
                    if (dataTableProcessor.IsListColumn(i))
                    {
                        var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                        var dataProcessor = Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                        string typeName = dataProcessor.Type.Name;

                        if (dataProcessor.IsEnum)
                        {
                            typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor
                                .LanguageKeyword);
                        }

                        stringBuilder
                            .AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}List(columnStrings[index++]);",
                                dataTableProcessor.GetName(i), typeName).AppendLine();
                        continue;
                    }

                    if (dataTableProcessor.IsArrayColumn(i))
                    {
                        var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                        var dataProcessor =
                            Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                        string typeName = dataProcessor.Type.Name;

                        if (dataProcessor.IsEnum)
                        {
                            typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor
                                .LanguageKeyword);
                        }

                        stringBuilder
                            .AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}Array(columnStrings[index++]);",
                                dataTableProcessor.GetName(i), typeName).AppendLine();
                        continue;
                    }

                    if (dataTableProcessor.IsDictionaryColumn(i))
                    {
                        var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                        var dataProcessorT1 =
                            Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                        var dataProcessorT2 =
                            Activator.CreateInstance(t[1]) as DataTableProcessor.DataProcessor;
                        var dataProcessorT1TypeName = dataProcessorT1.Type.Name;
                        if (dataProcessorT1.IsEnum)
                        {
                            dataProcessorT1TypeName =
                                DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT1.LanguageKeyword);
                        }

                        var dataProcessorT2TypeName = dataProcessorT2.Type.Name;
                        if (dataProcessorT2.IsEnum)
                        {
                            dataProcessorT2TypeName =
                                DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT2.LanguageKeyword);
                        }

                        stringBuilder.AppendFormat(
                                "\t\t\t{0} = DataTableExtension.Parse{1}{2}Dictionary(columnStrings[index++]);",
                                dataTableProcessor.GetName(i), dataProcessorT1TypeName, dataProcessorT2TypeName)
                            .AppendLine();
                        continue;
                    }

                    if (dataTableProcessor.IsEnumrColumn(i))
                    {
                        stringBuilder.AppendLine(
                            $"\t\t\t{dataTableProcessor.GetName(i)} = DataTableExtension.EnumParse<{dataTableProcessor.GetLanguageKeyword(i)}>(columnStrings[index++]);");
                        continue;
                    }

                    stringBuilder.AppendFormat("\t\t\t{0} = DataTableExtension.Parse{1}(columnStrings[index++]);",
                        dataTableProcessor.GetName(i), dataTableProcessor.GetType(i).Name).AppendLine();
                }
            }

            stringBuilder
                .AppendLine("            GeneratePropertyArray();")
                .AppendLine("            return true;")
                .AppendLine("        }")
                .AppendLine()
                .AppendLine(
                    "        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)")
                .AppendLine("        {")
                .AppendLine(
                    "            using (MemoryStream memoryStream = new MemoryStream(dataRowBytes, startIndex, length, false))")
                .AppendLine("            {")
                .AppendLine(
                    "                using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))")
                .AppendLine("                {");

            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                if (dataTableProcessor.IsCommentColumn(i))
                    // 注释列
                    continue;

                if (dataTableProcessor.IsIdColumn(i))
                {
                    // 编号列
                    stringBuilder.AppendLine("                    m_Id = binaryReader.Read7BitEncodedInt32();");
                    continue;
                }

                if (dataTableProcessor.IsIdColumn(i))
                {
                    // 编号列
                    stringBuilder.AppendLine("                m_Id = binaryReader.ReadInt32();");
                    continue;
                }

                var languageKeyword = dataTableProcessor.GetLanguageKeyword(i);
                if (dataTableProcessor.IsListColumn(i))
                {
                    var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                    var dataProcessor =
                        Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                    string typeName = dataProcessor.Type.Name;
                    if (dataProcessor.IsEnum)
                    {
                        typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor.LanguageKeyword);
                    }

                    stringBuilder.AppendFormat("\t\t\t\t\t{0} = binaryReader.Read{1}List();",
                        dataTableProcessor.GetName(i), typeName).AppendLine();
                    continue;
                }

                if (dataTableProcessor.IsArrayColumn(i))
                {
                    var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                    var dataProcessor = Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                    string typeName = dataProcessor.Type.Name;

                    if (dataProcessor.IsEnum)
                    {
                        typeName = DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessor.LanguageKeyword);
                    }

                    stringBuilder.AppendFormat("\t\t\t\t\t{0} = binaryReader.Read{1}Array();",
                        dataTableProcessor.GetName(i), typeName).AppendLine();
                    continue;
                }

                if (dataTableProcessor.IsDictionaryColumn(i))
                {
                    var t = dataTableProcessor.GetDataProcessor(i).GetType().GetGenericArguments();
                    var dataProcessorT1 =
                        Activator.CreateInstance(t[0]) as DataTableProcessor.DataProcessor;
                    var dataProcessorT2 =
                        Activator.CreateInstance(t[1]) as DataTableProcessor.DataProcessor;
                    var dataProcessorT1TypeName = dataProcessorT1.Type.Name;
                    if (dataProcessorT1.IsEnum)
                    {
                        dataProcessorT1TypeName =
                            DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT1.LanguageKeyword);
                    }

                    var dataProcessorT2TypeName = dataProcessorT2.Type.Name;
                    if (dataProcessorT2.IsEnum)
                    {
                        dataProcessorT2TypeName =
                            DataTableProcessorExtensions.GetFullNameWithNotDot(dataProcessorT2.LanguageKeyword);
                    }

                    stringBuilder.AppendFormat("\t\t\t\t\t{0} = binaryReader.Read{1}{2}Dictionary();",
                            dataTableProcessor.GetName(i), dataProcessorT1TypeName, dataProcessorT2TypeName)
                        .AppendLine();
                    continue;
                }

                if (dataTableProcessor.IsEnumrColumn(i))
                {
                    stringBuilder.AppendFormat("\t\t\t\t\t{0} = ({1})binaryReader.Read7BitEncodedInt32();",
                        dataTableProcessor.GetName(i), dataTableProcessor.GetLanguageKeyword(i)).AppendLine();
                    continue;
                }


                if (languageKeyword == "int" || languageKeyword == "uint" || languageKeyword == "long" ||
                    languageKeyword == "ulong")
                    stringBuilder.AppendFormat("                    {0} = binaryReader.Read7BitEncoded{1}();",
                        dataTableProcessor.GetName(i), dataTableProcessor.GetType(i).Name).AppendLine();
                else
                    stringBuilder.AppendFormat("                    {0} = binaryReader.Read{1}();",
                        dataTableProcessor.GetName(i), dataTableProcessor.GetType(i).Name).AppendLine();
            }

            stringBuilder
                .AppendLine("                }")
                .AppendLine("            }")
                .AppendLine()
                .AppendLine("            GeneratePropertyArray();")
                .AppendLine("            return true;")
                .Append("        }");

            return stringBuilder.ToString();
        }

        private static string GenerateDataTablePropertyArray(DataTableProcessor dataTableProcessor)
        {
            var propertyCollections = new List<PropertyCollection>();
            for (var i = 0; i < dataTableProcessor.RawColumnCount; i++)
            {
                if (dataTableProcessor.IsCommentColumn(i))
                    // 注释列
                    continue;

                if (dataTableProcessor.IsIdColumn(i))
                    // 编号列
                    continue;

                var name = dataTableProcessor.GetName(i);
                if (!EndWithNumberRegex.IsMatch(name)) continue;

                var propertyCollectionName = EndWithNumberRegex.Replace(name, string.Empty);
                var id = int.Parse(EndWithNumberRegex.Match(name).Value);

                PropertyCollection propertyCollection = null;
                foreach (var pc in propertyCollections)
                    if (pc.Name == propertyCollectionName)
                    {
                        propertyCollection = pc;
                        break;
                    }

                if (propertyCollection == null)
                {
                    propertyCollection =
                        new PropertyCollection(propertyCollectionName, dataTableProcessor.GetLanguageKeyword(i));
                    propertyCollections.Add(propertyCollection);
                }

                propertyCollection.AddItem(id, name);
            }

            var stringBuilder = new StringBuilder();
            var firstProperty = true;
            foreach (var propertyCollection in propertyCollections)
            {
                if (firstProperty)
                    firstProperty = false;
                else
                    stringBuilder.AppendLine().AppendLine();

                stringBuilder
                    .AppendFormat("        private KeyValuePair<int, {1}>[] m_{0} = null;", propertyCollection.Name,
                        propertyCollection.LanguageKeyword).AppendLine()
                    .AppendLine()
                    .AppendFormat("        public int {0}Count", propertyCollection.Name).AppendLine()
                    .AppendLine("        {")
                    .AppendLine("            get")
                    .AppendLine("            {")
                    .AppendFormat("                return m_{0}.Length;", propertyCollection.Name).AppendLine()
                    .AppendLine("            }")
                    .AppendLine("        }")
                    .AppendLine()
                    .AppendFormat("        public {1} Get{0}(int id)", propertyCollection.Name,
                        propertyCollection.LanguageKeyword).AppendLine()
                    .AppendLine("        {")
                    .AppendFormat("            foreach (KeyValuePair<int, {1}> i in m_{0})", propertyCollection.Name,
                        propertyCollection.LanguageKeyword).AppendLine()
                    .AppendLine("            {")
                    .AppendLine("                if (i.Key == id)")
                    .AppendLine("                {")
                    .AppendLine("                    return i.Value;")
                    .AppendLine("                }")
                    .AppendLine("            }")
                    .AppendLine()
                    .AppendFormat(
                        "            throw new GameFrameworkException(Utility.Text.Format(\"Get{0} with invalid id '{{0}}'.\", id.ToString()));",
                        propertyCollection.Name).AppendLine()
                    .AppendLine("        }")
                    .AppendLine()
                    .AppendFormat("        public {1} Get{0}At(int index)", propertyCollection.Name,
                        propertyCollection.LanguageKeyword).AppendLine()
                    .AppendLine("        {")
                    .AppendFormat("            if (index < 0 || index >= m_{0}.Length)", propertyCollection.Name)
                    .AppendLine()
                    .AppendLine("            {")
                    .AppendFormat(
                        "                throw new GameFrameworkException(Utility.Text.Format(\"Get{0}At with invalid index '{{0}}'.\", index.ToString()));",
                        propertyCollection.Name).AppendLine()
                    .AppendLine("            }")
                    .AppendLine()
                    .AppendFormat("            return m_{0}[index].Value;", propertyCollection.Name).AppendLine()
                    .Append("        }");
            }

            if (propertyCollections.Count > 0) stringBuilder.AppendLine().AppendLine();

            stringBuilder
                .AppendLine("        private void GeneratePropertyArray()")
                .AppendLine("        {");

            firstProperty = true;
            foreach (var propertyCollection in propertyCollections)
            {
                if (firstProperty)
                    firstProperty = false;
                else
                    stringBuilder.AppendLine().AppendLine();

                stringBuilder
                    .AppendFormat("            m_{0} = new KeyValuePair<int, {1}>[]", propertyCollection.Name,
                        propertyCollection.LanguageKeyword).AppendLine()
                    .AppendLine("            {");

                var itemCount = propertyCollection.ItemCount;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = propertyCollection.GetItem(i);
                    stringBuilder.AppendFormat("                new KeyValuePair<int, {0}>({1}, {2}),",
                        propertyCollection.LanguageKeyword, item.Key.ToString(), item.Value).AppendLine();
                }

                stringBuilder.Append("            };");
            }

            stringBuilder
                .AppendLine()
                .Append("        }");

            return stringBuilder.ToString();
        }


        private sealed class PropertyCollection
        {
            private readonly List<KeyValuePair<int, string>> m_Items;

            public PropertyCollection(string name, string languageKeyword)
            {
                Name = name;
                LanguageKeyword = languageKeyword;
                m_Items = new List<KeyValuePair<int, string>>();
            }

            public string Name { get; }

            public string LanguageKeyword { get; }

            public int ItemCount => m_Items.Count;

            public KeyValuePair<int, string> GetItem(int index)
            {
                if (index < 0 || index >= m_Items.Count)
                    throw new GameFrameworkException(Utility.Text.Format("GetItem with invalid index '{0}'.",
                        index.ToString()));

                return m_Items[index];
            }

            public void AddItem(int id, string propertyName)
            {
                m_Items.Add(new KeyValuePair<int, string>(id, propertyName));
            }
        }
    }
}