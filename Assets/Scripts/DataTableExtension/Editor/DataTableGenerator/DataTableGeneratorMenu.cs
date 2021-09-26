using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GameFramework;
using OfficeOpenXml;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace DE.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("DataTable/Generate DataTables/From Txt", false, 1)]
        public static void GenerateDataTablesFromTxtNotFileSystem()
        {
            DataTableConfig.RefreshDataTables();
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Txt, 2);
            foreach (var dataTableName in DataTableConfig.DataTableNames)
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

        [MenuItem("DataTable/Generate DataTables/From Excel", false, 1)]
        public static void GenerateDataTablesFormExcelNotFileSystem()
        {
            DataTableConfig.RefreshDataTables();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Excel, 2);
            foreach (var excelFile in DataTableConfig.ExcelFilePaths)
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

        [MenuItem("DataTable/Generate DataTables/From Txt Use FileSystem", false, 20)]
        public static void GenerateDataTablesFromTxtFileSystem()
        {
            DataTableConfig.RefreshDataTables();
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Txt, 2);
            foreach (var dataTableName in DataTableConfig.DataTableNames)
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

        [MenuItem("DataTable/Generate DataTables/From Excel Use FileSystem", false, 20)]
        public static void GenerateDataTablesFormExcelFileSystem()
        {
            DataTableConfig.RefreshDataTables();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Excel, 2);

            foreach (var excelFile in DataTableConfig.ExcelFilePaths)
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
        
        [MenuItem("DataTable/Test Open Excel Time EPPlus", false, 20)]
        public static void TestLoadExcelEPPlus()
        {
            string path = @"G:\Github\UGFExtensions\Excels\TestEnum.xlsx";
            // System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Stopwatch stopwatch = Stopwatch.StartNew();
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                {
                    Debug.Log($"OpenExcel time :{stopwatch.ElapsedMilliseconds}");
                }
            }

            stopwatch.Stop();
        }
    }
}