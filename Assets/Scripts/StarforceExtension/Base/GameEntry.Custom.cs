using UGFExtensions.SpriteCollection;
using UGFExtensions.Timer;

namespace UGFExtensions
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        /// <summary>
        /// 获取游戏基础组件。
        /// </summary>
        public static TimerComponent Timer
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取游戏基础组件。
        /// </summary>
        public static SpriteCollectionComponent SpriteCollection
        {
            get;
            private set;
        }
        /// <summary>
        /// 自定义数据组件
        /// </summary>
        public static BuiltinDataComponent BuiltinData { get; private set; }
        private static void InitCustomComponents()
        {
            Timer = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
            BuiltinData = UnityGameFramework.Runtime.GameEntry.GetComponent<BuiltinDataComponent>();
            SpriteCollection = UnityGameFramework.Runtime.GameEntry.GetComponent<SpriteCollectionComponent>();
        }
    }
}