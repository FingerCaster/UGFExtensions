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
        private static void InitCustomComponents()
        {
            Timer = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
        }
    }
}