

using GameFramework;

namespace UGFExtensions.Test
{
    public static class AssetUtility
    {
        public static string GetConfigAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Res/Configs/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDataTableAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Res/DataTables/{0}.{1}", assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetDictionaryAsset(string assetName, bool fromBytes)
        {
            return Utility.Text.Format("Assets/Res/Localization/{0}/Dictionaries/{1}.{2}", GameEntry.Localization.Language.ToString(), assetName, fromBytes ? "bytes" : "txt");
        }

        public static string GetFontAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/Fonts/{0}.asset", assetName);
        }

        public static string GetSceneAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/Scenes/{0}.unity", assetName);
        }

        public static string GetMusicAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/Music/{0}.mp3", assetName);
        }

        public static string GetSoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/Sounds/{0}.wav", assetName);
        }

        public static string GetEntityAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/Entities/{0}.prefab", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/UI/UIForms/{0}.prefab", assetName);
        }
        public static string GetUISoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/Res/UI/UISounds/{0}.wav", assetName);
        }
    }
}
