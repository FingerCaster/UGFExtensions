using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using GameFramework.DataTable;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UGFExtensions.Await;
using UnityGameFramework.Runtime;
using Debug = UnityEngine.Debug;

namespace UGFExtensions
{
    public class ProcedureAwaitTest : ProcedureBase
    {
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            // 提前注册事件
            AwaitableExtensions.SubscribeEvent();

            var task1 = GameEntry.DataTable.LoadDataTableAsync<DRTestEnum>("TestEnum", true);
            var task2 = GameEntry.DataTable.LoadDataTableAsync<DRTestDictionary>("TestDictionary", true);
            var task3 = GameEntry.DataTable.LoadDataTableAsync<DRTest>("Test", true);
            await Task.WhenAll(task1, task2, task3);
    
            var drTest = task3.Result.GetDataRow(1);
            if (drTest == null) return;
            
            Debug.Log(
                $"{drTest.Id}    {drTest.BoolValue}    {drTest.ByteValue}    {drTest.CharValue}    {drTest.Color32Value}    {drTest.ColorValue}    {drTest.DateTimeValue}    " +
                $"{drTest.DecimalValue}    {drTest.DoubleValue}    {drTest.FloatValue}    {drTest.IntValue}    {drTest.LongValue}    {drTest.QuaternionValue}    {drTest.RectValue}    " +
                $"{drTest.SByteValue}    {drTest.ShortValue}    {drTest.StringValue}    {drTest.UIntValue}    {drTest.ULongValue}    {drTest.UShortValue}    {drTest.Vector2Value}    " +
                $"{drTest.Vector3Value}    {drTest.Vector4Value}");
            Debug.Log(
                $"{drTest.Id}    {ListToString(drTest.BoolList)}    {ListToString(drTest.ByteList)}    {ListToString(drTest.CharList)}    {ListToString(drTest.Color32List)}    {ListToString(drTest.ColorList)}    {ListToString(drTest.DateTimeList)}    " +
                $"{ListToString(drTest.DecimalList)}    {ListToString(drTest.DoubleList)}    {ListToString(drTest.FloatList)}    {ListToString(drTest.IntList)}    {ListToString(drTest.LongList)}    {ListToString(drTest.QuaternionList)}    {ListToString(drTest.RectList)}    " +
                $"{ListToString(drTest.SByteList)}    {ListToString(drTest.ShortList)}    {ListToString(drTest.StringList)}    {ListToString(drTest.UIntList)}    {ListToString(drTest.ULongList)}    {ListToString(drTest.UShortList)}    {ListToString(drTest.Vector2List)}    " +
                $"{ListToString(drTest.Vector3List)}    {ListToString(drTest.Vector4List)}");
            Debug.Log(
                $"{drTest.Id}    {ArrayToString(drTest.BoolArray)}    {ArrayToString(drTest.ByteArray)}    {ArrayToString(drTest.CharArray)}    {ArrayToString(drTest.Color32Array)}    {ArrayToString(drTest.ColorArray)}    {ArrayToString(drTest.DateTimeArray)}    " +
                $"{ArrayToString(drTest.DecimalArray)}    {ArrayToString(drTest.DoubleArray)}    {ArrayToString(drTest.FloatArray)}    {ArrayToString(drTest.IntArray)}    {ArrayToString(drTest.LongArray)}    {ArrayToString(drTest.QuaternionArray)}    {ArrayToString(drTest.RectArray)}    " +
                $"{ArrayToString(drTest.SByteArray)}    {ArrayToString(drTest.ShortArray)}    {ArrayToString(drTest.StringArray)}    {ArrayToString(drTest.UIntArray)}    {ArrayToString(drTest.ULongArray)}    {ArrayToString(drTest.UShortArray)}    {ArrayToString(drTest.Vector2Array)}    " +
                $"{ArrayToString(drTest.Vector3Array)}    {ArrayToString(drTest.Vector4Array)}");
            
            var drTestDictionary = task2.Result.GetDataRow(1);
            if (drTestDictionary == null)
                return;
            Debug.Log(
                $"{drTestDictionary.Id}    TestIntIntDictionary:{DictionaryToString(drTestDictionary.TestIntIntDictionary)}    TestIntVector3Dictionary:{DictionaryToString(drTestDictionary.TestIntVector3Dictionary)}");
            
            var drTestEnum = task1.Result.GetDataRow(1);
            if (drTestEnum == null)
                return;
            Debug.Log(
                $"{drTestEnum.Id}    TestEnum:{drTestEnum.TestEnum}   TestEnum1:{drTestEnum.TestEnum1}  TestEnumList:{ListToString(drTestEnum.TestEnumList)} " +
                $"TestEnumArray:{ArrayToString(drTestEnum.TestEnumArray)}   TestEnumDic:{DictionaryToString(drTestEnum.TestEnumDic)}");
            
           var IDic = await  GameEntry.DataTable.LoadDataTableAsync<DRTestDictionary>("TestDictionary", true);
           
           drTestDictionary = IDic.GetDataRow(1);
           if (drTestDictionary == null)
               return;
           Debug.Log(
               $"{drTestDictionary.Id}    TestIntIntDictionary:{DictionaryToString(drTestDictionary.TestIntIntDictionary)}    TestIntVector3Dictionary:{DictionaryToString(drTestDictionary.TestIntVector3Dictionary)}");

        }
        
        string ArrayToString<T>(T[] array)
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
            
        string ListToString<T>(List<T> array)
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
        string DictionaryToString<K, V>(Dictionary<K, V> dictionary)
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