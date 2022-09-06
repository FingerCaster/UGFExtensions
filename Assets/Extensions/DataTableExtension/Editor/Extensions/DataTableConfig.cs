using System;
using System.IO;
using System.Linq;
using GameFramework;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace DE.Editor
{
    /// <summary>
    /// 数据表编辑器配置相关数据
    /// </summary>
    public class DataTableConfig : ScriptableObject
    {
        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        public string DataTableFolderPath;

        /// <summary>
        /// Excel存放的文件夹路径
        /// </summary>
        public string ExcelsFolder;

        /// <summary>
        /// 数据表C#实体类生成文件夹路径
        /// </summary>
        public string CSharpCodePath;

        /// <summary>
        /// 数据表C#实体类模板存放路径
        /// </summary>
        public string CSharpCodeTemplateFileName;

        /// <summary>
        /// 数据表扩展类文件夹路径
        /// </summary>
        public  string ExtensionDirectoryPath;

        /// <summary>
        /// 数据表命名空间
        /// </summary>
        public string NameSpace;

        /// <summary>
        /// 数据表中使用类型 所在的所有程序集
        /// </summary>
        public  string[] AssemblyNames =
        {
#if UNITY_2017_3_OR_NEWER
            //asmdef
            "Test",
#endif
            "Assembly-CSharp"
        };

        /// <summary>
        /// 编辑器中使用到的程序集
        /// </summary>
        public  string[] EditorAssemblyNames =
        {
#if UNITY_2017_3_OR_NEWER
            "UnityGameFramework.Editor",
            "DE.Editor",
#endif
            "Assembly-CSharp-Editor"
        };

        /// <summary>
        /// 数据表文件路径
        /// </summary>
        [NonSerialized]
        public string[] TxtFilePaths;

        /// <summary>
        /// 数据表文件名
        /// </summary>
        [NonSerialized]
        public string[] DataTableNames;

        /// <summary>
        /// Excel表文件路径
        /// </summary>
        [NonSerialized]
        public string[] ExcelFilePaths;
        
        
         //所有行列 是逻辑行列从0 开始 但是eppplus 需要从1开始遍历 使用时需要+1
        /// <summary>
        /// 字段名所在行
        /// </summary>
        public int NameRow;
        /// <summary>
        /// 类型名所在行
        /// </summary>
        public int TypeRow;
        /// <summary>
        /// 注释所在行
        /// </summary>
        public int CommentRow;
        /// <summary>
        /// 内容开始行
        /// </summary>
        public int ContentStartRow;
        /// <summary>
        /// id所在列
        /// </summary>
        public int IdColumn;

        public void RefreshDataTables()
        {
            if (Directory.Exists(DataTableFolderPath))
            {
                var txtFolder = new DirectoryInfo(DataTableFolderPath);
                TxtFilePaths = txtFolder.GetFiles("*.txt", SearchOption.TopDirectoryOnly)
                    .Select(_ => Utility.Path.GetRegularPath(_.FullName))
                    .ToArray();
                DataTableNames = txtFolder.GetFiles("*.txt", SearchOption.TopDirectoryOnly)
                    .Select(file => Path.GetFileNameWithoutExtension(file.Name))
                    .ToArray();
            }

            if (Directory.Exists(ExcelsFolder))
            {
                var excelFolder = new DirectoryInfo(ExcelsFolder);
                ExcelFilePaths = excelFolder.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly)
                    .Where(_ => !_.Name.StartsWith("~$")).Select(_ => Utility.Path.GetRegularPath(_.FullName))
                    .ToArray();
            }
        }

        private static string s_DataTableConfigPath = $"Assets/Extensions/DataTableExtension/Editor/Resource/DataTableConfig.asset";

        private static DataTableConfig s_DataTableConfig;
        
        public static DataTableConfig GetDataTableConfig()
        {
            if (s_DataTableConfig == null)
            {
                DataTableConfig dataTableConfig = AssetDatabase.LoadAssetAtPath<DataTableConfig>(s_DataTableConfigPath);
                if (dataTableConfig == null)
                {
                    throw new Exception($"不存在{nameof(DataTableConfig)} 请调用 DataTable/CreateDataTableConfig 创建配置");
                }

                s_DataTableConfig = dataTableConfig;
            }
            return s_DataTableConfig;
        }

        

        public static void CreateDataTableConfig()
        {
            try
            {
                GetDataTableConfig();
            }
            catch
            {
                // ignored
            }

            if (s_DataTableConfig != null)
            {
                EditorUtility.DisplayDialog("警告", $"已存在{nameof(DataTableConfig)}，路径:{s_DataTableConfig}", "确认");
                return;
            }
            
            DataTableConfig codeGeneratorSettingConfig = CreateInstance<DataTableConfig>();
            codeGeneratorSettingConfig.DataTableFolderPath = "Assets/Res/DataTables";
            codeGeneratorSettingConfig.ExcelsFolder = $"Assets/../Excels/";
            codeGeneratorSettingConfig.CSharpCodePath = "Assets/Extensions/DataTableExtension/Runtime/DataTable";
            codeGeneratorSettingConfig.CSharpCodeTemplateFileName = "Assets/Extensions/DataTableExtension/Editor/Resource/DataTableCodeTemplate.txt";
            codeGeneratorSettingConfig.ExtensionDirectoryPath = "Assets/Extensions/DataTableExtension/Runtime/Extensions";
            codeGeneratorSettingConfig.NameSpace = "UGFExtensions";
            codeGeneratorSettingConfig.AssemblyNames = new []
            {
#if UNITY_2017_3_OR_NEWER
                //asmdef
#endif
                "Assembly-CSharp"
            };
            codeGeneratorSettingConfig.EditorAssemblyNames =new []
            {
#if UNITY_2017_3_OR_NEWER
                "UnityGameFramework.Editor",
                "DE.Editor",
#endif
                "Assembly-CSharp-Editor"
            };
            
            codeGeneratorSettingConfig.NameRow = 1;
            codeGeneratorSettingConfig.TypeRow = 2;
            codeGeneratorSettingConfig.CommentRow = 3;
            codeGeneratorSettingConfig.ContentStartRow = 4;
            codeGeneratorSettingConfig.IdColumn = 1;
            AssetDatabase.CreateAsset(codeGeneratorSettingConfig, s_DataTableConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }
}