using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameFramework;
using OfficeOpenXml;
using UnityEditor;

namespace DE.Editor.DataTableTools
{
    public static class ExtensionsGenerate
    {
        public enum DataTableType
        {
            Txt,
            Excel
        }

        public static void GenerateExtensionByAnalysis(DataTableType dataTableType,string[] filePath ,int typeLine)
        {
            List<string> types = new List<string>(32);
            if (dataTableType == DataTableType.Txt)
            {
                foreach (var dataTableFileName in filePath)
                {
                    var lines = File.ReadAllLines(dataTableFileName, Encoding.UTF8);
                    var rawValue = lines[typeLine].Split('\t');
                    types.AddRange(rawValue.Select(_ => _.Trim('\"')));
                    types = types.Distinct().ToList();
                }
            }
            else
            {
                foreach (var excelFile in filePath)
                {
                    using (FileStream fileStream =
                        new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                        {
                            for (int i = 0; i < excelPackage.Workbook.Worksheets.Count; i++)
                            {
                                var sheet = excelPackage.Workbook.Worksheets[i];
                                int typeRow = DataTableConfig.GetDataTableConfig().TypeRow;
                                
                                if (sheet.Dimension.Rows<typeRow)
                                {
                                    throw new Exception("数据表格式不正确。请检查");
                                }
                                for (int j = 1; j <= sheet.Dimension.Columns; j++)
                                {
                                    string rawValue = sheet.Cells[typeRow+1, j].Value?.ToString().Trim('\"');
                                    if (!string.IsNullOrEmpty(rawValue))
                                    {
                                        types.Add(rawValue);
                                    }
                                }
                            }
                        }
                    }
                }
                types = types.Distinct().ToList();
            }


            types.Remove("Id");
            types.Remove("#");
            types.Remove("");
            types.Remove("comment");

            List<DataTableProcessor.DataProcessor> datableDataProcessors =
                types.Select(DataTableProcessor.DataProcessorUtility.GetDataProcessor).ToList();

            NameSpaces.Add("System");
            NameSpaces.Add("System.IO");
            NameSpaces.Add("System.Collections.Generic");
            NameSpaces.Add("UnityEngine");
            NameSpaces = NameSpaces.Distinct().ToList();
            var dataProcessorsArray = datableDataProcessors
                .Where(_ => _.LanguageKeyword.ToLower().EndsWith("[]"))
                .Select(_ =>
                    DataTableProcessor.DataProcessorUtility.GetDataProcessor(_.LanguageKeyword.ToLower()
                        .Replace("[]", "")))
                .ToDictionary(_ => _.LanguageKeyword, _ => _);

            var dataProcessorsList = datableDataProcessors
                .Where(_ => _.LanguageKeyword.ToLower().StartsWith("list"))
                .Select(_ => DataTableProcessor.DataProcessorUtility.GetDataProcessor(_.LanguageKeyword.ToLower()
                    .Replace("list", "").Replace("<", "").Replace(">", "")))
                .ToDictionary(_ => _.LanguageKeyword, _ => _);

            var dataProcessorsDictionary = datableDataProcessors
                .Where(_ => _.LanguageKeyword.ToLower().StartsWith("dictionary"))
                .Select(_ =>
                    {
                        var keyValue = _.LanguageKeyword.ToLower()
                            .Replace("dictionary", "").Replace("<", "").Replace(">", "").Split(',');
                        return new[]
                        {
                            DataTableProcessor.DataProcessorUtility.GetDataProcessor(keyValue[0]),
                            DataTableProcessor.DataProcessorUtility.GetDataProcessor(keyValue[1])
                        };
                    }
                ).ToList();
            if (dataProcessorsArray.Count > 0)
            {
                GenerateDataTableExtensionArray(dataProcessorsArray);
                GenerateBinaryReaderExtensionArray(dataProcessorsArray);
            }

            if (dataProcessorsList.Count > 0)
            {
                GenerateDataTableExtensionList(dataProcessorsList);
                GenerateBinaryReaderExtensionList(dataProcessorsList);
            }

            if (dataProcessorsDictionary.Count > 0)
            {
                GenerateDataTableExtensionDictionary(dataProcessorsDictionary);
                GenerateBinaryReaderExtensionDictionary(dataProcessorsDictionary);
            }

            AssetDatabase.Refresh();
        }

