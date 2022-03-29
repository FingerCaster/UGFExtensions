using System;
using System.Threading.Tasks;
using GameFramework.Fsm;
using GameFramework.Procedure;
using TimingWheel.Extensions;
using UGFExtensions.Timer;
using UnityEngine;
using UnityGameFramework.Runtime;


namespace UGFExtensions
{
    public class ProcedureTimingWheel : ProcedureBase
    {
        protected override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        private LoopTask m_LoopTask;

        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            await Task.Delay(1000);
            Log.Info($" 当前帧：{Time.frameCount}");
            await GameEntry.TimingWheel.AddFrameTaskAsync();
            Log.Info($" 当前帧：{Time.frameCount}");
          
            
            var task = GameEntry.TimingWheel.AddTask(TimeSpan.FromMilliseconds(1000), result => { Debug.Log(result); });
            await GameEntry.TimingWheel.AddTaskAsync(TimeSpan.FromMilliseconds(300));
            Debug.Log($"剩余时间：{task.TimeoutMs - DateTimeHelper.GetTimestamp()}");

            Test(1000);
            Test(2000);
            Test(300);
            Test(800);
            Test(812);

        }

        private async void Test(int delay)
        {
            var now = DateTimeHelper.GetTimestamp();
            await GameEntry.TimingWheel.AddTaskAsync(TimeSpan.FromMilliseconds(delay));
            var runTime = DateTimeHelper.GetTimestamp();
            var actualDelay2 = runTime - now;
            Debug.Log($"Timer 起始时间：{now}，" +
                      $"执行时间：{runTime}，" +
                      $"预期延时：{delay}ms，" +
                      $"精确延时：{actualDelay2}ms" +
                      $"延时误差：{actualDelay2 - delay}ms");
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.A))
            {
                Log.Info($" 每帧延迟 开始帧：{Time.frameCount}");
                m_LoopTask = UGFExtensions.GameEntry.TimingWheel.AddLoopTask(() =>
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