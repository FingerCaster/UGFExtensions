namespace UGFExtensions
{
    public class VersionInfo
    {
        /// <summary>
        /// 是否需要强制更新游戏应用
        /// </summary>
        public bool ForceUpdateGame { get; set; }

        /// <summary>
        /// 最新的游戏版本号
        /// </summary>

        public string LatestGameVersion { get; set; }

        /// <summary>
        /// 最新的游戏内部版本号
        /// </summary>
        public int InternalGameVersion { get; set; }

        /// <summary>
        /// 最新的资源内部版本号
        /// </summary>
        public int InternalResourceVersion { get; set; }

        /// <summary>
        /// 资源更新下载地址
        /// </summary>
        public string UpdatePrefixUri { get; set; }

        /// <summary>
        /// 资源版本列表长度
        /// </summary>

        public int VersionListLength { get; set; }

        /// <summary>
        /// 资源版本列表哈希值
        /// </summary>
        public int VersionListHashCode { get; set; }

        /// <summary>
        /// 资源版本列表压缩后长度
        /// </summary>
        public int VersionListCompressedLength { get; set; }

        /// <summary>
        /// 资源版本列表压缩后哈希值
        /// </summary>
        public int VersionListCompressedHashCode { get; set; }
    }
}