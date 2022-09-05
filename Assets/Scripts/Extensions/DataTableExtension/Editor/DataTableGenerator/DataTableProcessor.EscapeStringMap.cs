using System.Collections.Generic;

namespace DE.Editor.DataTableTools
{
    
    public partial class DataTableProcessor
    {
        /// <summary>
        /// 常用转义字符串转换表
        /// </summary>
        private static readonly Dictionary<string, string> m_EscapeStrings = new Dictionary<string, string>()
        {
            {"\\a","\a"},
            {"\\b","\b"},
            {"\\f","\f"},
            {"\\n","\n"},
            {"\\r","\r"},
            {"\\t","\t"},
            {"\\v","\v"},
        };
    }
}