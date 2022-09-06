using System.Collections.Generic;
using GameFramework;
using GameFramework.Resource;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Test
{
    public static class LocalizationExtension
    {
        public static void LoadDictionary(this LocalizationComponent localizationComponent, string dictionaryName,
            bool fromBytes, object userData = null)
        {
            if (string.IsNullOrEmpty(dictionaryName))
            {
                Log.Warning("Dictionary name is invalid.");
                return;
            }
            GameEntry.Localization.ReadData( AssetUtility.GetDictionaryAsset(dictionaryName, fromBytes),userData); 
        }
        
    }
}