        private static void GenerateDataTableExtensionArray(
            IDictionary<string, DataTableProcessor.DataProcessor> dataProcessors)
        {
            var sb = new StringBuilder();
            AddNameSpaces(sb);
            sb.AppendLine($"namespace {DataTableConfig.GetDataTableConfig().NameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static partial class DataTableExtension");
            sb.AppendLine("\t{");
            foreach (var item in dataProcessors)
            {
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\tpublic static {item.Value.LanguageKeyword}[] Parse{item.Value.LanguageKeyword.Replace(".", "")}Array(string value)");
                }
                else
                {
                    sb.AppendLine($"\t\tpublic static {item.Key}[] Parse{item.Value.Type.Name}Array(string value)");
                }

                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tif (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals(\"null\"))");
                sb.AppendLine("\t\t\t\treturn null;");
                if (item.Value.IsSystem || item.Value.IsEnum)
                    sb.AppendLine("\t\t\tstring[] splitValue = value.Split(',');");
                else
                    sb.AppendLine("\t\t\tstring[] splitValue = value.Split('|');");

                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\t\t{item.Value.LanguageKeyword}[] array = new {item.Value.LanguageKeyword}[splitValue.Length];");
                }
                else
                {
                    sb.AppendLine($"\t\t\t{item.Key}[] array = new {item.Key}[splitValue.Length];");
                }

                sb.AppendLine("\t\t\tfor (int i = 0; i < splitValue.Length; i++)");
                sb.AppendLine("\t\t\t{");
                if (item.Value.IsSystem)
                {
                    if (item.Key == "string")
                        sb.AppendLine("\t\t\t\tarray[i] = splitValue[i];");
                    else
                        sb.AppendLine($"\t\t\t\tarray[i] = {item.Value.Type.Name}.Parse(splitValue[i]);");
                }
                else
                {
                    if (item.Value.IsEnum)
                    {
                        sb.AppendLine($"\t\t\t\tarray[i] = EnumParse<{item.Value.LanguageKeyword}>(splitValue[i]);");
                    }
                    else
                    {
                        sb.AppendLine($"\t\t\t\tarray[i] = Parse{item.Value.Type.Name}(splitValue[i]);");
                    }
                }

                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
                sb.AppendLine("\t\t\treturn array;");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            GenerateCodeFile("DataTableExtension.Array", sb.ToString());
        }

        private static void GenerateDataTableExtensionList(
            IDictionary<string, DataTableProcessor.DataProcessor> dataProcessors)
        {
            var sb = new StringBuilder();
            AddNameSpaces(sb);
            sb.AppendLine($"namespace {DataTableConfig.GetDataTableConfig().NameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static partial class DataTableExtension");
            sb.AppendLine("\t{");
            foreach (var item in dataProcessors)
            {
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\tpublic static List<{item.Value.LanguageKeyword}> Parse{item.Value.LanguageKeyword.Replace(".", "")}List(string value)");
                }
                else
                    sb.AppendLine($"\t\tpublic static List<{item.Key}> Parse{item.Value.Type.Name}List(string value)");

                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tif (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals(\"null\"))");
                sb.AppendLine("\t\t\t\treturn null;");
                if (item.Value.IsSystem || item.Value.IsEnum)
                    sb.AppendLine("\t\t\tstring[] splitValue = value.Split(',');");
                else
                    sb.AppendLine("\t\t\tstring[] splitValue = value.Split('|');");
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\t\tList<{item.Value.LanguageKeyword}> list = new List<{item.Value.LanguageKeyword}>(splitValue.Length);");
                }
                else
                    sb.AppendLine($"\t\t\tList<{item.Key}> list = new List<{item.Key}>(splitValue.Length);");

                sb.AppendLine("\t\t\tfor (int i = 0; i < splitValue.Length; i++)");
                sb.AppendLine("\t\t\t{");
                if (item.Value.IsSystem)
                {
                    if (item.Key == "string")
                        sb.AppendLine("\t\t\t\tlist.Add(splitValue[i]);");
                    else
                        sb.AppendLine($"\t\t\t\tlist.Add({item.Value.Type.Name}.Parse(splitValue[i]));");
                }
                else
                {
                    if (item.Value.IsEnum)
                    {
                        sb.AppendLine($"\t\t\t\tlist.Add(EnumParse<{item.Value.LanguageKeyword}>(splitValue[i]));");
                    }
                    else
                    {
                        sb.AppendLine($"\t\t\t\tlist.Add(Parse{item.Value.Type.Name}(splitValue[i]));");
                    }
                }

                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\treturn list;");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            GenerateCodeFile("DataTableExtension.List", sb.ToString());
        }

