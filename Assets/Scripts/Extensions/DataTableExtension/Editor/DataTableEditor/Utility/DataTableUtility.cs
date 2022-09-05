using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;

namespace DataTableEditor
{
    public partial class Utility
    {
        public class DataTableUtility
        {
            public static Vector2 GetMiddlePosition(float windowWidth, float windowHeight)
            {
                return new Vector2(Screen.currentResolution.width / 2 - windowWidth / 2,
                    Screen.currentResolution.height / 2 - windowHeight / 2);
            }

            public static Vector2 GetMiddlePosition(Vector2 windowSize)
            {
                return new Vector2(Screen.currentResolution.width / 2 - windowSize.x / 2,
                    Screen.currentResolution.height / 2 - windowSize.y / 2);
            }

            //模板
            public static List<DataTableRowData> DataTableTemplate = new List<DataTableRowData>
        {
            new DataTableRowData()
            {
                Data = new List<string>()
                {
                    "#", "配置", "", ""
                }
            },
            new DataTableRowData()
            {
                Data = new List<string>()
                {
                    "#", "ID", "", ""
                }
            },
            new DataTableRowData()
            {
                Data = new List<string>()
                {
                    "#", "int", "", ""
                }
            },
            new DataTableRowData()
            {
                Data = new List<string>()
                {
                    "", "0", "", ""
                }
            },
        };

            /// <summary>
            /// 新建表格
            /// </summary>
            /// <returns>新建表格文件路径</returns>
            public static string NewDataTableFile(Encoding encoding)
            {
                string path = EditorUtility.SaveFilePanel("保存文件", "", "template.txt", "txt");

                if (string.IsNullOrEmpty(path) == false)
                    SaveDataTableFile(path, DataTableTemplate, encoding);

                return path;
            }

            /// <summary>
            /// 保存表格文件
            /// </summary>
            /// <param name="path">保存文件路径</param>
            /// <param name="data">数据信息</param>
            /// <returns>保存是否成功</returns>
            public static bool SaveDataTableFile(string path, List<DataTableRowData> data, Encoding encoding)
            {
                using (StreamWriter sw = new StreamWriter(path, false, encoding))
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        for (int j = 0; j < data[i].Data.Count; j++)
                        {
                            sw.Write(data[i].Data[j]);

                            if (j != data[i].Data.Count - 1)
                            {
                                sw.Write("\t");
                            }
                        }

                        if (i != data.Count - 1)
                        {
                            sw.WriteLine();
                        }
                    }
                }

                //EditorUtility.DisplayDialog("提示", "保存成功!", "ojbk");

                AssetDatabase.Refresh();

                return true;
            }

            /// <summary>
            /// 加载数据表文件
            /// </summary>
            /// <param name="path">表格文件路径</param>
            /// <returns>保存的信息数据</returns>
            public static List<DataTableRowData> LoadDataTableFile(string path, Encoding encoding)
            {
                if (File.Exists(path) == false)
                {
                    EditorUtility.DisplayDialog("提示", "文件路径不存在", "确定");
                    return null;
                }

                List<DataTableRowData> data = new List<DataTableRowData>();

                using (StreamReader sr = new StreamReader(path, encoding))
                {
                    while (sr.EndOfStream == false)
                    {
                        // UTF8Encoding utf8 = new UTF8Encoding();
                        string line = sr.ReadLine();
                        string[] splited = line.Split('\t');
                        DataTableRowData row = new DataTableRowData();

                        for (int i = 0; i < splited.Length; i++)
                        {
                            row.Data.Add(splited[i]);
                        }

                        data.Add(row);
                    }
                }

                return data;
            }
        }
    }
}
