using System;
using GameFramework.Fsm;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = UGFExtensions.Test.GameEntry;

namespace Test
{
    public class TestFSM : MonoBehaviour
    {
        private IFsm<TestFSM> m_Fsm;
        private void Start()
        {
            State1 state1 = new State1();
            State2 state2 = new State2();
            IFsm<TestFSM> fsm = GameEntry.Fsm.CreateFsm(this, state1, state2);
            fsm.Start(typeof(State1));
        }

        public void Console(string str)
        {
            Debug.Log(str);
        }
    }

    public class State1 : FsmState<TestFSM>
    {

        protected override void OnEnter(IFsm<TestFSM> fsm)
        {
            base.OnEnter(fsm);
            Log.Info($"{nameof(State1)} OnEnter !");
        }
        protected override void OnUpdate(IFsm<TestFSM> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);
            if (Input.GetKeyDown(KeyCode.A))
            {
                fsm.Owner.Console("A");
                ChangeState<State2>(fsm);
            }
        }
        protected override void OnLeave(IFsm<TestFSM> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            Log.Info($"{nameof(State1)} OnLeave !");
        }
    }
    public class State2 : FsmState<TestFSM>
    {


        protected override void OnEnter(IFsm<TestFSM> fsm)
        {
            base.OnEnter(fsm);
            Log.Info($"{nameof(State2)} OnEnter !");
        }

        protected override void OnUpdate(IFsm<TestFSM> fsm, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);
            if (Input.GetKeyDown(KeyCode.B))
            {
                fsm.Owner.Console("B");
                ChangeState<State1>(fsm);
            }
        }

        protected override void OnLeave(IFsm<TestFSM> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
            Log.Info($"{nameof(State2)} OnLeave !");
        }
    }
}