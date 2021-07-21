using System;
using UnityEngine;

namespace UGFExtensions
{
    public static partial class DataTableExtension
    {
        internal static readonly char[] DataSplitSeparators = {'\t'};
        internal static readonly char[] DataTrimSeparators = {'\"'};

        public static Color32 ParseColor32(string value)
        {
            var splitValue = value.Split(',');
            return new Color32(byte.Parse(splitValue[0]), byte.Parse(splitValue[1]), byte.Parse(splitValue[2]),
                byte.Parse(splitValue[3]));
        }

        public static Color ParseColor(string value)
        {
            var splitValue = value.Split(',');
            return new Color(float.Parse(splitValue[0]), float.Parse(splitValue[1]), float.Parse(splitValue[2]),
                float.Parse(splitValue[3]));
        }

        public static Quaternion ParseQuaternion(string value)
        {
            var splitValue = value.Split(',');
            return new Quaternion(float.Parse(splitValue[0]), float.Parse(splitValue[1]), float.Parse(splitValue[2]),
                float.Parse(splitValue[3]));
        }

        public static Rect ParseRect(string value)
        {
            var splitValue = value.Split(',');
            return new Rect(float.Parse(splitValue[0]), float.Parse(splitValue[1]), float.Parse(splitValue[2]),
                float.Parse(splitValue[3]));
        }

        public static Vector2 ParseVector2(string value)
        {
            var splitValue = value.Split(',');
            return new Vector2(float.Parse(splitValue[0]), float.Parse(splitValue[1]));
        }

        public static Vector3 ParseVector3(string value)
        {
            var splitValue = value.Split(',');
            return new Vector3(float.Parse(splitValue[0]), float.Parse(splitValue[1]), float.Parse(splitValue[2]));
        }

        public static Vector4 ParseVector4(string value)
        {
            var splitValue = value.Split(',');
            return new Vector4(float.Parse(splitValue[0]), float.Parse(splitValue[1]), float.Parse(splitValue[2]),
                float.Parse(splitValue[3]));
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