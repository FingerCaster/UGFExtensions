using System;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        public interface ICollectionProcessor
        {
            Type ItemType { get; }

            string ItemLanguageKeyword { get; }
        }
    }
}