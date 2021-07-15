using System.Collections.Generic;
using GameFramework;
using GameFramework.Resource;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Sudoku
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

        // private static readonly Dictionary<string, string> TextEffects = new Dictionary<string, string>();
        //
        // public static void LoadTextEffect(this LocalizationComponent localizationComponent,
        //     Dictionary<string, bool> loadedFlag)
        // {
        //     TextEffects.Clear();
        //     GameEntry.Resource.LoadAsset("Assets/Res/Localization/TextEffect.txt", new LoadAssetCallbacks(
        //         (assetName, asset, duration, userData) =>
        //         {
        //             TextAsset textAsset = (TextAsset) asset;
        //             TextEffect[] data = Utility.Json.ToObject<TextEffect[]>(textAsset.text);
        //             foreach (TextEffect item in data)
        //             {
        //                 string[] strings = new string[item.ContentId.Length];
        //                 for (var i = 0; i < item.ContentId.Length; i++)
        //                 {
        //                     ushort id = item.ContentId[i];
        //                     strings[i] = GameEntry.Localization.GetString(id.ToString());
        //                 }
        //
        //                 string format;
        //                 if (string.IsNullOrEmpty(item.ContentFormat))
        //                 {
        //                     format = strings[0];
        //                 }
        //                 else
        //                 {
        //                     format = string.Format(item.ContentFormat, strings);
        //                 }
        //
        //                 TextEffects.Add(item.Id, format);
        //             }
        //
        //             loadedFlag["Localization.TextEffect"] = true;
        //         },
        //         (assetName, status, errorMessage, userData) =>
        //         {
        //             Log.Error("Can not load TextEffect 'TextEffect' from '{0}' with error message '{1}'.", assetName,
        //                 errorMessage);
        //         }));
        // }
        //
        // public static string GetText(this LocalizationComponent localizationComponent, string key)
        // {
        //     TextEffects.TryGetValue(key, out string value);
        //     return string.IsNullOrEmpty(value) ? Utility.Text.Format("<NoKey>.{0}", key) : value;
        // }

        // /// <summary>
        // /// 本地化图片字典
        // /// </summary>
        // /// Author : 石成智
        // private static readonly Dictionary<string, List<SpriteSetting>> SpriteDictionary =
        //     new Dictionary<string, List<SpriteSetting>>();
        //
        // public static void LoadImagePath(this LocalizationComponent localizationComponent,
        //     Dictionary<string, bool> loadedFlag)
        // {
        //     SpriteDictionary.Clear();
        //     GameEntry.Resource.LoadAsset("Assets/Res/Localization/Image.txt", new LoadAssetCallbacks(
        //         (assetName, asset, duration, userData) =>
        //         {
        //             TextAsset textAsset = (TextAsset) asset;
        //             SpriteSetting[] data = Utility.Json.ToObject<SpriteSetting[]>(textAsset.text);
        //             foreach (SpriteSetting item in data)
        //             {
        //                 if (SpriteDictionary.ContainsKey(AssetUtility.GetUIFormAsset(item.UIForm)))
        //                 {
        //                     SpriteDictionary[AssetUtility.GetUIFormAsset(item.UIForm)].Add(item);
        //                 }
        //                 else
        //                 {
        //                     SpriteDictionary.Add(AssetUtility.GetUIFormAsset(item.UIForm),
        //                         new List<SpriteSetting>() {item});
        //                 }
        //             }
        //
        //             loadedFlag["Localization.ImagePath"] = true;
        //         },
        //         (assetName, status, errorMessage, userData) =>
        //         {
        //             Log.Error("Can not load TextEffect 'TextEffect' from '{0}' with error message '{1}'.", assetName,
        //                 errorMessage);
        //         }));
        // }
        //
        // public static List<SpriteSetting> GetImageSettings(this LocalizationComponent localizationComponent, string key)
        // {
        //     SpriteDictionary.TryGetValue(key, out List<SpriteSetting> value);
        //     return value;
        // }
    }

    // public class TextEffect
    // {
    //     /// <summary>
    //     /// id
    //     /// </summary>
    //     public string Id { get; set; }
    //
    //     /// <summary>
    //     /// 所有内容id
    //     /// </summary>
    //     public ushort[] ContentId { get; set; }
    //
    //     /// <summary>
    //     /// 消息格式
    //     /// </summary>
    //     public string ContentFormat { get; set; }
    // }

    // /// <summary>
    // /// 国际化图片设置
    // /// </summary>
    // public sealed class SpriteSetting
    // {
    //     /// <summary>
    //     /// 界面id
    //     /// </summary>
    //     public string UIForm { get; set; }
    //
    //     /// <summary>
    //     /// 图片在图集中的名字
    //     /// </summary>
    //     public string SpriteName { get; set; }
    //
    //     /// <summary>
    //     /// 图片项目使用地址
    //     /// </summary>
    //     public string Image { get; set; }
    // }
}