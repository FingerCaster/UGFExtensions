﻿#if !ODIN_INSPECTOR
using System;
using UnityEngine;

namespace UGFExtensions
{
    [Serializable]
    public class StringSpriteDictionary : SerializableDictionary<string, Sprite> {}
}
#endif