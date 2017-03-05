using System;
using System.Threading;
using HS.Console.Behaviors;

namespace Unity.Console.Behaviors
{
    class ParameterizedInvokedBehavior : AsyncBehavior
    {
        ParameterizedThreadStart func;
        object arg = null;

        public static void Execute(ParameterizedThreadStart start, object state)
        {
            AsyncBehavior.Run<ParameterizedInvokedBehavior>(start, state);
        }

        public static bool ExecuteAndWait(ParameterizedThreadStart start, object state, TimeSpan waitTime)
        {
            return AsyncBehavior.RunAndWait<ParameterizedInvokedBehavior>(waitTime, start, state);
        }

        protected override void SetArgs(object[] parameters)
        {
            //args = parameters;
            if (parameters.Length != 2)
                return;
            func = parameters[0] as ParameterizedThreadStart;
            arg = parameters[1];
        }

        protected override object UpdateStep(int step)
        {
            func?.Invoke(arg);
            return null;
        }
    }
}
