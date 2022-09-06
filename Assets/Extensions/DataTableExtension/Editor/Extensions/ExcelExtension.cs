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
using Extensions.DataTableExtension.Editor;
using OfficeOpenXml;
using UnityEngine;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace DE.Editor
{
    public static class ExcelExtension
    {
        private static readonly Regex NameRegex = new Regex(@"^[A-Z][A-Za-z0-9_]*$");

        public static void ExcelToTxt(string excelFolder, string txtFolder)
        {
            string[] excelFiles = Directory.GetFiles(excelFolder);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            foreach (var excelFile in excelFiles)
            {
                if (!excelFile.EndsWith(".xlsx") || excelFile.Contains("~$"))
                    continue;
                using (FileStream fileStream =
                       new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(fileStream))
                    {
                        for (int s = 0; s < excelPackage.Workbook.Worksheets.Count; s++)
                        {
                            var sheet = excelPackage.Workbook.Worksheets[s];
                            if (sheet.Dimension.End.Row < 1)
                                continue;
                            string fileName = sheet.Name;
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

                            string fileFullPath = $"{txtFolder}/{fileName}.txt";
                            if (File.Exists(fileFullPath))
                            {
                                File.Delete(fileFullPath);
                            }

                            List<string> sContents = new List<string>();
                            StringBuilder sb = new StringBuilder();
                            if (sheet.Dimension.End.Row < 3)
                            {
                                Debug.LogErrorFormat("{0} has wrong row num!", fileFullPath);
                                continue;
                            }

                            int columnCount = sheet.Dimension.End.Column;
                            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
                            {
                                if (i > DataTableConfig.GetDataTableConfig().ContentStartRow)
                                {
                                    if (sheet.Cells[i, DataTableConfig.GetDataTableConfig().IdColumn + 1].Value == null)
                                    {
                                        continue;
                                    }
                                }

                                sb.Clear();
                                for (int j = 1; j <= columnCount; j++)
                                {
                                    if (sheet.Cells[i, j] == null)
                                    {
                                        sb.Append("");
                                    }
                                    else
                                    {
                                        sb.Append(sheet.Cells[i, j].Value);
                                    }

                                    if (j != columnCount)
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
            }
        }
    }
}