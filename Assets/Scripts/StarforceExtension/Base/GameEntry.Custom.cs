using UGFExtensions.SpriteCollection;
using UGFExtensions.Texture;
using UGFExtensions.Timer;

namespace UGFExtensions
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        /// <summary>
        /// 获取定时器组件。
        /// </summary>
        public static TimerComponent Timer
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取精灵收集组件。
        /// </summary>
        public static SpriteCollectionComponent SpriteCollection
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取图片设置组件。
        /// </summary>
        public static TextureSetComponent TextureSet
        {
            get;
            private set;
        }
        /// <summary>
        /// 自定义数据组件
        /// </summary>
        public static BuiltinDataComponent BuiltinData { get; private set; }
        
        /// <summary>
        /// 数据表扩展组件
        /// </summary>
        public static DataTableExtensionComponent DataTableExtension { get; private set; }
        private static void InitCustomComponents()
        {
            Timer = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
            BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
            SpriteCollection = UnityGameFramework.Runtime.GameEntry.GetComponent<SpriteCollectionComponent>();
            TextureSet = UnityGameFramework.Runtime.GameEntry.GetComponent<TextureSetComponent>();
            DataTableExtension = UnityGameFramework.Runtime.GameEntry.GetComponent<DataTableExtensionComponent>();
        }
    }
}