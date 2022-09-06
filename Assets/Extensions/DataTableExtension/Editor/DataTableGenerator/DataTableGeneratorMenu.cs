using System.IO;
using GameFramework;
using OfficeOpenXml;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace DE.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("DataTable/Generate DataTables/From Txt", priority = 2)]
        public static void GenerateDataTablesFromTxtNotFileSystem()
        {
            DataTableConfig.GetDataTableConfig().RefreshDataTables();

            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Txt,
                DataTableConfig.GetDataTableConfig().TxtFilePaths, 2);
            foreach (var dataTableName in DataTableConfig.GetDataTableConfig().DataTableNames)
            {
                var dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Generate DataTables/From Excel", priority = 2)]
        public static void GenerateDataTablesFormExcelNotFileSystem()
        {
            DataTableConfig.GetDataTableConfig().RefreshDataTables();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Excel,
                DataTableConfig.GetDataTableConfig().ExcelFilePaths, 2);
            foreach (var excelFile in DataTableConfig.GetDataTableConfig().ExcelFilePaths)
            {
                using (FileStream fileStream =
                       new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                    {
                        for (int i = 0; i < excelPackage.Workbook.Worksheets.Count; i++)
                        {
                            ExcelWorksheet sheet = excelPackage.Workbook.Worksheets[i];
                            var dataTableProcessor = DataTableGenerator.CreateExcelDataTableProcessor(sheet);
                            if (!DataTableGenerator.CheckRawData(dataTableProcessor, sheet.Name))
                            {
                                Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'",
                                    sheet.Name));
                                break;
                            }

                            DataTableGenerator.GenerateDataFile(dataTableProcessor, sheet.Name);
                            DataTableGenerator.GenerateCodeFile(dataTableProcessor, sheet.Name);
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Generate DataTables/From Txt Use FileSystem", priority = 20)]
        public static void GenerateDataTablesFromTxtFileSystem()
        {
            DataTableConfig.GetDataTableConfig().RefreshDataTables();

            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Txt,
                DataTableConfig.GetDataTableConfig().TxtFilePaths, 2);
            foreach (var dataTableName in DataTableConfig.GetDataTableConfig().DataTableNames)
            {
                var dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }

                DataTableGenerator.GenerateFileSystemFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Generate DataTables/From Excel Use FileSystem", priority = 20)]
        public static void GenerateDataTablesFormExcelFileSystem()
        {
            DataTableConfig.GetDataTableConfig().RefreshDataTables();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Excel,
                DataTableConfig.GetDataTableConfig().ExcelFilePaths, 2);
            foreach (var excelFile in DataTableConfig.GetDataTableConfig().ExcelFilePaths)
            {
                using (FileStream fileStream =
                       new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                    {
                        for (int i = 0; i < excelPackage.Workbook.Worksheets.Count; i++)
                        {
                            ExcelWorksheet sheet = excelPackage.Workbook.Worksheets[i];
                            var dataTableProcessor = DataTableGenerator.CreateExcelDataTableProcessor(sheet);
                            if (!DataTableGenerator.CheckRawData(dataTableProcessor, sheet.Name))
                            {
                                Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'",
                                    sheet.Name));
                                break;
                            }

                            DataTableGenerator.GenerateFileSystemFile(dataTableProcessor, sheet.Name);
                            DataTableGenerator.GenerateCodeFile(dataTableProcessor, sheet.Name);
                        }
                    }
                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/Generate DataTables/Excel To Txt", priority = 32)]
        public static void ExcelToTxt()
        {
            DataTableConfig.GetDataTableConfig().RefreshDataTables();
            if (!Directory.Exists(DataTableConfig.GetDataTableConfig().ExcelsFolder))
            {
                Debug.LogError($"{DataTableConfig.GetDataTableConfig().ExcelsFolder} is not exist!");
                return;
            }

            ExcelExtension.ExcelToTxt(DataTableConfig.GetDataTableConfig().ExcelsFolder,
                DataTableConfig.GetDataTableConfig().DataTableFolderPath);
            AssetDatabase.Refresh();
        }

        [MenuItem("DataTable/CreateDataTableConfig", priority = 3)]
        public static void CreateDataTableConfig()
        {
            DataTableConfig.CreateDataTableConfig();
        }
    }
}