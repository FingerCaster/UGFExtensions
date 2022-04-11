using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;
using TimingWheel.Extensions;
using UnityEngine;
using UnityGameFramework.Runtime;
using Random = System.Random;


namespace UGFExtensions
{
    public class ProcedureTimingWheel : ProcedureBase
    {
        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        private long m_DelayTimes1 = 0;
        private long m_DelayTimes2 = 0;
        private LoopTask m_LoopTask;

        private int m_ID = 0;
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            await Task.Delay(1000);
            Log.Info($" 当前帧：{Time.frameCount}");
            await GameEntry.TimingWheel.AddFrameTaskAsync();
            Log.Info($" 当前帧：{Time.frameCount}");
          
            
            var task = GameEntry.TimingWheel.AddTask(TimeSpan.FromMilliseconds(1000), result => { Debug.Log(result); });
            await GameEntry.TimingWheel.AddTaskAsync(TimeSpan.FromMilliseconds(300));
            Debug.Log($"剩余时间：{task.TimeoutMs - DateTimeHelper.GetTimestamp()}");

            for (int i = 0; i < 10000; i++)
            {
                Test(i+100,i);
            }
            for (int i = 0; i < 10000; i++)
            {
                Test3(i+100,i);
            }
            // for (int i = 0; i < 10000; i++)
            // {
            //     Test3(i,i);
            // }

        }

        private async void Test(int delay,int id)
        {
            var now = DateTimeHelper.GetTimestamp();
            await GameEntry.TimingWheel.AddTaskAsync(TimeSpan.FromMilliseconds(delay));
            var runTime = DateTimeHelper.GetTimestamp();
            var actualDelay2 = runTime - now;
            m_DelayTimes1 += actualDelay2 - delay;
            if (id == 9999)
            {
                Debug.Log($"时间轮 task 总误差 :{m_DelayTimes1}  平均误差：{m_DelayTimes1 / 10000}");
            }
            Debug.Log($"ID: {id}  "+
                      $"Timer 起始时间：{now}，" +
                      $"执行时间：{runTime}，" +
                      $"预期延时：{delay}ms，" +
                      $"精确延时：{actualDelay2}ms" +
                      $"延时误差：{actualDelay2 - delay}ms");
        }
        
        private void Test2(int delay,int id)
        {
            var now = DateTimeHelper.GetTimestamp();
            GameEntry.TimingWheel.AddTask(TimeSpan.FromMilliseconds(delay), result =>
            {
                var runTime = DateTimeHelper.GetTimestamp();
                var actualDelay2 = runTime - now;
                m_DelayTimes1 += actualDelay2 - delay;
                if (id == 9999)
                {
                    Debug.Log($"时间轮 总误差 :{m_DelayTimes1}  平均误差：{m_DelayTimes1 / 10000}");
                }
                // Debug.Log($"ID: {id}  "+
                //           $"Timer 起始时间：{now}，" +
                //           $"执行时间：{runTime}，" +
                //           $"预期延时：{delay}ms，" +
                //           $"精确延时：{actualDelay2}ms" +
                //           $"延时误差：{actualDelay2 - delay}ms");
            });
           
        }
        
        private void Test3(int delay,int id)
        {
            var now = DateTimeHelper.GetTimestamp();
            GameEntry.Timer.AddOnceTimer(delay, () =>
            {
                var runTime = DateTimeHelper.GetTimestamp();
                var actualDelay2 = runTime - now;
                m_DelayTimes1 += actualDelay2 - delay;
                if (id == 9999)
                {
                    Debug.Log($"红黑树 总误差 :{m_DelayTimes1}  平均误差：{m_DelayTimes1 / 10000}");
                }
                //
                Debug.Log($"ID: {id}  "+
                          $"Timer 起始时间：{now}，" +
                          $"执行时间：{runTime}，" +
                          $"预期延时：{delay}ms，" +
                          $"精确延时：{actualDelay2}ms" +
                          $"延时误差：{actualDelay2 - delay}ms");
            });
           
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.A))
            {
                Log.Info($" 每帧延迟 开始帧：{Time.frameCount}");
                m_LoopTask = UGFExtensions.GameEntry.TimingWheel.AddLoopTask((statTime,task) =>
                {
                    Log.Info($" 每帧延迟 当前时间毫秒:{DateTimeHelper.GetTimestamp(false)}  当前时间秒:{DateTimeHelper.GetTimestamp(true)} 当前帧：{Time.frameCount}");
                },LoopType.Millisecond,1500);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                m_LoopTask.Stop();
            }
        }
    }
}