        private static void GenerateBinaryReaderExtensionList(
            IDictionary<string, DataTableProcessor.DataProcessor> dataProcessors)
        {
            var sb = new StringBuilder();
            AddNameSpaces(sb);

            sb.AppendLine($"namespace {DataTableConfig.GetDataTableConfig().NameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static partial class BinaryReaderExtension");
            sb.AppendLine("\t{");
            foreach (var item in dataProcessors)
            {
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\tpublic static List<{item.Value.LanguageKeyword}> Read{item.Value.LanguageKeyword.Replace(".", "")}List(this BinaryReader binaryReader)");
                }
                else
                    sb.AppendLine(
                        $"\t\tpublic static List<{item.Key}> Read{item.Value.Type.Name}List(this BinaryReader binaryReader)");

                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tint count = binaryReader.Read7BitEncodedInt32();");
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\t\tList<{item.Value.LanguageKeyword}> list = new List<{item.Value.LanguageKeyword}>(count);");
                }
                else
                    sb.AppendLine($"\t\t\tList<{item.Key}> list = new List<{item.Key}>(count);");

                sb.AppendLine("\t\t\tfor (int i = 0; i < count; i++)");
                sb.AppendLine("\t\t\t{");
                if (IsCustomType(item.Value.Type) || item.Value.Type == typeof(DateTime))
                {
                    sb.AppendLine($"\t\t\t\tlist.Add(Read{item.Key}(binaryReader));");
                }
                else
                {
                    var languageKeyword = item.Key;
                    if (languageKeyword == "int" || languageKeyword == "uint" || languageKeyword == "long" ||
                        languageKeyword == "ulong")
                        sb.AppendLine($"\t\t\t\tlist.Add(binaryReader.Read7BitEncoded{item.Value.Type.Name}());");
                    else if (item.Value.IsEnum)
                    {
                        sb.AppendLine(
                            $"\t\t\t\tlist.Add(({item.Value.LanguageKeyword})binaryReader.Read7BitEncodedInt32());");
                    }
                    else
                        sb.AppendLine($"\t\t\t\tlist.Add(binaryReader.Read{item.Value.Type.Name}());");
                }

                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\treturn list;");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            GenerateCodeFile("BinaryReaderExtension.List", sb.ToString());
        }

        private static void GenerateBinaryReaderExtensionArray(
            IDictionary<string, DataTableProcessor.DataProcessor> dataProcessors)
        {
            var sb = new StringBuilder();
            AddNameSpaces(sb);

            sb.AppendLine($"namespace {DataTableConfig.GetDataTableConfig().NameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static partial class BinaryReaderExtension");
            sb.AppendLine("\t{");
            foreach (var item in dataProcessors)
            {
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\tpublic static {item.Value.LanguageKeyword}[] Read{item.Value.LanguageKeyword.Replace(".", "")}Array(this BinaryReader binaryReader)");
                }
                else
                    sb.AppendLine(
                        $"\t\tpublic static {item.Key}[] Read{item.Value.Type.Name}Array(this BinaryReader binaryReader)");

                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tint count = binaryReader.Read7BitEncodedInt32();");
                if (item.Value.IsEnum)
                {
                    sb.AppendLine(
                        $"\t\t\t{item.Value.LanguageKeyword}[] array = new {item.Value.LanguageKeyword}[count];");
                }
                else
                    sb.AppendLine($"\t\t\t{item.Key}[] array = new {item.Key}[count];");

                sb.AppendLine("\t\t\tfor (int i = 0; i < count; i++)");
                sb.AppendLine("\t\t\t{");
                if (IsCustomType(item.Value.Type) || item.Value.Type == typeof(DateTime))
                {
                    sb.AppendLine($"\t\t\t\tarray[i] = Read{item.Key}(binaryReader);");
                }
                else
                {
                    var languageKeyword = item.Key;
                    if (languageKeyword == "int" || languageKeyword == "uint" || languageKeyword == "long" ||
                        languageKeyword == "ulong")
                        sb.AppendLine($"\t\t\t\tarray[i] = binaryReader.Read7BitEncoded{item.Value.Type.Name}();");
                    else if (item.Value.IsEnum)
                    {
                        sb.AppendLine(
                            $"\t\t\t\tarray[i] = ({item.Value.LanguageKeyword})binaryReader.Read7BitEncodedInt32();");
                    }
                    else
                        sb.AppendLine($"\t\t\t\tarray[i] = binaryReader.Read{item.Value.Type.Name}();");
                }

                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\treturn array;");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            GenerateCodeFile("BinaryReaderExtension.Array", sb.ToString());
        }

