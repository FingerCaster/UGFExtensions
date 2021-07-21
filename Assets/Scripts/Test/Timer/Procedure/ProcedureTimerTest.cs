using GameFramework.Fsm;
using GameFramework.Procedure;
using UGFExtensions.Timer;
using UnityEngine;

namespace UGFExtensions
{
    public class ProcedureTimerTest : ProcedureBase
    {
        private int m_TimerId;
        private int m_AwaitTimerId;
        private CancellationToken m_CancellationToken;
        protected override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
           Debug.Log(TimerTimeUtility.Now());
            m_TimerId = GameEntry.Timer.AddOnceTimer(4000, () =>
            {
                Debug.Log(TimerTimeUtility.Now());
                Debug.Log("延时4s 执行！");
            }, l =>
            {
                Debug.Log($"剩余时间：{l}");
            });

            m_CancellationToken = new CancellationToken();
            Debug.Log(TimerTimeUtility.Now());
             bool result =  await GameEntry.Timer.OnceTimerAsync(5000,m_CancellationToken);
             if (result)
             {
                 Debug.Log(TimerTimeUtility.Now());
                 Debug.Log("延时5秒执行");
             }
             else
             {
                 Debug.Log("提前取消");
             }

        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (Input.GetKeyDown(KeyCode.A))
            {
                GameEntry.Timer.PauseTimer(m_TimerId);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                GameEntry.Timer.ResumeTimer(m_TimerId);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                GameEntry.Timer.CancelTimer(m_TimerId);
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                m_CancellationToken.Cancel();
            }
        }
    }
}