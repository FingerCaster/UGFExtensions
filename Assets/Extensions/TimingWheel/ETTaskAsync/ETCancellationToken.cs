using System;
using System.Collections.Generic;

namespace ET
{
    public class ETCancellationToken
    {
        private HashSet<Action> actions = new HashSet<Action>();

        private readonly object syncRoot = new object();

        public void Add(Action callback)
        {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            lock (this.syncRoot)
            {
                if (this.actions != null)
                {
                    this.actions.Add(callback);
                    return;
                }
            }

            callback.Invoke();
        }

        public void Remove(Action callback)
        {
            lock (this.syncRoot)
            {
                this.actions?.Remove(callback);
            }
        }

        public bool IsCancel()
        {
            lock (this.syncRoot)
            {
                return this.actions == null;
            }
        }

        public void Cancel()
        {
            HashSet<Action> runActions;
            lock (this.syncRoot)
            {
                if (this.actions == null)
                {
                    return;
                }

                runActions = this.actions;
                this.actions = null;
            }

            try
            {
                foreach (Action action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
