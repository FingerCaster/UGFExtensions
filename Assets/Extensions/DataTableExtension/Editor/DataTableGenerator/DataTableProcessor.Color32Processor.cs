//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;
using UnityEngine;

namespace DE.Editor.DataTableTools
{
    public sealed partial class DataTableProcessor
    {
        private sealed class Color32Processor : GenericDataProcessor<Color32>
        {
            public override bool IsSystem => false;

            public override string LanguageKeyword => "Color32";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "color32",
                    "unityengine.color32"
                };
            }

            public override Color32 Parse(string value)
            {
                var splitedValue = value.Split(',');
                return new Color32(byte.Parse(splitedValue[0]), byte.Parse(splitedValue[1]),
                    byte.Parse(splitedValue[2]), byte.Parse(splitedValue[3]));
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                var color32 = Parse(value);
                binaryWriter.Write(color32.r);
                binaryWriter.Write(color32.g);
                binaryWriter.Write(color32.b);
                binaryWriter.Write(color32.a);
            }
        }
    }
}