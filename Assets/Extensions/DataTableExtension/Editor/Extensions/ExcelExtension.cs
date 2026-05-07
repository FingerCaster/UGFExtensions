//------------------------------------------------------------
// ExcelToTxt
// Copyright Xu wei
//------------------------------------------------------------

using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DE.Editor
{
    public static class ExcelExtension
    {
        private static readonly Regex NameRegex = new Regex(@"^[A-Z][A-Za-z0-9_]*$");

        public static void ExcelToTxt(string excelFolder, string txtFolder)
        {
            string[] excelFiles = Directory.GetFiles(excelFolder);

            foreach (var excelFile in excelFiles)
            {
                if (!excelFile.EndsWith(".xlsx") || excelFile.Contains("~$"))
                    continue;
                using (FileStream fileStream =
                       new FileStream(excelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (OpenXmlWorkbook workbook = OpenXmlWorkbook.Open(fileStream))
                    {
                        for (int s = 0; s < workbook.Worksheets.Count; s++)
                        {
                            var sheet = workbook.Worksheets[s];
                            if (sheet.RowCount < 1)
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
                            if (sheet.RowCount < 3)
                            {
                                Debug.LogErrorFormat("{0} has wrong row num!", fileFullPath);
                                continue;
                            }

                            int columnCount = sheet.ColumnCount;
                            for (int i = 1; i <= sheet.RowCount; i++)
                            {
                                if (i > DataTableConfig.GetDataTableConfig().ContentStartRow)
                                {
                                    if (sheet.GetCellValue(i, DataTableConfig.GetDataTableConfig().IdColumn + 1) == null)
                                    {
                                        continue;
                                    }
                                }

                                sb.Clear();
                                for (int j = 1; j <= columnCount; j++)
                                {
                                    string value = sheet.GetCellValue(i, j);
                                    if (value == null)
                                    {
                                        sb.Append("");
                                    }
                                    else
                                    {
                                        sb.Append(value);
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
