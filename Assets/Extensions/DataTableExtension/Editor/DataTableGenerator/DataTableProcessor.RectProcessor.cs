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
        private sealed class RectProcessor : GenericDataProcessor<Rect>
        {
            public override bool IsSystem => false;

            public override string LanguageKeyword => "Rect";

            public override string[] GetTypeStrings()
            {
                return new[]
                {
                    "rect",
                    "unityengine.rect"
                };
            }

            public override Rect Parse(string value)
            {
                var splitedValue = value.Split(',');
                return new Rect(float.Parse(splitedValue[0]), float.Parse(splitedValue[1]),
                    float.Parse(splitedValue[2]), float.Parse(splitedValue[3]));
            }

            public override void WriteToStream(DataTableProcessor dataTableProcessor, BinaryWriter binaryWriter,
                string value)
            {
                var rect = Parse(value);
                binaryWriter.Write(rect.x);
                binaryWriter.Write(rect.y);
                binaryWriter.Write(rect.width);
                binaryWriter.Write(rect.height);
            }
        }
    }
}