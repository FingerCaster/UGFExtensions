using System.IO;
using System.Linq;
using GameFramework;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;

namespace DE.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {

        [MenuItem("DataTable/Generate DataTables From Txt")]
        public static void GenerateDataTables()
        {
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Txt,2);
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
        [MenuItem("DataTable/Generate DataTables From Excel")]
        public static void GenerateDataTablesFormExcel()
        {
            ExtensionsGenerate.GenerateExtensionByAnalysis(ExtensionsGenerate.DataTableType.Excel,2);

            foreach (var excelFile in DataTableConfig.ExcelFilePaths)
            {
                using (FileStream fileStream = new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var workbook = new XSSFWorkbook(fileStream);
                    for (int i = 0; i < workbook.NumberOfSheets; i++)
                    {
                        ISheet sheet = workbook.GetSheetAt(i);
                        var dataTableProcessor = DataTableGenerator.CreateExcelDataTableProcessor(sheet);
                        if (!DataTableGenerator.CheckRawData(dataTableProcessor, sheet.SheetName))
                        {
                            Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", sheet.SheetName));
                            break;
                        }

                        DataTableGenerator.GenerateDataFile(dataTableProcessor, sheet.SheetName);
                        DataTableGenerator.GenerateCodeFile(dataTableProcessor, sheet.SheetName);
                    }
                }
            }

            AssetDatabase.Refresh();
        }
    }
}