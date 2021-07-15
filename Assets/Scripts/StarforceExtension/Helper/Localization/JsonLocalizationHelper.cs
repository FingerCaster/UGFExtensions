using System;
using System.IO;
using GameFramework;
using GameFramework.Localization;
using LitJson;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Sudoku
{
    public class JsonLocalizationHelper : DefaultLocalizationHelper
    {
        public override bool ParseData(ILocalizationManager localizationManager, string dictionaryString, object userData)
        {
            try
            {
                JsonData data = Utility.Json.ToObject<JsonData>(dictionaryString);
                foreach (string item in data.Keys)
                {
                    if (!localizationManager.AddRawString(item,  data[item].ToString()))
                    {
                        Log.Warning("Can not add raw string with key '{0}' which may be invalid or duplicate.", item);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
            return true;
        }
        
    }
}