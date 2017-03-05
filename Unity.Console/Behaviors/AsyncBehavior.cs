using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace HS.Console.Behaviors
{
    public abstract class AsyncBehavior : MonoBehaviour
    {
        private readonly ManualResetEvent completeEvent = new ManualResetEvent(false);
        protected bool start;

        public static void Run<T>(params object[] args) where T : AsyncBehavior
        {
            RunAndWaitArgs<T>(TimeSpan.FromSeconds(30), args);
        }

        public static bool RunAndWait<T>(TimeSpan waitTime, params object[] args) where T : AsyncBehavior
        {
            return RunAndWaitArgs<T>(waitTime, args);
        }

        private static bool RunAndWaitArgs<T>(TimeSpan waitTime, object[] args) where T : AsyncBehavior
        {
            var obj2 = new GameObject(typeof(T).FullName);
            var ex = obj2.AddComponent<T>();
            ex.SetArgs(args);
            ex.start = true;
            return ex.WaitFor(waitTime);
        }

        // ReSharper disable once UnusedMember.Local
        protected virtual void Update()
        {
            if (!start) return;

            start = false;
            StartCoroutine(StartCoroutineProc());
        }

        protected abstract void SetArgs(object[] args);

        protected abstract object UpdateStep(int step);

        [DebuggerHidden]
        private IEnumerator StartCoroutineProc()
        {
            return new AsyncProcIter(this);
        }

        protected virtual void Start()
        {
            completeEvent.Reset();
        }

        public void Complete()
        {
            completeEvent.Set();
            OnComplete();
            Destroy(this.gameObject);
        }

        public virtual void OnComplete() { }

        public bool WaitFor(TimeSpan time)
        {
            var result = completeEvent.WaitOne(time,false);
            return result;
        }

        internal class AsyncProcIter : IEnumerator<object>
        {
            private readonly AsyncBehavior owner;
            private object current;
            private float oldTime;
            private int step;

            internal AsyncProcIter(AsyncBehavior owner)
            {
                this.owner = owner;
                step = 0;
                try
                {
                    owner.Start();
                }
                catch (Exception ex)
                {
                    step = -1;
                    System.Console.WriteLine("Exception: " + ex.Message);
                    Complete();
                }
            }

            void Complete()
            {
                try
                {
                    owner.Complete();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: " + ex.Message);
                }
            }

            [DebuggerHidden]
            public void Dispose()
            {
                step = -1;
            }

            public bool MoveNext()
            {
                var num = step;
                step = -1;
                if (num < 0)
                    return false;
                if (num == 0)
                {
                    oldTime = Time.timeScale;
                    Time.timeScale = 0;
                    current = new WaitForEndOfFrame();
                    step = num + 1;
                }
                else
                {
                    step = num + 1;
                    try
                    {
                        current = owner.UpdateStep(num);
                        if (current == null)
                            step = -1;
                    }
                    catch (Exception ex)
                    {
                        current = null;
                        step = -1;
                        System.Console.WriteLine("Exception: " + ex.Message);
                    }
                }
                if (step <= 0)
                {
                    Time.timeScale = oldTime;
                    Complete();
                }
                return step >= 0;
            }

            [DebuggerHidden]
            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator.Current
            {
                [DebuggerHidden] get { return current; }
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden] get { return current; }
            }
        }
    }
}