using System;
using System.Threading;
using HS.Console.Behaviors;
using UnityEngine;

namespace Unity.Console.Behaviors
{
    public delegate TResult Func<T, TResult>(T arg1);

    public class FuncBehavior : AsyncBehavior
    {
        private static object _lock = new object();
        private object arg;
        private System.Delegate func;
        private object result;

        public static TResult Execute<T,TResult>(Func<T, TResult> func, T arg) 
        {
            if (!Monitor.TryEnter(_lock))
            {
                return func(arg);
            }
            else
            {
                var obj2 = new GameObject(Guid.NewGuid().ToString());
                var ex = obj2.AddComponent<FuncBehavior>();
                if (ex == null)
                {
                    return default(TResult);
                }
                else
                {
                    ex.func = func;
                    ex.arg = arg;
                    ex.result = default(TResult);
                    ex.SetArgs(new object[] {arg});
                    ex.start = true;
                    ex.WaitFor(TimeSpan.FromSeconds(30));
                    return (TResult) ex.result;
                }
            }
        }

        public static TResult ExecuteAndWait<T, TResult>(Func<T, TResult> func, T arg, TimeSpan waitTime)
        {
            if (!Monitor.TryEnter(_lock))
            {
                return func(arg);
            }
            else
            {
                var obj2 = new GameObject(Guid.NewGuid().ToString());
                var ex = obj2.AddComponent<FuncBehavior>();
                ex.func = func;
                ex.arg = arg;
                ex.result = default(TResult);
                ex.SetArgs(new object[] {arg});
                ex.start = true;
                ex.WaitFor(waitTime);
                return (TResult) ex.result;
            }
        }
        
        // ReSharper disable once UnusedMember.Local
        protected override void Update()
        {
            base.Update();
        }

        protected override void SetArgs(object[] parameters)
        {
        }

        protected override object UpdateStep(int step)
        {
            result = func.DynamicInvoke(arg);
            return null;
        }
    }
}