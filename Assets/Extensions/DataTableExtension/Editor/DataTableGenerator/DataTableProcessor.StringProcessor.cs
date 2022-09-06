//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;
using System.Linq;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class StringProcessor : GenericDataProcessor<string>
        {
            public override bool IsSystem => true;

            public override string LanguageKeyword => "string";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "string",
                    "system.string"
                };
            }

            public override string Parse(string value)
            {
                return m_EscapeStrings.Aggregate(value, (current, escapeString) => current.Replace(escapeString.Key, escapeString.Value));
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                binaryWriter.Write(Parse(value));
            }
        }
    }
}