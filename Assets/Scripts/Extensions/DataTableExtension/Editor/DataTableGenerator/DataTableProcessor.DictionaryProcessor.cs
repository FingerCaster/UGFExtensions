using System;
using System.Collections.Generic;
using System.IO;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class DictionaryProcessor<T1, T2, T3, T4> : DataProcessor, IDictionaryProcessor
            where T1 : GenericDataProcessor<T3>, new()
            where T2 : GenericDataProcessor<T4>, new()
        {
            public override Type Type
            {
                get
                {
                    var t1 = new T1();
                    var t2 = new T2();
                    var type = typeof(Dictionary<,>);
                    type = type.MakeGenericType(t1.Type, t2.Type);
                    return type;
                }
            }
            public override bool IsEnum => false;

            public override bool IsId => false;

            public override bool IsComment => false;

            public override bool IsSystem => false;

            public override string LanguageKeyword
            {
                get
                {
                    var t1 = new T1();
                    var t2 = new T2();
                    return $"Dictionary<{t1.LanguageKeyword},{t2.LanguageKeyword}>";
                }
            }

            public Type KeyType
            {
                get
                {
                    var t1 = new T1();
                    return t1.Type;
                }
            }

            public Type ValueType
            {
                get
                {
                    var t2 = new T2();
                    return t2.Type;
                }
            }

            public string KeyLanguageKeyword
            {
                get
                {
                    var t2 = new T2();
                    return t2.LanguageKeyword;
                }
            }

            public string ValueLanguageKeyword
            {
                get
                {
                    var t2 = new T2();
                    return t2.LanguageKeyword;
                }
            }

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "Dictionary<{0},{1}>",
                    "System.Collections.Generic.Dictionary<{0},{1}>"
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

                DataProcessor dataProcessor1 = new T1();
                DataProcessor dataProcessor2 = new T2();
                var splitValues = value.Split('|');
                binaryWriter.Write7BitEncodedInt32(splitValues.Length);
                foreach (var itemValue in splitValues)
                {
                    var keyValue = itemValue.Split('#');
                    dataProcessor1.WriteToStream(dataTableProcessor, binaryWriter, keyValue[0].Substring(1));
                    dataProcessor2.WriteToStream(dataTableProcessor, binaryWriter,
                        keyValue[1].Substring(0, keyValue[1].Length - 1));
                }
            }
        }
    }
}