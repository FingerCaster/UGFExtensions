//------------------------------------------------------------
// ExcelToTxt
// Copyright Xu wei
//------------------------------------------------------------

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DE.Editor.DataTableTools;
using UnityEngine;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace DE.Editor
{
    public static class ExcelExtension
    {
        private static readonly Regex NameRegex = new Regex(@"^[A-Z][A-Za-z0-9_]*$");

        [MenuItem("DataTable/Excel To Txt", priority = 13)]
        public static void ExcelToTxt()
        {
            if (!Directory.Exists(DataTableConfig.ExcelsFolder))
            {
                Debug.LogError($"{DataTableConfig.ExcelsFolder} is not exist!");
                return;
            }

            string[] excelFiles = Directory.GetFiles(DataTableConfig.ExcelsFolder);
            foreach (var excelFile in excelFiles)
            {
                if (!excelFile.EndsWith(".xlsx") || excelFile.Contains("~$"))
                    continue;
                using (FileStream fileStream = new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    IWorkbook workbook = new XSSFWorkbook(fileStream);
                    for (int s = 0; s < workbook.NumberOfSheets; s++)
                    {
                        ISheet sheet = workbook.GetSheetAt(s);
                        if (sheet.LastRowNum < 1)
                            continue;
                        string fileName = sheet.SheetName;
                        if (string.IsNullOrWhiteSpace(fileName))
                        {
                            Debug.LogErrorFormat("{0} has not datable name!", fileName);
                            continue;
                        }

                        if (!NameRegex.IsMatch(fileName))
                        {
                            Debug.LogErrorFormat("{0} has wrong datable name!", fileName);
                            continue;
                        }

                        string fileFullPath = $"{DataTableConfig.DataTableFolderPath}/{fileName}.txt";
                        if (File.Exists(fileFullPath))
                        {
                            File.Delete(fileFullPath);
                        }

                        List<string> sContents = new List<string>();
                        StringBuilder sb = new StringBuilder();
                        if (sheet.LastRowNum < 3)
                        {
                            Debug.LogErrorFormat("{0} has wrong row num!", fileFullPath);
                            continue;
                        }
                        
                        int columnCount = sheet.GetRow(sheet.LastRowNum).LastCellNum;
                        for (int i = 0; i <= sheet.LastRowNum; i++)
                        {
                            sb.Clear();
                            IRow row = sheet.GetRow(i);
                            for (int j = 0; j < columnCount; j++)
                            {
                                if (row.GetCell(j) == null)
                                {
                                    sb.Append("");
                                }
                                else
                                {
                                    ICell cell = row.GetCell(j);
                                    sb.Append(cell);
                                }
                                if (j != columnCount - 1)
                                {
                                    sb.Append('\t');
                                }
                            }
                            sContents.Add(sb.ToString());
                        }

                        File.WriteAllLines(fileFullPath, sContents, Encoding.UTF8);
                        Debug.LogFormat("更新Excel表格：{0}", fileFullPath);
                    }
                }
            }
            AssetDatabase.Refresh();
        }
    }
}