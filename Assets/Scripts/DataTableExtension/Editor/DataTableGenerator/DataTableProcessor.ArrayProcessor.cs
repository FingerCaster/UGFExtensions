using System;
using System.IO;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class ArrayProcessor<T, K> : DataProcessor, ICollectionProcessor
            where T : GenericDataProcessor<K>, new()
        {
            public override bool IsComment => false;

            public override bool IsSystem => false;
            public override bool IsEnum => false;

            public override Type Type
            {
                get
                {
                    if (!(Activator.CreateInstance(typeof(T)) is T t))
                        return typeof(T[]);
                    var type = typeof(T[]);
                    return type;
                }
            }

            public override bool IsId => false;

            public override string LanguageKeyword
            {
                get
                {
                    if (Activator.CreateInstance(typeof(T)) is T t) return $"{t.LanguageKeyword}[]";

                    return $"{typeof(T)}[]";
                }
            }

            public Type ItemType
            {
                get
                {
                    DataProcessor dataProcessor = Activator.CreateInstance(typeof(T)) as T;
                    return dataProcessor.Type;
                }
            }

            public string ItemLanguageKeyword
            {
                get
                {
                    DataProcessor dataProcessor = Activator.CreateInstance(typeof(T)) as T;
                    return dataProcessor.LanguageKeyword;
                }
            }

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "{0}[]"
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