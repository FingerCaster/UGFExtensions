using System.IO;
using System.Linq;
using GameFramework;
using UnityEngine;

namespace DE.Editor
{
    /// <summary>
    /// 数据表配置类
    /// </summary>
    public class DataTableConfig
    {
        /// <summary>
        /// 数据表存放文件夹路径
        /// </summary>
        public const string DataTableFolderPath = "Assets/Res/DataTables";

        public static readonly string ExcelsFolder = $"{Application.dataPath}/../Excels/";

        /// <summary>
        /// 数据表C#实体类生成文件夹路径
        /// </summary>
        public const string CSharpCodePath = "Assets/Scripts/DataTableExtension/Runtime/DataTable";
        /// <summary>
        /// 数据表C#实体类模板存放路径
        /// </summary>
        public const string CSharpCodeTemplateFileName = "Assets/Res/Configs/DataTableCodeTemplate.txt";
        /// <summary>
        /// 数据表扩展类文件夹路径
        /// </summary>
        public static readonly string ExtensionDirectoryPath = "Assets/Scripts/DataTableExtension/Runtime/Extensions";
        /// <summary>
        /// 数据表命名空间
        /// </summary>
        public const string NameSpace = "UGFExtensions";
        /// <summary>
        /// 数据表中使用类型 所在的所有程序集
        /// </summary>
        public static readonly string[] AssemblyNames =
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
        public static readonly string[] EditorAssemblyNames =
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
        public static string[] DataTablePaths;
        /// <summary>
        /// 数据表文件名
        /// </summary>
        public static string[] DataTableNames;
        static DataTableConfig()
        {
            RefreshDataTables();
        }

        public static void RefreshDataTables()
        {
            var folder = new DirectoryInfo(DataTableFolderPath);
            DataTablePaths = folder.GetFiles("*.txt").Select(_ => Utility.Path.GetRegularPath(_.FullName))
                .ToArray();
            DataTableNames = folder.GetFiles("*.txt").Select(file => Path.GetFileNameWithoutExtension(file.Name))
                .ToArray();
        }
    }
}