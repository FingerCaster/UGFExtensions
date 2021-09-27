using System.Collections.Generic;
using System.Text;

namespace UGFExtensions
{
    public static class ToStringUtility
    {
        public static string ArrayToString<T>(T[] array)
        {
            var stringBuilder = new StringBuilder();
            var comma = ",";
            for (var i = 0; i < array.Length; i++)
            {
                var separator = i < array.Length - 1 ? comma : string.Empty;
                stringBuilder.Append($"{array[i].ToString()}{separator}");
            }

            return stringBuilder.ToString();
        }

        public static string ListToString<T>(List<T> array)
        {
            var stringBuilder = new StringBuilder();
            var comma = ",";
            for (var i = 0; i < array.Count; i++)
            {
                var separator = i < array.Count - 1 ? comma : string.Empty;
                stringBuilder.Append($"{array[i].ToString()}{separator}");
            }

            return stringBuilder.ToString();
        }

        public static string DictionaryToString<K, V>(Dictionary<K, V> dictionary)
        {
            var stringBuilder = new StringBuilder();
            var comma = ",";
            var index = 0;
            foreach (var keyValue in dictionary)
            {
                var separator = index < dictionary.Count - 1 ? comma : string.Empty;
                stringBuilder.Append($"{{{keyValue.Key.ToString()},{keyValue.Value.ToString()}}}{separator}");
            }

            return stringBuilder.ToString();
        }
    }
}