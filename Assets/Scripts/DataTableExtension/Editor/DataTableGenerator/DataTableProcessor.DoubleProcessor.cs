//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class DoubleProcessor : GenericDataProcessor<double>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "double";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "double",
                    "system.double"
                };
            }

            public override double Parse(string value)
            {
                return double.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}