using System;
using System.IO;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class EnumProcessor<T> : GenericDataProcessor<int>
            where T : struct, IConvertible
        {
            static EnumProcessor()
            {
                if (!typeof(T).IsEnum)
                {
                    throw new ArgumentException("T must be an enumerated type");
                }
            }

            public Type EnumType => typeof(T);
            public override bool IsSystem => false;
            public override bool IsEnum => true;
            public string NameSpace => EnumType.Namespace;
            public override string LanguageKeyword => EnumType.FullName;

            public override string[] GetTypeStrings()
            {
                if (string.IsNullOrEmpty(EnumType.FullName) || EnumType.FullName == EnumType.Name)
                {
                    return new[] {EnumType.Name.ToLower()};
                }
                else
                {
                    return new[]
                    {
                        //EnumType.Name.ToLower(),
                        EnumType.FullName.ToLower()
                    };
                }
            }

            public override int Parse(string value)
            {
                bool isInt = int.TryParse(value, out int v);
                if (isInt)
                {
                    return int.Parse(value);
                }

                bool isString = EnumParse(value, out T v1);
                if (isString)
                {
                    return v1.ToInt32(null);
                }

                throw new Exception($"Value:{value} is not {typeof(T)}!");
            }
            

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write7BitEncodedInt32(Parse(value));
            }
            
            public static bool EnumParse<TE>(string value,out TE defaultValue) where TE : struct, IConvertible 
            {
                if (!typeof(TE).IsEnum) throw new ArgumentException("T must be an enumerated type");
                if (string.IsNullOrEmpty(value))
                {
                    defaultValue = default;
                    return false;
                }
                foreach (TE item in Enum.GetValues(typeof(TE)))
                {
                    if (!item.ToString().ToLowerInvariant().Equals(value.Trim().ToLowerInvariant())) continue;
                    defaultValue = item;
                    return true;
                }

                defaultValue = default;
                return false;
            }
        }
    }
}