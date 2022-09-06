using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions.DataTableExtension.Editor;
using GameFramework;
using OfficeOpenXml;
using UnityEngine;

namespace DE.Editor.DataTableTools
{
    public partial class DataTableProcessor
    {
        public DataTableProcessor(ExcelWorksheet sheet, int nameRow, int typeRow,
            int? defaultValueRow, int? commentRow, int contentStartRow, int idColumn)
        {
            // if (string.IsNullOrEmpty(sheet))
            //     throw new GameFrameworkException("Data table file name is invalid.");
            //
            // if (!dataTableFileName.EndsWith(".xlsx", StringComparison.Ordinal))
            //     throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not a excel.",
            //         dataTableFileName));
            //
            // if (!File.Exists(dataTableFileName))
            //     throw new GameFrameworkException(Utility.Text.Format("Data table file '{0}' is not exist.",
            //         dataTableFileName));
            var rawRowCount = 0;
            var rawColumnCount = 0;
            var rawValues = new List<string[]>();
            rawColumnCount = sheet.Dimension.End.Column;
            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
            {
                if (i>DataTableConfig.GetDataTableConfig().ContentStartRow)
                {
                    //跳过没有id的空行
                    if (sheet.Cells[i, DataTableConfig.GetDataTableConfig().IdColumn+1].Value == null)
                    {
                        continue;
                    }
                }
                var rawValue = new string[rawColumnCount];
                for (int j = 1; j <= rawColumnCount; j++)
                {
                    if (sheet.Cells[i,j].Value == null)
                    {
                        rawValue[j-1] = string.Empty;
                    }
                    else
                    {
                        rawValue[j-1] = sheet.Cells[i,j].Value.ToString();
                    }
                }

                rawRowCount++;
               
                rawValues.Add(rawValue);
            }

            m_RawValues = rawValues.ToArray();


            if (nameRow < 0)
                throw new GameFrameworkException(Utility.Text.Format("Name row '{0}' is invalid.", nameRow.ToString()));

            if (typeRow < 0)
                throw new GameFrameworkException(Utility.Text.Format("Type row '{0}' is invalid.", typeRow.ToString()));

            if (contentStartRow < 0)
                throw new GameFrameworkException(Utility.Text.Format("Content start row '{0}' is invalid.",
                    contentStartRow.ToString()));

            if (idColumn < 0)
                throw new GameFrameworkException(
                    Utility.Text.Format("Id column '{0}' is invalid.", idColumn.ToString()));

            if (nameRow >= rawRowCount)
                throw new GameFrameworkException(Utility.Text.Format(
                    "Name row '{0}' >= raw row count '{1}' is not allow.", nameRow.ToString(), rawRowCount.ToString()));

            if (typeRow >= rawRowCount)
                throw new GameFrameworkException(Utility.Text.Format(
                    "Type row '{0}' >= raw row count '{1}' is not allow.", typeRow.ToString(), rawRowCount.ToString()));

            if (defaultValueRow.HasValue && defaultValueRow.Value >= rawRowCount)
                throw new GameFrameworkException(Utility.Text.Format(
                    "Default value row '{0}' >= raw row count '{1}' is not allow.", defaultValueRow.Value.ToString(),
                    rawRowCount.ToString()));

            if (commentRow.HasValue && commentRow.Value >= rawRowCount)
                throw new GameFrameworkException(Utility.Text.Format(
                    "Comment row '{0}' >= raw row count '{1}' is not allow.", commentRow.Value.ToString(),
                    rawRowCount.ToString()));

            if (contentStartRow > rawRowCount)
                throw new GameFrameworkException(Utility.Text.Format(
                    "Content start row '{0}' > raw row count '{1}' is not allow.", contentStartRow.ToString(),
                    rawRowCount.ToString()));

            if (idColumn >= rawColumnCount)
                throw new GameFrameworkException(Utility.Text.Format(
                    "Id column '{0}' >= raw column count '{1}' is not allow.", idColumn.ToString(),
                    rawColumnCount.ToString()));

            m_NameRow = m_RawValues[nameRow];
            m_TypeRow = m_RawValues[typeRow];
            m_DefaultValueRow = defaultValueRow.HasValue ? m_RawValues[defaultValueRow.Value] : null;
            m_CommentRow = commentRow.HasValue ? m_RawValues[commentRow.Value] : null;
            ContentStartRow = contentStartRow;
            IdColumn = idColumn;

            m_DataProcessor = new DataProcessor[rawColumnCount];
            for (var i = 0; i < rawColumnCount; i++)
                if (i == IdColumn)
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor("id");
                else
                    m_DataProcessor[i] = DataProcessorUtility.GetDataProcessor(m_TypeRow[i]);

            var strings = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = contentStartRow; i < rawRowCount; i++)
            {
                if (IsCommentRow(i)) continue;

                for (var j = 0; j < rawColumnCount; j++)
                {
                    if (m_DataProcessor[j] is ICollectionProcessor collectionProcessor)
                    {
                        if (collectionProcessor.ItemLanguageKeyword != "string") continue;
                    }
                    else
                    {
                        if (m_DataProcessor[j].LanguageKeyword != "string") continue;
                    }

                    var str = m_RawValues[i][j];
                    var values = str.Split(',');
                    foreach (var value in values)
                        if (strings.ContainsKey(value))
                            strings[value]++;
                        else
                            strings[value] = 1;
                }
            }

            m_Strings = strings.OrderBy(value => value.Key).ThenByDescending(value => value.Value)
                .Select(value => value.Key).ToArray();

            m_CodeTemplate = null;
            m_CodeGenerator = null;
        }
    }
}