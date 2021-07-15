using System;

namespace Sudoku
{
    public static class TimerTimeUtility
    {
        private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        /// <summary>
        /// 当前时间
        /// </summary>
        /// <returns></returns>
        public static long Now()
        {
            return (DateTime.UtcNow.Ticks - epoch) / 10000;
        }
        
        // /// <summary>
        // /// 当前时间
        // /// </summary>
        // /// <returns></returns>
        // public static long Now()
        // {
        //     return (DateTime.UtcNow.Ticks - epoch) / 10000;
        // }
    }
}