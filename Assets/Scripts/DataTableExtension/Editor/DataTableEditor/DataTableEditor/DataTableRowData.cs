using System;
using System.Collections.Generic;

namespace DataTableEditor
{

    [Serializable]
    public class DataTableRowData
    {
        public List<string> Data { get; set; }

        public DataTableRowData()
        {
            Data = new List<string>();
        }
    }
}