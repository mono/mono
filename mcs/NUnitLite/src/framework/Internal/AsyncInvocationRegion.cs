#if NET_4_5
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace NUnit.Framework.Internal
{
    internal abstract class AsyncInvocationRegion : IDisposable
    {
        private static readonly Type AsyncStateMachineAttribute = Type.GetType("System.Runtime.CompilerServices.AsyncStateMachineAttribute");
#if __MOBILE__
        static void PreserveStackTrace (Exception e)
        {
        }
#else
        private static readonly MethodInfo PreserveStackTraceMethod = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Action<Exception> PreserveStackTrace;

        static AsyncInvocationRegion()
        {
            PreserveStackTrace = (Action<Exception>)Delegate.CreateDelegate(typeof(Action<Exception>), PreserveStackTraceMethod);
        }
#endif

        private AsyncInvocationRegion()
        {
        }

        public static AsyncInvocationRegion Create(Delegate @delegate)
        {
            return Create(@delegate.Method);
        }

        public static AsyncInvocationRegion Create(MethodInfo method)
        {
            if (!IsAsyncOperation(method))
                throw new InvalidOperationException(@"Either asynchronous support is not available or an attempt 
at wrapping a non-async method invocation in an async region was done");

            if (method.ReturnType == typeof(void))
                return new AsyncVoidInvocationRegion();

            return new AsyncTaskInvocationRegion();
        }

        public static bool IsAsyncOperation(MethodInfo method)
        {
            return AsyncStateMachineAttribute != null && method.IsDefined(AsyncStateMachineAttribute, false);
        }

        public static bool IsAsyncOperation(Delegate @delegate)
        {
            return IsAsyncOperation(@delegate.Method);
        }

        /// <summary>
        /// Waits for pending asynchronous operations to complete, if appropriate,
        /// and returns a proper result of the invocation by unwrapping task results
        /// </summary>
        /// <param name="invocationResult">The raw result of the method invocation</param>
        /// <returns>The unwrapped result, if necessary</returns>
        public abstract object WaitForPendingOperationsToComplete(object invocationResult);

        public virtual void Dispose()
        { }

        private class AsyncVoidInvocationRegion : AsyncInvocationRegion
        {
            private readonly SynchronizationContext _previousContext;
            private readonly AsyncSynchronizationContext _currentContext;

            public AsyncVoidInvocationRegion()
            {
                _previousContext = SynchronizationContext.Current;
                _currentContext = new AsyncSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(_currentContext);
            }

            public override void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(_previousContext);
            }

            public override object WaitForPendingOperationsToComplete(object invocationResult)
            {
                try
                {
                    _currentContext.WaitForPendingOperationsToComplete();
                    return invocationResult;
                }
                catch (Exception e)
                {
                    PreserveStackTrace(e);
                    throw;
                }
            }
        }

        private class AsyncTaskInvocationRegion : AsyncInvocationRegion
        {
            private const string TaskWaitMethod = "Wait";
            private const string TaskResultProperty = "Result";
            private const string SystemAggregateException = "System.AggregateException";
            private const string InnerExceptionsProperty = "InnerExceptions";
            private const BindingFlags TaskResultPropertyBindingFlags = BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;

            public override object WaitForPendingOperationsToComplete(object invocationResult)
            {
                try
                {
                    invocationResult.GetType().GetMethod(TaskWaitMethod, new Type[0]).Invoke(invocationResult, null);
                }
                catch (TargetInvocationException e)
                {
                    IList<Exception> innerExceptions = GetAllExceptions(e.InnerException);

                    PreserveStackTrace(innerExceptions[0]);
                    throw innerExceptions[0];
                }

                PropertyInfo taskResultProperty = invocationResult.GetType().GetProperty(TaskResultProperty, TaskResultPropertyBindingFlags);

                return taskResultProperty != null ? taskResultProperty.GetValue(invocationResult, null) : invocationResult;
            }

            private static IList<Exception> GetAllExceptions(Exception exception)
            {
                if (SystemAggregateException.Equals(exception.GetType().FullName))
                    return (IList<Exception>)exception.GetType().GetProperty(InnerExceptionsProperty).GetValue(exception, null);

                return new Exception[] { exception };
            }
        }
    }
}
#endif