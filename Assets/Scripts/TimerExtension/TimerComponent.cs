using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions.Timer
{
    public class TimerComponent : GameFrameworkComponent
    {
        /// <summary>
        /// timer 类型
        /// </summary>
        private enum TimerType
        {
            /// <summary>
            /// 默认 无
            /// </summary>
            None,

            /// <summary>
            /// 等待执行一次
            /// </summary>
            OnceWait,

            /// <summary>
            /// 执行一次
            /// </summary>
            Once,

            /// <summary>
            /// 重复执行
            /// </summary>
            Repeated,
        }

        /// <summary>
        /// 定时器
        /// </summary>
        private class Timer : IReference
        {
            /// <summary>
            /// 自增id
            /// </summary>
            private static int _serialId;

            static Timer()
            {
                _serialId = 0;
            }

            /// <summary>
            /// timer 类型
            /// </summary>
            public TimerType TimerType { get; private set; }

            /// <summary>
            /// 计时结束回调函数
            /// </summary>
            public object Callback { get; private set; }

            /// <summary>
            /// 每帧回调函数 (返回剩余时间)
            /// </summary>
            public Action<long> UpdateCallBack { get; private set; }

            /// <summary>
            /// 时间
            /// </summary>
            public long Time { get; set; }

            /// <summary>
            /// 开始时间
            /// </summary>
            public long StartTime { get; set; }

            /// <summary>
            /// 开始时间
            /// </summary>
            public int RepeatCount { get; set; }

            /// <summary>
            /// ID
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// 创建定时器
            /// </summary>
            /// <param name="time">时间</param>
            /// <param name="startTime">开始时间</param>
            /// <param name="timerType">定时器类型</param>
            /// <param name="callback">回调</param>
            /// <param name="repeatCount">调用次数</param>
            /// <param name="updateCallBack">每帧回调</param>
            /// <returns>定时器</returns>
            public static Timer Create(long time, long startTime, TimerType timerType, object callback,
                int repeatCount = 0, Action<long> updateCallBack = null)
            {
                Timer timer = ReferencePool.Acquire<Timer>();
                timer.ID = _serialId++;
                timer.Time = time;
                timer.StartTime = startTime;
                timer.TimerType = timerType;
                timer.Callback = callback;
                timer.RepeatCount = repeatCount;
                timer.UpdateCallBack = updateCallBack;
                return timer;
            }

            public void Clear()
            {
                ID = -1;
                Time = 0;
                StartTime = 0;
                Callback = null;
                UpdateCallBack = null;
                RepeatCount = 0;
                TimerType = TimerType.None;
            }
        }


        /// <summary>
        /// 暂停的定时器
        /// </summary>
        private class PausedTimer : IReference
        {
            /// <summary>
            /// 被暂停的定时器
            /// </summary>
            public Timer Timer { get; private set; }

            /// <summary>
            /// 暂停时间
            /// </summary>
            public long PausedTime { get; private set; }

            /// <summary>
            /// 获取剩余运行时间
            /// </summary>
            /// <returns>剩余运行时间</returns>
            public long GetResidueTime()
            {
                return Timer.Time + Timer.StartTime - PausedTime;
            }

            /// <summary>
            /// 创建定时器
            /// </summary>
            /// <param name="pausedTime">暂停时间</param>
            /// <param name="pauseTimer">暂停的计时器</param>
            /// <returns>暂停的定时器</returns>
            public static PausedTimer Create(long pausedTime, Timer pauseTimer)
            {
                PausedTimer timer = ReferencePool.Acquire<PausedTimer>();
                timer.Timer = pauseTimer;
                timer.PausedTime = pausedTime;
                return timer;
            }

            public void Clear()
            {
                Timer = null;
                PausedTime = 0;
            }
        }

        /// <summary>
        /// 存储所有的timer
        /// </summary>
        private readonly Dictionary<int, Timer> _timers = new Dictionary<int, Timer>();

        /// <summary>
        /// 根据timer的到期时间存储 对应的 N个timerId
        /// </summary>
        private readonly MultiMap<long, int> _timeId = new MultiMap<long, int>();

        /// <summary>
        /// 需要执行的 到期时间
        /// </summary>
        private readonly Queue<long> _timeOutTime = new Queue<long>();

        /// <summary>
        /// 到期的所有 timerId
        /// </summary>
        private readonly Queue<int> _timeOutTimerIds = new Queue<int>();

        /// <summary>
        /// 暂停的计时器
        /// </summary>
        private readonly Dictionary<int, PausedTimer> _pausedTimer = new Dictionary<int, PausedTimer>();

        /// <summary>
        /// 需要每帧回调的计时器
        /// </summary>
        private readonly Dictionary<int, Timer> _updateTimer = new Dictionary<int, Timer>();

        /// <summary>
        /// 记录最小时间，不用每次都去MultiMap取第一个值
        /// </summary>
        private long _minTime;

        private void Update()
        {
            RunUpdateCallBack();
            if (_timeId.Count == 0)
            {
                return;
            }

            long timeNow = TimerTimeUtility.Now();

            if (timeNow < _minTime)
            {
                return;
            }

            foreach (KeyValuePair<long, List<int>> kv in _timeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    _minTime = k;
                    break;
                }

                _timeOutTime.Enqueue(k);
            }

            while (_timeOutTime.Count > 0)
            {
                long time = _timeOutTime.Dequeue();
                foreach (int timerId in _timeId[time])
                {
                    _timeOutTimerIds.Enqueue(timerId);
                }

                _timeId.Remove(time);
            }

            while (_timeOutTimerIds.Count > 0)
            {
                int timerId = _timeOutTimerIds.Dequeue();

                _timers.TryGetValue(timerId, out Timer timer);
                if (timer == null)
                {
                    continue;
                }

                RunTimer(timer);
            }
        }

        /// <summary>
        /// 执行每帧回调
        /// </summary>
        private void RunUpdateCallBack()
        {
            if (_updateTimer.Count == 0)
            {
                return;
            }

            long timeNow = TimerTimeUtility.Now();
            foreach (Timer timer in _updateTimer.Values)
            {
                timer.UpdateCallBack?.Invoke(timer.Time + timer.StartTime - timeNow);
            }
        }

        /// <summary>
        /// 执行定时器回调
        /// </summary>
        /// <param name="timer">定时器</param>
        private void RunTimer(Timer timer)
        {
            switch (timer.TimerType)
            {
                case TimerType.OnceWait:
                {
                    TaskCompletionSource<bool> tcs = timer.Callback as TaskCompletionSource<bool>;
                    RemoveTimer(timer.ID);
                    tcs?.SetResult(true);
                    break;
                }
                case TimerType.Once:
                {
                    Action action = timer.Callback as Action;
                    RemoveTimer(timer.ID);
                    action?.Invoke();
                    break;
                }
                case TimerType.Repeated:
                {
                    Action action = timer.Callback as Action;
                    long nowTime = TimerTimeUtility.Now();
                    long tillTime = nowTime + timer.Time;
                    if (timer.RepeatCount == 1)
                    {
                        RemoveTimer(timer.ID);
                    }
                    else
                    {
                        if (timer.RepeatCount > 1)
                        {
                            timer.RepeatCount--;
                        }

                        timer.StartTime = nowTime;
                        AddTimer(tillTime, timer.ID);
                    }

                    action?.Invoke();

                    break;
                }
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="tillTime">延时时间</param>
        /// <param name="id">定时器ID</param>
        private void AddTimer(long tillTime, int id)
        {
            _timeId.Add(tillTime, id);
            if (tillTime < _minTime)
            {
                _minTime = tillTime;
            }
        }

        /// <summary>
        /// 删除定时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        private void RemoveTimer(int id)
        {
            _timers.TryGetValue(id, out Timer timer);
            if (timer == null)
            {
                Debug.LogError($"删除了不存在的Timer ID:{id}");
                return;
            }
            
            ReferencePool.Release(timer);
            _timers.Remove(id);
            _updateTimer.Remove(id);
            if (_pausedTimer.ContainsKey(id))
            {
                ReferencePool.Release(_pausedTimer[id]);
                _pausedTimer.Remove(id);
            }
        }

        /// <summary>
        /// 取消计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void CancelTimer(int id)
        {
            if (_pausedTimer.ContainsKey(id))
            {
                ReferencePool.Release(_pausedTimer[id].Timer);
                ReferencePool.Release(_pausedTimer[id]);
                _pausedTimer.Remove(id);
                return;
            }

            RemoveTimer(id);
        }

        /// <summary>
        /// 查询是否存在计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public bool IsExistTimer(int id)
        {
            return _pausedTimer.ContainsKey(id) || _timers.ContainsKey(id);
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void PauseTimer(int id)
        {
            _timers.TryGetValue(id, out Timer oldTimer);
            if (oldTimer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timeId.Remove(oldTimer.StartTime + oldTimer.Time, oldTimer.ID);
            _timers.Remove(id);
            _updateTimer.Remove(id);
            PausedTimer timer = PausedTimer.Create(TimerTimeUtility.Now(), oldTimer);
            _pausedTimer.Add(id, timer);
        }

        /// <summary>
        /// 恢复计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void ResumeTimer(int id)
        {
            _pausedTimer.TryGetValue(id, out PausedTimer timer);
            if (timer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timers.Add(id, timer.Timer);
            if (timer.Timer.UpdateCallBack != null)
            {
                _updateTimer.Add(id, timer.Timer);
            }

            long tillTime = TimerTimeUtility.Now() + timer.GetResidueTime();
            timer.Timer.StartTime += TimerTimeUtility.Now() - timer.PausedTime;
            AddTimer(tillTime, timer.Timer.ID);
            ReferencePool.Release(timer);
            _pausedTimer.Remove(id);
        }

        /// <summary>
        /// 修改定时器时间
        /// </summary>
        /// <param name="id">定时器ID</param>
        /// <param name="time">修改时间</param>
        /// <param name="isChangeRepeat">是否修改如果是RepeatTimer每次运行时间</param>
        public void ChangeTime(int id, long time, bool isChangeRepeat = false)
        {
            _pausedTimer.TryGetValue(id, out PausedTimer pausedTimer);
            if (pausedTimer?.Timer != null)
            {
                pausedTimer.Timer.Time += time;
                return;
            }

            _timers.TryGetValue(id, out Timer oldTimer);
            if (oldTimer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
            }

            _timeId.Remove(oldTimer.StartTime + oldTimer.Time, oldTimer.ID);
            if (oldTimer.TimerType == TimerType.Repeated && !isChangeRepeat)
            {
                oldTimer.StartTime += time;
            }
            else
            {
                oldTimer.Time += time;
            }

            AddTimer(oldTimer.StartTime + oldTimer.Time, oldTimer.ID);
        }

        /// <summary>
        /// 添加执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallBack">每帧回调函数</param>
        /// <returns></returns>
        public int AddOnceTimer(long time, Action callback, Action<long> updateCallBack = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            long nowTime = TimerTimeUtility.Now();
            Timer timer = Timer.Create(time, nowTime, TimerType.Once, callback, 1, updateCallBack);
            _timers.Add(timer.ID, timer);
            if (updateCallBack != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            AddTimer(nowTime + time, timer.ID);
            return timer.ID;
        }

        /// <summary>
        /// 可等待执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> OnceTimerAsync(long time, CancellationToken cancellationToken = null)
        {
            long nowTime = TimerTimeUtility.Now();
            if (time <= 0)
            {
                return true;
            }
        
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Timer timer = Timer.Create(time, nowTime, TimerType.OnceWait, tcs);
            _timers.Add(timer.ID, timer);
            int timerId = timer.ID;
        
            AddTimer(nowTime + time, timerId);
        
            void CancelAction()
            {
                RemoveTimer(timerId);
                tcs.SetResult(false);
            }

            bool result;
            try
            {
                cancellationToken?.Add(CancelAction);
                result = await tcs.Task;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        
            return result;
        }

        /// <summary>
        /// 可等待的帧定时器
        /// </summary>
        /// <returns>定时器 ID</returns>
        public async Task<bool> FrameAsync(CancellationToken cancellationToken = null)
        {
            return await OnceTimerAsync(1, cancellationToken);
        }

        /// <summary>
        /// 添加执行多次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="repeatCount">重复次数 (小于等于零 无限次调用） </param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallback">每帧回调函数</param>
        /// <returns>定时器 ID</returns>
        /// <exception cref="Exception">定时时间太短 无意义</exception>
        public int AddRepeatedTimer(long time, int repeatCount, Action callback, Action<long> updateCallback = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            long nowTime = TimerTimeUtility.Now();
            Timer timer = Timer.Create(time, nowTime, TimerType.Repeated, callback, repeatCount, updateCallback);
            _timers.Add(timer.ID, timer);
            if (updateCallback != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            AddTimer(nowTime + time, timer.ID);
            return timer.ID;
        }

        public void AddRepeatedTimer(out int id, long time, int repeatCount, Action callback,
            Action<long> updateCallback = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            long nowTime = TimerTimeUtility.Now();
            Timer timer = Timer.Create(time, nowTime, TimerType.Repeated, callback, repeatCount, updateCallback);
            _timers.Add(timer.ID, timer);
            if (updateCallback != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            id = timer.ID;
            AddTimer(nowTime + time, timer.ID);
        }

        /// <summary>
        /// 添加帧定时器
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <returns>定时器 ID</returns>
        public int AddFrameTimer(Action callback)
        {
            long nowTime = TimerTimeUtility.Now();
            Timer timer = Timer.Create(1, nowTime, TimerType.Once, callback);
            _timers.Add(timer.ID, timer);
            AddTimer(nowTime + 1, timer.ID);
            return timer.ID;
        }
    }
}