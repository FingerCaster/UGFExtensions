using UnityEngine;

namespace UGFExtensions.Build.Editor
{
   


    [CreateAssetMenu(fileName = "VersionInfo", menuName = "UGFExtensions/VersionInfo", order = 0)]
    public class VersionInfoData : ScriptableObject
    {
        public enum EnvironmentType
        {
            Debug,
            Release
        }
        [SerializeField] private bool m_ForceUpdateGame;
        [SerializeField] private string m_LatestGameVersion;
        [SerializeField] private int m_InternalGameVersion;
        [SerializeField] private string m_UpdatePrefixUri;
        [SerializeField] private int m_VersionListLength;
        [SerializeField] private int m_InternalResourceVersion;
        [SerializeField] private int m_VersionListHashCode;
        [SerializeField] private int m_VersionListCompressedLength;
        [SerializeField] private int m_VersionListCompressedHashCode;
        [SerializeField] private EnvironmentType m_Environment ;

        public EnvironmentType Environment
        {
            get => m_Environment;
            set => m_Environment = value;
        }

        /// <summary>
        /// 是否需要强制更新游戏应用
        /// </summary>
        public bool ForceUpdateGame
        {
            get => m_ForceUpdateGame;
            set => m_ForceUpdateGame = value;
        }

        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string LatestGameVersion
        {
            get => m_LatestGameVersion;
            set => m_LatestGameVersion = value;
        }

        /// <summary>
        /// 最新的游戏内部版本号
        /// </summary>
        public int InternalGameVersion
        {
            get => m_InternalGameVersion;
            set => m_InternalGameVersion = value;
        }

        /// <summary>
        /// 最新的资源内部版本号
        /// </summary>
        public int InternalResourceVersion
        {
            get => m_InternalResourceVersion;
            set => m_InternalResourceVersion = value;
        }

        /// <summary>
        /// 资源更新下载地址
        /// </summary>
        public string UpdatePrefixUri
        {
            get => m_UpdatePrefixUri;
            set => m_UpdatePrefixUri = value;
        }

        /// <summary>
        /// 资源版本列表长度
        /// </summary>

        public int VersionListLength
        {
            get => m_VersionListLength;
            set => m_VersionListLength = value;
        }

        /// <summary>
        /// 资源版本列表哈希值
        /// </summary>
        public int VersionListHashCode
        {
            get => m_VersionListHashCode;
            set => m_VersionListHashCode = value;
        }

        /// <summary>
        /// 资源版本列表压缩后长度
        /// </summary>
        public int VersionListCompressedLength
        {
            get => m_VersionListCompressedLength;
            set => m_VersionListCompressedLength = value;
        }

        /// <summary>
        /// 资源版本列表压缩后哈希值
        /// </summary>
        public int VersionListCompressedHashCode
        {
            get => m_VersionListCompressedHashCode;
            set => m_VersionListCompressedHashCode = value;
        }
    }
}