        private static void GenerateCodeFile(string fileName, string value)
        {
            var filePath =
                Utility.Path.GetRegularPath(Path.Combine(DataTableConfig.GetDataTableConfig().ExtensionDirectoryPath, fileName + ".cs"));
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                using (var stream = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    stream.Write(value);
                }
            }
        }

        private static void GenerateDataTableExtensionDictionary(List<DataTableProcessor.DataProcessor[]> keyValueList)
        {
            var sb = new StringBuilder();
            AddNameSpaces(sb);

            sb.AppendLine($"namespace {DataTableConfig.GetDataTableConfig().NameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static partial class DataTableExtension");
            sb.AppendLine("\t{");
            foreach (var item in keyValueList)
            {
                var dataProcessorT1 = item[0];
                var dataProcessorT2 = item[1];
                (string, string) names = GetNames(dataProcessorT1, dataProcessorT2);
                sb.AppendLine(
                    $"\t\tpublic static Dictionary<{dataProcessorT1.LanguageKeyword},{dataProcessorT2.LanguageKeyword}> Parse{names.Item1}{names.Item2}Dictionary(string value)");

                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tif (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals(\"null\"))");
                sb.AppendLine("\t\t\t\treturn null;");
                sb.AppendLine("\t\t\tstring[] splitValue = value.Split('|');");
                sb.AppendLine(
                    $"\t\t\tDictionary<{dataProcessorT1.LanguageKeyword},{dataProcessorT2.LanguageKeyword}> dictionary = new Dictionary<{dataProcessorT1.LanguageKeyword},{dataProcessorT2.LanguageKeyword}>(splitValue.Length);");
                sb.AppendLine("\t\t\tfor (int i = 0; i < splitValue.Length; i++)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tstring[] keyValue = splitValue[i].Split('#');");
                if (dataProcessorT1.IsSystem)
                {
                    if (dataProcessorT2.IsSystem)
                    {
                        if (dataProcessorT1.LanguageKeyword == "string" && dataProcessorT2.LanguageKeyword == "string")
                            sb.AppendLine(
                                "\t\t\t\tdictionary.Add(keyValue[0].Substring(1),keyValue[1].Substring(0, keyValue[1].Length - 1));");
                        else if (dataProcessorT1.LanguageKeyword == "string")
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(keyValue[0].Substring(1),{dataProcessorT2.Type.Name}.Parse(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                        else if (dataProcessorT2.LanguageKeyword == "string")
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add({dataProcessorT1.Type.Name}.Parse(keyValue[0].Substring(1)),keyValue[1].Substring(0, keyValue[1].Length - 1));");
                        else
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add({dataProcessorT1.Type.Name}.Parse(keyValue[0].Substring(1)),{dataProcessorT2.Type.Name}.Parse(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                    }
                    else
                    {
                        if (dataProcessorT1.LanguageKeyword == "string")
                            if (dataProcessorT2.IsEnum)
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(keyValue[0].Substring(1),EnumParse<{dataProcessorT2.LanguageKeyword}>(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                            else
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(keyValue[0].Substring(1),Parse{dataProcessorT2.Type.Name}(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                        else if (dataProcessorT2.IsEnum)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add({dataProcessorT1.Type.Name}.Parse(keyValue[0].Substring(1)),EnumParse<{dataProcessorT2.LanguageKeyword}>(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                        }
                        else
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add({dataProcessorT1.Type.Name}.Parse(keyValue[0].Substring(1)),Parse{dataProcessorT2.Type.Name}(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                    }
                }
                else
                {
                    if (dataProcessorT2.IsSystem)
                    {
                        if (dataProcessorT2.LanguageKeyword == "string")
                            if (dataProcessorT1.IsEnum)
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(EnumParse<{dataProcessorT1.LanguageKeyword}>(keyValue[0].Substring(1)),keyValue[1].Substring(0, keyValue[1].Length - 1));");
                            else
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(Parse{dataProcessorT1.Type.Name}(keyValue[0].Substring(1)),keyValue[1].Substring(0, keyValue[1].Length - 1));");
                        else if (dataProcessorT1.IsEnum)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(EnumParse<{dataProcessorT1.LanguageKeyword}>(keyValue[0].Substring(1)),{dataProcessorT2.Type.Name}.Parse(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                        }
                        else
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(Parse{dataProcessorT1.Type.Name}(keyValue[0].Substring(1)),{dataProcessorT2.Type.Name}.Parse(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                    }
                    else
                    {
                        if (dataProcessorT2.IsEnum)
                        {
                            sb.AppendLine(
                                dataProcessorT1.IsEnum
                                    ? $"\t\t\t\tdictionary.Add(EnumParse<{dataProcessorT1.LanguageKeyword}>(keyValue[1].Substring(0, keyValue[1].Length - 1)),EnumParse<{dataProcessorT2.LanguageKeyword}>(keyValue[1].Substring(0, keyValue[1].Length - 1)));"
                                    : $"\t\t\t\tdictionary.Add(Parse{dataProcessorT1.Type.Name}(keyValue[0].Substring(1)),EnumParse<{dataProcessorT2.LanguageKeyword}>(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                        }
                        else if (dataProcessorT1.IsEnum)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(EnumParse<{dataProcessorT2.LanguageKeyword}>(keyValue[1].Substring(0, keyValue[1].Length - 1)),Parse{dataProcessorT2.Type.Name}(keyValue[0].Substring(1)));");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(Parse{dataProcessorT1.Type.Name}(keyValue[0].Substring(1)),Parse{dataProcessorT2.Type.Name}(keyValue[1].Substring(0, keyValue[1].Length - 1)));");
                        }
                    }
                }

                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\treturn dictionary;");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            GenerateCodeFile("DataTableExtension.Dictionary", sb.ToString());
        }

        static (string, string) GetNames(DataTableProcessor.DataProcessor t1, DataTableProcessor.DataProcessor t2)
        {
            string t1Name = t1.IsEnum ? t1.LanguageKeyword.Replace(".", "") : t1.Type.Name;
            string t2Name = t2.IsEnum ? t2.LanguageKeyword.Replace(".", "") : t2.Type.Name;
            return (t1Name, t2Name);
        }

        private static void GenerateBinaryReaderExtensionDictionary(
            List<DataTableProcessor.DataProcessor[]> keyValueList)
        {
            var sb = new StringBuilder();
            AddNameSpaces(sb);

            sb.AppendLine($"namespace {DataTableConfig.GetDataTableConfig().NameSpace}");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static partial class BinaryReaderExtension");
            sb.AppendLine("\t{");


            foreach (var item in keyValueList)
            {
                var dataProcessorT1 = item[0];
                var dataProcessorT2 = item[1];
                (string, string) names = GetNames(dataProcessorT1, dataProcessorT2);
                sb.AppendLine(
                    $"\t\tpublic static Dictionary<{dataProcessorT1.LanguageKeyword},{dataProcessorT2.LanguageKeyword}> Read{names.Item1}{names.Item2}Dictionary(this BinaryReader binaryReader)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t\tint count = binaryReader.Read7BitEncodedInt32();");
                sb.AppendLine(
                    $"\t\t\tDictionary<{dataProcessorT1.LanguageKeyword},{dataProcessorT2.LanguageKeyword}> dictionary = new Dictionary<{dataProcessorT1.LanguageKeyword},{dataProcessorT2.LanguageKeyword}>(count);");
                sb.AppendLine("\t\t\tfor (int i = 0; i < count; i++)");
                sb.AppendLine("\t\t\t{");
                var t1LanguageKeyword = dataProcessorT1.LanguageKeyword;
                var t2LanguageKeyword = dataProcessorT2.LanguageKeyword;

                if (dataProcessorT1.IsSystem && dataProcessorT1.Type != typeof(DateTime))
                {
                    if (dataProcessorT2.IsSystem && dataProcessorT2.Type != typeof(DateTime))
                    {
                        if (t1LanguageKeyword == "int" || t1LanguageKeyword == "uint" || t1LanguageKeyword == "long" ||
                            t1LanguageKeyword == "ulong")
                        {
                            if (t2LanguageKeyword == "int" || t2LanguageKeyword == "uint" ||
                                t2LanguageKeyword == "long" || t2LanguageKeyword == "ulong")
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(binaryReader.Read7BitEncoded{dataProcessorT1.Type.Name}(),binaryReader.Read7BitEncoded{dataProcessorT2.Type.Name}());");
                            else
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(binaryReader.Read7BitEncoded{dataProcessorT1.Type.Name}(),binaryReader.Read{dataProcessorT2.Type.Name}());");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(binaryReader.Read{dataProcessorT1.Type.Name}(),binaryReader.Read{dataProcessorT2.Type.Name}());");
                        }
                    }
                    else
                    {
                        if (t1LanguageKeyword == "int" || t1LanguageKeyword == "uint" || t1LanguageKeyword == "long" ||
                            t1LanguageKeyword == "ulong")
                            if (dataProcessorT2.IsEnum)
                            {
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(binaryReader.Read7BitEncoded{dataProcessorT1.Type.Name}(),({dataProcessorT2.LanguageKeyword}) binaryReader.Read7BitEncodedInt32());");
                            }
                            else
                            {
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(binaryReader.Read7BitEncoded{dataProcessorT1.Type.Name}(), Read{dataProcessorT2.LanguageKeyword}(binaryReader));");
                            }
                        else if (dataProcessorT2.IsEnum)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(binaryReader.Read{dataProcessorT1.Type.Name}(),({dataProcessorT2.LanguageKeyword}) binaryReader.Read7BitEncodedInt32());");
                        }
                        else
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(binaryReader.Read{dataProcessorT1.Type.Name}(),Read{dataProcessorT2.LanguageKeyword}(binaryReader));");
                    }
                }
                else
                {
                    if (dataProcessorT2.IsSystem && dataProcessorT2.Type != typeof(DateTime))
                    {
                        if (t2LanguageKeyword == "int" || t2LanguageKeyword == "uint" ||
                            t2LanguageKeyword == "long" ||
                            t2LanguageKeyword == "ulong")
                            if (dataProcessorT1.IsEnum)
                            {
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(({dataProcessorT1.LanguageKeyword}) binaryReader.Read7BitEncodedInt32(),({dataProcessorT2.LanguageKeyword}) binaryReader.Read7BitEncodedInt32());");
                            }
                            else
                            {
                                sb.AppendLine(
                                    $"\t\t\t\tdictionary.Add(Read{dataProcessorT1.LanguageKeyword}(binaryReader),binaryReader.Read7BitEncoded{dataProcessorT2.Type.Name}());");
                            }

                        else if (dataProcessorT1.IsEnum)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(({dataProcessorT1.LanguageKeyword}) binaryReader.Read7BitEncodedInt32(),binaryReader.Read{dataProcessorT2.Type.Name}());");
                        }
                        else
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(Read{dataProcessorT1.LanguageKeyword}(binaryReader),binaryReader.Read{dataProcessorT2.Type.Name}());");
                    }
                    else
                    {
                        if (dataProcessorT2.IsEnum)
                        {
                            sb.AppendLine(
                                dataProcessorT1.IsEnum
                                    ? $"\t\t\t\tdictionary.Add(({dataProcessorT1.LanguageKeyword}) binaryReader.Read7BitEncodedInt32(),({dataProcessorT2.LanguageKeyword}) binaryReader.Read7BitEncodedInt32());"
                                    : $"\t\t\t\tdictionary.Add(Read{dataProcessorT1.LanguageKeyword}(binaryReader),({dataProcessorT2.LanguageKeyword}) binaryReader.Read7BitEncodedInt32());");
                        }
                        else if (dataProcessorT1.IsEnum)
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(({dataProcessorT1.LanguageKeyword}) binaryReader.Read7BitEncodedInt32(),Read{dataProcessorT2.LanguageKeyword}(binaryReader));");
                        }
                        else
                        {
                            sb.AppendLine(
                                $"\t\t\t\tdictionary.Add(Read{dataProcessorT1.LanguageKeyword}(binaryReader),Read{dataProcessorT2.LanguageKeyword}(binaryReader));");
                        }
                    }
                }

                sb.AppendLine("\t\t\t}");
                sb.AppendLine("\t\t\treturn dictionary;");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");
            GenerateCodeFile("BinaryReaderExtension.Dictionary", sb.ToString());
        }

        private static bool IsCustomType(Type type)
        {
            return type != typeof(object) && Type.GetTypeCode(type) == TypeCode.Object;
        }

        private static List<string> NameSpaces = new List<string>();

        private static void AddNameSpaces(StringBuilder stringBuilder)
        {
            foreach (var nameSpace in NameSpaces)
            {
                stringBuilder.AppendLine($"using {nameSpace};");
            }
        }
    }
}