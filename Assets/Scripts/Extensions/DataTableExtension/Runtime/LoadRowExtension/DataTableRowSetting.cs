namespace UGFExtensions
{
    /// <summary>
    /// 数据表行设置
    /// </summary>
    public class DataTableRowSetting
    {
        public DataTableRowSetting(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        /// <summary>
        /// 起始位置
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; }
    }
}