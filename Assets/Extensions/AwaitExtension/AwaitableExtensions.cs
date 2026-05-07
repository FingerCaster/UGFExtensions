using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Await
{
    public static partial class AwaitableExtensions
    {
        private static readonly Dictionary<int, TaskCompletionSource<UIForm>> s_UIFormTcs =
            new Dictionary<int, TaskCompletionSource<UIForm>>();

        private static readonly Dictionary<int, TaskCompletionSource<Entity>> s_EntityTcs =
            new Dictionary<int, TaskCompletionSource<Entity>>();

        private static readonly Dictionary<string, TaskCompletionSource<bool>> s_DataTableTcs =
            new Dictionary<string, TaskCompletionSource<bool>>();

        private static readonly Dictionary<string, TaskCompletionSource<bool>> s_LoadSceneTcs =
            new Dictionary<string, TaskCompletionSource<bool>>();
        private static readonly Dictionary<string, TaskCompletionSource<bool>> s_UnLoadSceneTcs =
            new Dictionary<string, TaskCompletionSource<bool>>();

        private static readonly HashSet<int> s_WebSerialIDs = new HashSet<int>();

        private static readonly HashSet<int> s_DownloadSerialIds = new HashSet<int>();

        private static readonly HashSet<string> s_AllowedHttpHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

#if UNITY_EDITOR
        private static bool s_IsSubscribeEvent = false;
#endif

        public static void SetAllowedHttpHosts(params string[] hosts)
        {
            s_AllowedHttpHosts.Clear();
            if (hosts == null)
            {
                return;
            }

            foreach (string host in hosts)
            {
                if (!string.IsNullOrWhiteSpace(host))
                {
                    s_AllowedHttpHosts.Add(host.Trim());
                }
            }
        }

        private static bool IsValidHttpUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri) || !Uri.TryCreate(uri, UriKind.Absolute, out Uri parsedUri))
            {
                return false;
            }

            if (!parsedUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(parsedUri.UserInfo) ||
                !string.IsNullOrEmpty(parsedUri.Query) ||
                !string.IsNullOrEmpty(parsedUri.Fragment))
            {
                return false;
            }

            string host = parsedUri.Host;
            if (string.IsNullOrWhiteSpace(host) ||
                host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!s_AllowedHttpHosts.Contains(host))
            {
                return false;
            }

            if (IPAddress.TryParse(host, out IPAddress address))
            {
                return !IsPrivateOrLoopbackAddress(address);
            }

            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                if (addresses.Length == 0)
                {
                    return false;
                }

                foreach (IPAddress resolvedAddress in addresses)
                {
                    if (IsPrivateOrLoopbackAddress(resolvedAddress))
                    {
                        return false;
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }

            return true;
        }

        private static bool IsSafeDownloadPath(string downloadPath)
        {
            if (string.IsNullOrWhiteSpace(downloadPath) || downloadPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }

            string normalized = downloadPath.Replace('\\', '/');
            string[] segments = normalized.Split('/');
            foreach (string segment in segments)
            {
                if (string.IsNullOrEmpty(segment) || segment == "." || segment == "..")
                {
                    return false;
                }
            }

            if (!Path.IsPathRooted(downloadPath))
            {
                return true;
            }

            string fullPath = Path.GetFullPath(downloadPath);
            return IsPathInRoot(fullPath, Application.persistentDataPath) ||
                   IsPathInRoot(fullPath, Application.temporaryCachePath);
        }

        private static bool IsPathInRoot(string path, string root)
        {
            string fullPath = Path.GetFullPath(path);
            string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPrivateOrLoopbackAddress(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            byte[] bytes = address.GetAddressBytes();
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return bytes[0] == 0 ||
                       bytes[0] == 10 ||
                       bytes[0] == 127 ||
                       bytes[0] == 169 && bytes[1] == 254 ||
                       bytes[0] == 192 && bytes[1] == 168 ||
                       bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31;
            }

            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal ||
                       address.Equals(IPAddress.IPv6Loopback) ||
                       address.Equals(IPAddress.IPv6None) ||
                       address.Equals(IPAddress.IPv6Any) ||
                       address.IsIPv4MappedToIPv6 && IsPrivateOrLoopbackAddress(address.MapToIPv4()) ||
                       bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80 ||
                       bytes[0] == 0xfc || bytes[0] == 0xfd;
            }

            return false;
        }

        /// <summary>
        /// 注册需要的事件 (需再流程入口处调用 防止框架重启导致事件被取消问题)
        /// </summary>
        public static void SubscribeEvent()
        {
            EventComponent eventComponent = UnityGameFramework.Runtime.GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
            eventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            eventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            eventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            eventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            eventComponent.Subscribe(UnloadSceneSuccessEventArgs.EventId, OnUnloadSceneSuccess);
            eventComponent.Subscribe(UnloadSceneFailureEventArgs.EventId, OnUnloadSceneFailure);

            // eventComponent.Subscribe(LoadDataTableSuccessEventArgs.EventId, OnLoadDataTableSuccess);
            // eventComponent.Subscribe(LoadDataTableFailureEventArgs.EventId, OnLoadDataTableFailure);

            eventComponent.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
            eventComponent.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

            eventComponent.Subscribe(DownloadSuccessEventArgs.EventId, OnDownloadSuccess);
            eventComponent.Subscribe(DownloadFailureEventArgs.EventId, OnDownloadFailure);
#if UNITY_EDITOR
            s_IsSubscribeEvent = true;
#endif
        }

