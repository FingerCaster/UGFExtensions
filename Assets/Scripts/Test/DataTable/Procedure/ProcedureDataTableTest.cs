using System;
using System.Collections.Generic;
using System.Text;
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;
using GameFramework.Procedure;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    public class ProcedureDataTableTest : ProcedureBase
    {
        private const string DataRowClassPrefixName = "UGFExtensions.DR";

        public static readonly string[] DataTableNames =
        {
            "Test", // 测试 
            "TestDictionary", // 测试字典
            "TestEnum" // 测试枚举
        };

        private readonly Dictionary<string, bool> m_LoadedFlag = new Dictionary<string, bool>();

        /// <summary>
        ///     获取事件组件。
        /// </summary>
        public static EventComponent Event { get; private set; }

        /// <summary>
        ///     获取数据表组件。
        /// </summary>
        public static DataTableComponent DataTable { get; private set; }

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Event = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
            DataTable = UnityGameFramework.Runtime.GameEntry.GetComponent<DataTableComponent>();
            Event.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            Event.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
            PreloadResources();
        }

        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            Event.Unsubscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            Event.Unsubscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);
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
        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            foreach (var loadedFlag in m_LoadedFlag)
                if (!loadedFlag.Value)
                    return;
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                var drTests = DataTable.GetDataTable<DRTest>();
                var drTest = drTests.GetDataRow(1);
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
            
                
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                var drTestDictionaries = DataTable.GetDataTable<DRTestDictionary>();
                var drTestDictionary = drTestDictionaries.GetDataRow(1);
                if (drTestDictionary == null)
                    return;
                Debug.Log(
                    $"{drTestDictionary.Id}    TestIntIntDictionary:{DictionaryToString(drTestDictionary.TestIntIntDictionary)}    TestIntVector3Dictionary:{DictionaryToString(drTestDictionary.TestIntVector3Dictionary)}");
            
               
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                var drTestEnums = DataTable.GetDataTable<DRTestEnum>();
                var drTestEnum = drTestEnums.GetDataRow(1);
                if (drTestEnum == null)
                    return;
                Debug.Log(
                    $"{drTestEnum.Id}    TestEnum:{drTestEnum.TestEnum}   TestEnum1:{drTestEnum.TestEnum1}  TestEnumList:{ListToString(drTestEnum.TestEnumList)} " +
                    $"TestEnumArray:{ArrayToString(drTestEnum.TestEnumArray)}   TestEnumDic:{DictionaryToString(drTestEnum.TestEnumDic)}");
            
            }
        }

        private void PreloadResources()
        {
            // Preload data tables
            foreach (var dataTableName in DataTableNames) LoadDataTable(dataTableName);
        }

        private void LoadDataTable(string dataTableName)
        {
            var dataTableAssetName = Utility.Text.Format("Assets/Res/DataTables/{0}.txt", dataTableName);
            m_LoadedFlag.Add(dataTableAssetName, false);
            if (string.IsNullOrEmpty(dataTableName))
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            var splitedNames = dataTableName.Split('_');
            if (splitedNames.Length > 2)
            {
                Log.Warning("Data table name is invalid.");
                return;
            }

            var dataRowClassName = DataRowClassPrefixName + splitedNames[0];

            var dataRowType = Type.GetType(dataRowClassName);
            if (dataRowType == null)
            {
                Log.Warning("Can not get data row type with class name '{0}'.", dataRowClassName);
                return;
            }

            var name = splitedNames.Length > 1 ? splitedNames[1] : null;
            var dataTable = DataTable.CreateDataTable(dataRowType, name);
            dataTable.ReadData(dataTableAssetName, 100, this);
        }


        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Res/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        private void OnLoadDataTableSuccess(object sender, GameEventArgs e)
        {
            var ne = (LoadDataTableSuccessEventArgs) e;
            if (ne.UserData != this) return;

            m_LoadedFlag[ne.DataTableAssetName] = true;
            Log.Info("Load data table '{0}' OK.", ne.DataTableAssetName);
        }

        private void OnLoadDataTableFailure(object sender, GameEventArgs e)
        {
            var ne = (LoadDataTableFailureEventArgs) e;
            if (ne.UserData != this) return;

            Log.Error("Can not load data table '{0}' from '{1}' with error message '{2}'.", ne.DataTableAssetName,
                ne.DataTableAssetName, ne.ErrorMessage);
        }
    }
}