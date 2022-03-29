using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ET;
using GameFramework;
using TimingWheel;
using TimingWheel.Extensions;
using TimingWheel.Interfaces;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UGFExtensions
{
    public class TimingWheelComponent : GameFrameworkComponent
    {
        [SerializeField] [Tooltip("时间槽大小")] private int m_TickSpan = 100;
        [SerializeField] [Tooltip("时间槽数量")] private int m_SlotCount = 100;

        private ITimer m_Timer;
        private void Start()
        {
            Loom.Instance.Update();//初始化一下Loom
            m_Timer = TimingWheelTimer.Build(TimeSpan.FromMilliseconds(m_TickSpan), m_SlotCount);
            m_Timer.Start();
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="timeout">过期时间，相对时间</param>
        /// <param name="cancellationToken">任务取消令牌</param>
        /// <returns></returns>
        public ETTask<bool> AddTaskAsync(TimeSpan timeout, ETCancellationToken cancellationToken = default)
        {
            return m_Timer.AddTask(timeout, cancellationToken);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="timeout">过期时间，相对时间</param>
        /// <param name="callback">任务回调(true 成功执行  false 取消执行)</param>
        /// <returns></returns>
        public ITimeTask AddTask(TimeSpan timeout, Action<bool> callback)
        {
            return m_Timer.AddTask(timeout, callback);
        }


        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="timeoutMs">过期时间戳，绝对时间</param>
        /// <param name="cancellationToken">任务取消令牌</param>
        /// <returns></returns>
        public ETTask<bool> AddTaskAsync(long timeoutMs, ETCancellationToken cancellationToken = default)
        {
            return m_Timer.AddTask(timeoutMs, cancellationToken);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="timeoutMs">过期时间戳，绝对时间</param>
        /// <param name="callback">任务回调(true 成功执行  false 取消执行)</param>
        /// <returns></returns>
        public ITimeTask AddTask(long timeoutMs, Action<bool> callback)
        {
            return m_Timer.AddTask(timeoutMs, callback);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void StartTimer()
        {
            m_Timer.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void StopTimer()
        {
            m_Timer.Stop();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void PauseTimer()
        {
            m_Timer.Pause();
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void ResumeTimer()
        {
            m_Timer.Start();
        }


        private void Update()
        {
            Loom.Instance.Update();
            for (int i = m_UpdateFrameTimeTasks.Count - 1; i >= 0; i--)
            {
                if (m_UpdateFrameTimeTasks[i] == null || !m_UpdateFrameTimeTasks[i].IsLoop)
                {
                    ReferencePool.Release(m_UpdateFrameTimeTasks[i]);
                    m_UpdateFrameTimeTasks.RemoveAt(i);
                    continue;
                }

                m_UpdateFrameTimeTasks[i].Update();
            }
        }

        /// <summary>
        /// 添加帧定时任务
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <returns>定时器 ID</returns>
        public void AddFrameTask(Action callback)
        {
            // StartCoroutine(FrameTask(callback));
            Loom.Instance.PostNext(callback);
        }

        /// <summary>
        /// 添加帧定时任务
        /// </summary>
        /// <returns>定时器 ID</returns>
        public async ETTask AddFrameTaskAsync()
        {
            ETTask task = ETTask.Create(true);

            void CallBack()
            {
                task.SetResult();
            }

            AddFrameTask(CallBack);
            await task;
        }

        private List<LoopTask> m_UpdateFrameTimeTasks = new List<LoopTask>();

        /// <summary>
        /// 添加循环调用任务
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="loopType">循环类型 0帧  1 毫秒 </param>
        /// <param name="rateCount"></param>
        /// <returns></returns>
        public LoopTask AddLoopTask(Action callback, LoopType loopType, int rateCount)
        {
            LoopTask task = LoopTask.Create(callback, loopType, rateCount);
            m_UpdateFrameTimeTasks.Add(task);
            return task;
        }
    }

    /// <summary>
    /// 循环调用类型
    /// </summary>
    public enum LoopType
    {
        Frame = 0,
        Millisecond = 1,
    }

    /// <summary>
    /// 循环任务
    /// </summary>
    public class LoopTask : IReference
    {
        /// <summary>
        /// 是否循环
        /// </summary>
        public bool IsLoop { get; private set; }
        /// <summary>
        /// 回调
        /// </summary>
        private Action CallBack { get; set; }
        /// <summary>
        /// 循环类型
        /// </summary>
        private LoopType LoopType { get; set; }
        /// <summary>
        /// 循环速率
        /// </summary>
        private int RateCount { get; set; }
        private int LastCount { get; set; }

        public static LoopTask Create(Action callback, LoopType loopType, int rateCount)
        {
            LoopTask loopTask = ReferencePool.Acquire<LoopTask>();
            loopTask.IsLoop = true;
            loopTask.CallBack = callback;
            loopTask.LoopType = loopType;
            loopTask.RateCount = rateCount;
            return loopTask;
        }

        public void Update()
        {
            switch (LoopType)
            {
                case LoopType.Frame:
                {
                    LastCount++;
                    if (LastCount == RateCount)
                    {
                        this.CallBack();
                        LastCount = 0;
                    }
                }
                    break;
                case LoopType.Millisecond:
                    if (LastCount==0)
                    {
                        LastCount = -1;
                        GameEntry.TimingWheel.AddTask(TimeSpan.FromMilliseconds(RateCount), MillisecondCallBack);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void MillisecondCallBack(bool result)
        {
            LastCount = 0;
            this.CallBack();
        }
        public void Stop()
        {
            IsLoop = false;
        }

        public void Clear()
        {
            IsLoop = default;
            CallBack = default;
            LoopType = default;
            RateCount = default;
        }
    }
}