using DelayQueue.Interfaces;
using System;
using System.Collections.Generic;
using TimingWheel.Extensions;

namespace TimingWheel
{
    /// <summary>
    /// 时间槽
    /// </summary>
    public class TimeSlot : IDelayItem
    {
        /// <summary>
        /// 过期时间戳，标识该时间槽的过期时间
        /// </summary>
        public AtomicLong TimeoutMs { get; } = new AtomicLong();

        /// <summary>
        /// 总任务数
        /// </summary>
        private readonly AtomicInt _taskCount;

        private readonly object _lock = new object();

        /// <summary>
        /// 任务队列
        /// </summary>
        private readonly LinkedList<TimeTask> _tasks = new LinkedList<TimeTask>();

        public TimeSlot(AtomicInt taskCount)
        {
            _taskCount = taskCount;
        }

        /// <summary>
        /// 添加定时任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public void AddTask(TimeTask task)
        {
            var done = false;
            while (!done)
            {
                // 先从其它队列移除掉
                // 在lock之外操作，避免死锁
                task.Remove();

                lock (_lock)
                {
                    if (task.TimeSlot == null)
                    {
                        _tasks.AddLast(task);
                        task.TimeSlot = this;
                        _taskCount.Increment();
                        done = true;
                    }
                }
            }
        }

        /// <summary>
        /// 移除定时任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public bool RemoveTask(TimeTask task)
        {
            lock (_lock)
            {
                if (task.TimeSlot == this)
                {
                    if (_tasks.Remove(task))
                    {
                        task.TimeSlot = null;
                        _taskCount.Decrement();
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 输出所有任务
        /// </summary>
        /// <param name="func"></param>
        public void Flush(Action<TimeTask> func)
        {
            lock (_lock)
            {
                while (_tasks.Count > 0 && _tasks.First != null)
                {
                    var task = _tasks.First.Value;
                    RemoveTask(task);
                    func(task);
                }

                // 重置过期时间，标识该时间槽已出队
                TimeoutMs.Set(default);
            }
        }

        /// <summary>
        /// 设置过期时间
        /// </summary>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        public bool SetExpiration(long timeoutMs)
        {
            // 第一次设置槽的时间，或是复用槽时，两者才不相等
            return TimeoutMs.GetAndSet(timeoutMs) != timeoutMs;
        }

        public TimeSpan GetDelaySpan()
        {
            var delayMs = Math.Max(TimeoutMs.Get() - DateTimeHelper.GetTimestamp(), 0);
            return TimeSpan.FromMilliseconds(delayMs);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is TimeSlot slot)
            {
                return TimeoutMs.CompareTo(slot.TimeoutMs);
            }

            throw new ArgumentException($"Object is not a {nameof(TimeSlot)}");
        }
    }
}