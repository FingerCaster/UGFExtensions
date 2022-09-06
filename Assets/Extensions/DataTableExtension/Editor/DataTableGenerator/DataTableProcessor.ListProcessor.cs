using System;
using System.Collections.Generic;
using System.IO;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ListProcessor<T, K> : DataProcessor, ICollectionProcessor
            where T : GenericDataProcessor<K>, new()
        {
            public override bool IsComment => false;

            public override bool IsSystem => false;
            public override bool IsEnum => false;

            public override Type Type
            {
                get
                {
                    var t = new T();
                    var type = typeof(List<>);
                    type = type.MakeGenericType(t.Type);
                    return type;
                }
            }

            public override bool IsId => false;

            public override string LanguageKeyword
            {
                get
                {
                    var t = new T();
                    return $"List<{t.LanguageKeyword}>";
                }
            }

            public Type ItemType
            {
                get
                {
                    var t = new T();
                    return t.Type;
                }
            }

            public string ItemLanguageKeyword
            {
                get
                {
                    var t = new T();
                    return t.LanguageKeyword;
                }
            }

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "List<{0}>",
                    "System.Collections.Generic.List<{0}>"
                };
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                if (string.IsNullOrEmpty(value) || value.ToLowerInvariant().Equals("null"))
                {
                    binaryWriter.Write7BitEncodedInt32(0);
                    return;
                }

                DataProcessor dataProcessor = new T();
                string[] splitValues;
                splitValues = value.Split(dataProcessor.IsSystem|| dataProcessor.IsEnum ? ',' : '|');
                binaryWriter.Write7BitEncodedInt32(splitValues.Length);
                foreach (var itemValue in splitValues)
                    dataProcessor.WriteToStream(dataTableProcessor, binaryWriter, itemValue);
            }
        }
    }
}