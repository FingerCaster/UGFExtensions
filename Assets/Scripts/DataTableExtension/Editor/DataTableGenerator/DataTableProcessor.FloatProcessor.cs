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
        private sealed class FloatProcessor : GenericDataProcessor<float>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "float";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "float",
                    "single",
                    "system.single"
                };
            }

            public override float Parse(string value)
            {
                return float.Parse(value);
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}