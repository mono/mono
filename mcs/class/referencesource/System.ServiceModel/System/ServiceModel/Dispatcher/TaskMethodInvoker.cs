// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading.Tasks;

    /// <summary>
    /// An invoker used when some operation contract has a return value of Task or its generic counterpart (Task of T) 
    /// </summary>
    internal class TaskMethodInvoker : IOperationInvoker
    {
        private const string ResultMethodName = "Result";
        private MethodInfo taskMethod;
        private bool isGenericTask;
        private InvokeDelegate invokeDelegate;
        private int inputParameterCount;
        private int outputParameterCount;
        private object[] outputs;
        private MethodInfo toAsyncMethodInfo;
        private MethodInfo taskTResultGetMethod;

        public TaskMethodInvoker(MethodInfo taskMethod, Type taskType)
        {
            if (taskMethod == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("taskMethod"));
            }

            this.taskMethod = taskMethod;

            if (taskType != ServiceReflector.VoidType)
            {
                this.toAsyncMethodInfo = TaskExtensions.MakeGenericMethod(taskType);
                this.taskTResultGetMethod = ((PropertyInfo)taskMethod.ReturnType.GetMember(ResultMethodName)[0]).GetGetMethod();
                this.isGenericTask = true;
            }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }

        public MethodInfo TaskMethod
        {
            get { return this.taskMethod; }
        }

        private InvokeDelegate InvokeDelegate
        {
            get
            {
                this.EnsureIsInitialized();
                return this.invokeDelegate;
            }
        }

        private int InputParameterCount
        {
            get
            {
                this.EnsureIsInitialized();
                return this.inputParameterCount;
            }
        }

        private int OutputParameterCount
        {
            get
            {
                this.EnsureIsInitialized();
                return this.outputParameterCount;
            }
        }

        public object[] AllocateInputs()
        {
            return EmptyArray.Allocate(this.InputParameterCount);
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoServiceObject)));
            }

            if (inputs == null)
            {
                if (this.InputParameterCount > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInputParametersToServiceNull, this.InputParameterCount)));
                }
            }
            else if (inputs.Length != this.InputParameterCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInputParametersToServiceInvalid, this.InputParameterCount, inputs.Length)));
            }

            this.outputs = EmptyArray.Allocate(this.OutputParameterCount);

            AsyncMethodInvoker.StartOperationInvokePerformanceCounters(this.taskMethod.Name);

            IAsyncResult returnValue;
            bool callFailed = true;
            bool callFaulted = false;
            ServiceModelActivity activity = null;

            try
            {
                Activity boundActivity = null;
                AsyncMethodInvoker.CreateActivityInfo(ref activity, ref boundActivity);

                AsyncMethodInvoker.StartOperationInvokeTrace(this.taskMethod.Name);
                                
                using (boundActivity)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        string activityName = SR.GetString(SR.ActivityExecuteMethod, this.taskMethod.DeclaringType.FullName, this.taskMethod.Name);
                        ServiceModelActivity.Start(activity, activityName, ActivityType.ExecuteUserCode);
                    }

                    object taskReturnValue = this.InvokeDelegate(instance, inputs, this.outputs);

                    if (taskReturnValue == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("task");
                    }
                    else if (this.isGenericTask)
                    {
                        returnValue = (IAsyncResult)this.toAsyncMethodInfo.Invoke(null, new object[] { taskReturnValue, callback, state });
                    }
                    else
                    {
                        returnValue = ((Task)taskReturnValue).AsAsyncResult(callback, state);
                    }

                    callFailed = false;
                }
            }
            catch (System.Security.SecurityException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(AuthorizationBehavior.CreateAccessDeniedFaultException());
            }
            catch (Exception e)
            {
                TraceUtility.TraceUserCodeException(e, this.taskMethod);
                if (e is FaultException)
                {
                    callFaulted = true;
                    callFailed = false;
                }

                throw;
            }
            finally
            {
                ServiceModelActivity.Stop(activity);

                // Any exception above means InvokeEnd will not be called, so complete it here.
                if (callFailed || callFaulted)
                {
                    AsyncMethodInvoker.StopOperationInvokeTrace(callFailed, callFaulted, this.TaskMethod.Name);
                    AsyncMethodInvoker.StopOperationInvokePerformanceCounters(callFailed, callFaulted, this.TaskMethod.Name);
                }
            }

            return returnValue;
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            object returnVal;
            bool callFailed = true;
            bool callFaulted = false;
            ServiceModelActivity activity = null;

            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoServiceObject)));
            }

            try
            {
                Activity boundOperation = null;
                AsyncMethodInvoker.GetActivityInfo(ref activity, ref boundOperation);

                using (boundOperation)
                {
                    Task task = result as Task;

                    Fx.Assert(task != null, "InvokeEnd needs to be called with the result returned from InvokeBegin.");
                    if (task.IsFaulted)
                    {
                        Fx.Assert(task.Exception != null, "Task.IsFaulted guarantees non-null exception.");

                        // If FaultException is thrown, we will get 'callFaulted' behavior below.
                        // Any other exception will retain 'callFailed' behavior.
                        throw FxTrace.Exception.AsError<FaultException>(task.Exception);
                    }

                    // Task cancellation without an exception indicates failure but we have no
                    // additional information to provide.  Accessing Task.Result will throw a
                    // TaskCanceledException.   For consistency between void Tasks and Task<T>,
                    // we detect and throw here.
                    if (task.IsCanceled)
                    {
                        throw FxTrace.Exception.AsError(new TaskCanceledException(task));
                    }

                    outputs = this.outputs;
                    if (this.isGenericTask)
                    {
                        returnVal = this.taskTResultGetMethod.Invoke(result, Type.EmptyTypes);
                    }
                    else
                    {                        
                        returnVal = null;
                    }

                    callFailed = false;
                }
            }
            catch (SecurityException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(AuthorizationBehavior.CreateAccessDeniedFaultException());
            }
            catch (FaultException)
            {
                callFaulted = true;
                callFailed = false;
                throw;
            }
            finally
            {
                ServiceModelActivity.Stop(activity);
                AsyncMethodInvoker.StopOperationInvokeTrace(callFailed, callFaulted, this.TaskMethod.Name);
                AsyncMethodInvoker.StopOperationInvokePerformanceCounters(callFailed, callFaulted, this.TaskMethod.Name);
            }

            return returnVal;
        }

        private void EnsureIsInitialized()
        {
            if (this.invokeDelegate == null)
            {
                // Only pass locals byref because InvokerUtil may store temporary results in the byref.
                // If two threads both reference this.count, temporary results may interact.
                int inputParameterCount;
                int outputParameterCount;
                InvokeDelegate invokeDelegate = new InvokerUtil().GenerateInvokeDelegate(this.taskMethod, out inputParameterCount, out outputParameterCount);
                this.inputParameterCount = inputParameterCount;
                this.outputParameterCount = outputParameterCount;
                this.invokeDelegate = invokeDelegate;  // must set this last due to ----
            }
        }
    }
}
