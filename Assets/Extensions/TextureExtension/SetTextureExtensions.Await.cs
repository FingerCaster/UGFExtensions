using ET;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Texture
{
    public static partial class SetTextureExtensions
    {
        public static void SetTextureByNetworkAsync(this RawImage rawImage, string file, string saveFilePath = null,ETCancellationToken cancellationToken =null)
        {
            GameEntry.GetComponent<TextureSetComponent>().SetTextureByNetworkAsync(SetRawImage.Create(rawImage, file), saveFilePath,cancellationToken);
        }

        public static void SetTextureByResourcesAsync(this RawImage rawImage, string file,ETCancellationToken cancellationToken = null)
        {
            GameEntry.GetComponent<TextureSetComponent>().SetTextureByResourcesAsync(SetRawImage.Create(rawImage, file),cancellationToken);
        }
    }
}