#if UNITY_EDITOR
        private static void TipsSubscribeEvent()
        {
            if (!s_IsSubscribeEvent)
            {
                throw new Exception("Use await/async extensions must to subscribe event!");
            }
        }
#endif

        /// <summary>
        /// 打开界面（可等待）
        /// </summary>
        public static Task<UIForm> OpenUIFormAsync(this UIComponent uiComponent,string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            int serialId = uiComponent.OpenUIForm(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
            var tcs = new TaskCompletionSource<UIForm>();
            s_UIFormTcs.Add(serialId, tcs);
            return tcs.Task;
        }

        private static void OnOpenUIFormSuccess(object sender, GameEventArgs e)
        {
            OpenUIFormSuccessEventArgs ne = (OpenUIFormSuccessEventArgs)e;
            s_UIFormTcs.TryGetValue(ne.UIForm.SerialId, out TaskCompletionSource<UIForm> tcs);
            if (tcs != null)
            {
                tcs.SetResult(ne.UIForm);
                s_UIFormTcs.Remove(ne.UIForm.SerialId);
            }
        }

        private static void OnOpenUIFormFailure(object sender, GameEventArgs e)
        {
            OpenUIFormFailureEventArgs ne = (OpenUIFormFailureEventArgs)e;
            s_UIFormTcs.TryGetValue(ne.SerialId, out TaskCompletionSource<UIForm> tcs);
            if (tcs != null)
            {
                Debug.LogError(ne.ErrorMessage);
                tcs.SetException(new GameFrameworkException(ne.ErrorMessage));
                s_UIFormTcs.Remove(ne.SerialId);
            }
        }

        /// <summary>
        /// 显示实体（可等待）
        /// </summary>
        public static Task<Entity> ShowEntityAsync(this EntityComponent entityComponent, int entityId,
            Type entityLogicType, string entityAssetName, string entityGroupName, int priority,object userData)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = new TaskCompletionSource<Entity>();
            s_EntityTcs.Add(entityId, tcs);
            entityComponent.ShowEntity(entityId, entityLogicType, entityAssetName, entityGroupName, priority, userData);
            return tcs.Task;
        }


        private static void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            ShowEntitySuccessEventArgs ne = (ShowEntitySuccessEventArgs)e;
            s_EntityTcs.TryGetValue(ne.Entity.Id, out var tcs);
            if (tcs != null)
            {
                tcs.SetResult(ne.Entity);
                s_EntityTcs.Remove(ne.Entity.Id);
            }
        }

        private static void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            ShowEntityFailureEventArgs ne = (ShowEntityFailureEventArgs)e;
            s_EntityTcs.TryGetValue(ne.EntityId, out var tcs);
            if (tcs != null)
            {
                Debug.LogError(ne.ErrorMessage);
                tcs.SetException(new GameFrameworkException(ne.ErrorMessage));
                s_EntityTcs.Remove(ne.EntityId);
            }
        }


        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static async Task<bool> LoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = new TaskCompletionSource<bool>();
            var isUnLoadScene = s_UnLoadSceneTcs.TryGetValue(sceneAssetName, out var unloadSceneTcs);
            if (isUnLoadScene)
            {
                await unloadSceneTcs.Task;
            }
            s_LoadSceneTcs.Add(sceneAssetName, tcs);

            try
            {
                sceneComponent.LoadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                tcs.SetException(e);
                s_LoadSceneTcs.Remove(sceneAssetName);
            }
            return await tcs.Task;
        }

        private static void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = (LoadSceneSuccessEventArgs)e;
            s_LoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                tcs.SetResult(true);
                s_LoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        private static void OnLoadSceneFailure(object sender, GameEventArgs e)
        {
            LoadSceneFailureEventArgs ne = (LoadSceneFailureEventArgs)e;
            s_LoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                Debug.LogError(ne.ErrorMessage);
                tcs.SetException(new GameFrameworkException(ne.ErrorMessage));
                s_LoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }
        
        /// <summary>
        /// 卸载场景（可等待）
        /// </summary>
        public static async Task<bool> UnLoadSceneAsync(this SceneComponent sceneComponent, string sceneAssetName)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            var tcs = new TaskCompletionSource<bool>();
            var isLoadSceneTcs = s_LoadSceneTcs.TryGetValue(sceneAssetName, out var loadSceneTcs);
            if (isLoadSceneTcs)
            {
                Debug.Log("Unload  loading scene");
                await loadSceneTcs.Task;
            }
            s_UnLoadSceneTcs.Add(sceneAssetName, tcs);
            try
            {
                sceneComponent.UnloadScene(sceneAssetName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                tcs.SetException(e);
                s_UnLoadSceneTcs.Remove(sceneAssetName);
            }
            return await tcs.Task;
        }
        private static void OnUnloadSceneSuccess(object sender, GameEventArgs e)
        {
            UnloadSceneSuccessEventArgs ne = (UnloadSceneSuccessEventArgs)e;
            s_UnLoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                tcs.SetResult(true);
                s_UnLoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        private static void OnUnloadSceneFailure(object sender, GameEventArgs e)
        {
            UnloadSceneFailureEventArgs ne = (UnloadSceneFailureEventArgs)e;
            s_UnLoadSceneTcs.TryGetValue(ne.SceneAssetName, out var tcs);
            if (tcs != null)
            {
                Debug.LogError($"Unload scene {ne.SceneAssetName} failure.");
                tcs.SetException(new GameFrameworkException($"Unload scene {ne.SceneAssetName} failure."));
                s_UnLoadSceneTcs.Remove(ne.SceneAssetName);
            }
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> LoadAssetAsync<T>(this ResourceComponent resourceComponent, string assetName)
            where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            TaskCompletionSource<T> loadAssetTcs = new TaskCompletionSource<T>();
            resourceComponent.LoadAsset(assetName, typeof(T), new LoadAssetCallbacks(
                (tempAssetName, asset, duration, userdata) =>
                {
                    var source = loadAssetTcs;
                    loadAssetTcs = null;
                    T tAsset = asset as T;
                    if (tAsset != null)
                    {
                        source.SetResult(tAsset);
                    }
                    else
                    {
                        Debug.LogError($"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}.");
                        source.SetException(new GameFrameworkException(
                            $"Load asset failure load type is {asset.GetType()} but asset type is {typeof(T)}."));
                    }
                },
                (tempAssetName, status, errorMessage, userdata) =>
                {
                    Debug.LogError(errorMessage);
                    loadAssetTcs.SetException(new GameFrameworkException(errorMessage));
                }
            ));

            return loadAssetTcs.Task;
        }

        /// <summary>
        /// 加载多个资源（可等待）
        /// </summary>
        public static async Task<T[]> LoadAssetsAsync<T>(this ResourceComponent resourceComponent, string[] assetName) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            if (assetName == null)
            {
                return null;
            }
            T[] assets = new T[assetName.Length];
            Task<T>[] tasks = new Task<T>[assets.Length];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = resourceComponent.LoadAssetAsync<T>(assetName[i]);
            }

            await Task.WhenAll(tasks);
            for (int i = 0; i < assets.Length; i++)
            {
                assets[i] = tasks[i].Result;
            }

            return assets;
        }


        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static Task<WebResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent,
            string webRequestUri, WWWForm wwwForm = null, object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            if (webRequestComponent == null || !IsValidHttpUri(webRequestUri))
            {
                return Task.FromResult(WebResult.Create(null, true, "Web request uri is invalid.", userdata));
            }

            var tsc = new TaskCompletionSource<WebResult>();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, wwwForm,
                AwaitDataWrap<WebResult>.Create(userdata, tsc));
            s_WebSerialIDs.Add(serialId);
            return tsc.Task;
        }

        /// <summary>
        /// 增加Web请求任务（可等待）
        /// </summary>
        public static Task<WebResult> AddWebRequestAsync(this WebRequestComponent webRequestComponent,
            string webRequestUri, byte[] postData, object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            if (webRequestComponent == null || !IsValidHttpUri(webRequestUri))
            {
                return Task.FromResult(WebResult.Create(null, true, "Web request uri is invalid.", userdata));
            }

            var tsc = new TaskCompletionSource<WebResult>();
            int serialId = webRequestComponent.AddWebRequest(webRequestUri, postData,
                AwaitDataWrap<WebResult>.Create(userdata, tsc));
            s_WebSerialIDs.Add(serialId);
            return tsc.Task;
        }

        private static void OnWebRequestSuccess(object sender, GameEventArgs e)
        {
            WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;
            if (s_WebSerialIDs.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<WebResult> webRequestUserdata)
                {
                    WebResult result = WebResult.Create(ne.GetWebResponseBytes(), false, string.Empty,
                        webRequestUserdata.UserData);
                    webRequestUserdata.Source.TrySetResult(result);
                    ReferencePool.Release(webRequestUserdata);
                }

                s_WebSerialIDs.Remove(ne.SerialId);
            }
        }

        private static void OnWebRequestFailure(object sender, GameEventArgs e)
        {
            WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
            if (s_WebSerialIDs.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<WebResult> webRequestUserdata)
                {
                    WebResult result = WebResult.Create(null, true, ne.ErrorMessage, webRequestUserdata.UserData);
                    webRequestUserdata.Source.TrySetResult(result);
                    ReferencePool.Release(webRequestUserdata);
                }

                s_WebSerialIDs.Remove(ne.SerialId);
            }
        }

        /// <summary>
        /// 增加下载任务（可等待)
        /// </summary>
        public static Task<DownLoadResult> AddDownloadAsync(this DownloadComponent downloadComponent,
            string downloadPath,
            string downloadUri,
            object userdata = null)
        {
#if UNITY_EDITOR
            TipsSubscribeEvent();
#endif
            if (downloadComponent == null || !IsSafeDownloadPath(downloadPath) || !IsValidHttpUri(downloadUri))
            {
                return Task.FromResult(DownLoadResult.Create(true, "Download path or uri is invalid.", userdata));
            }

            var tcs = new TaskCompletionSource<DownLoadResult>();
            int serialId = downloadComponent.AddDownload(downloadPath, downloadUri,
                AwaitDataWrap<DownLoadResult>.Create(userdata, tcs));
            s_DownloadSerialIds.Add(serialId);
            return tcs.Task;
        }

        private static void OnDownloadSuccess(object sender, GameEventArgs e)
        {
            DownloadSuccessEventArgs ne = (DownloadSuccessEventArgs)e;
            if (s_DownloadSerialIds.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<DownLoadResult> awaitDataWrap)
                {
                    DownLoadResult result = DownLoadResult.Create(false, string.Empty, awaitDataWrap.UserData);
                    awaitDataWrap.Source.TrySetResult(result);
                    ReferencePool.Release(awaitDataWrap);
                }

                s_DownloadSerialIds.Remove(ne.SerialId);
            }
        }

        private static void OnDownloadFailure(object sender, GameEventArgs e)
        {
            DownloadFailureEventArgs ne = (DownloadFailureEventArgs)e;
            if (s_DownloadSerialIds.Contains(ne.SerialId))
            {
                if (ne.UserData is AwaitDataWrap<DownLoadResult> awaitDataWrap)
                {
                    DownLoadResult result = DownLoadResult.Create(true, ne.ErrorMessage, awaitDataWrap.UserData);
                    awaitDataWrap.Source.TrySetResult(result);
                    ReferencePool.Release(awaitDataWrap);
                }

                s_DownloadSerialIds.Remove(ne.SerialId);
            }
        }
    }
}
