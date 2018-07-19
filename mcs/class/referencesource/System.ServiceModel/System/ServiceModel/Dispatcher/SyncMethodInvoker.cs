//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Globalization;
    using System.Threading;
    using System.Collections;
    using System.Diagnostics;
    using System.Security;
    using System.Runtime;

    class SyncMethodInvoker : IOperationInvoker
    {
        Type type;
        string methodName;
        MethodInfo method;
        InvokeDelegate invokeDelegate;
        int inputParameterCount;
        int outputParameterCount;

        public SyncMethodInvoker(MethodInfo method)
        {
            if (method == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("method"));

            this.method = method;
        }

        public SyncMethodInvoker(Type type, string methodName)
        {
            if (type == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("type"));

            if (methodName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("methodName"));

            this.type = type;
            this.methodName = methodName;
        }

        public bool IsSynchronous
        {
            get { return true; }
        }

        public MethodInfo Method
        {
            get
            {
                if (method == null)
                    method = type.GetMethod(methodName);
                return method;
            }
        }

        public string MethodName
        {
            get
            {
                if (methodName == null)
                    methodName = method.Name;
                return methodName;
            }
        }

        public object[] AllocateInputs()
        {
            EnsureIsInitialized();

            return EmptyArray.Allocate(this.inputParameterCount);
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            EnsureIsInitialized();

            if (instance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoServiceObject)));
            if (inputs == null)
            {
                if (this.inputParameterCount > 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInputParametersToServiceNull, this.inputParameterCount)));
            }
            else if (inputs.Length != this.inputParameterCount)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInputParametersToServiceInvalid, this.inputParameterCount, inputs.Length)));

            outputs = EmptyArray.Allocate(this.outputParameterCount);

            long startCounter = 0;
            long stopCounter = 0;
            long beginOperation = 0;
            bool callSucceeded = false;
            bool callFaulted = false;

            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MethodCalled(this.MethodName);
                try
                {
                    if (System.ServiceModel.Channels.UnsafeNativeMethods.QueryPerformanceCounter(out startCounter) == 0)
                    {
                        startCounter = -1;
                    }
                }
                catch (SecurityException securityException)
                {
                    DiagnosticUtility.TraceHandledException(securityException, TraceEventType.Warning);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustPerformanceCountersNotEnabled), securityException));
                }
            }

            EventTraceActivity eventTraceActivity = null;
            if (TD.OperationCompletedIsEnabled() ||
                    TD.OperationFaultedIsEnabled() ||
                    TD.OperationFailedIsEnabled())
            {
                beginOperation = DateTime.UtcNow.Ticks;
                OperationContext context = OperationContext.Current;
                if (context != null && context.IncomingMessage != null)
                {
                    eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(context.IncomingMessage);
                }
            }

            object returnValue;
            try
            {
                ServiceModelActivity activity = null;
                IDisposable boundActivity = null;
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    activity = ServiceModelActivity.CreateBoundedActivity(true);
                    boundActivity = activity;
                }
                else if (TraceUtility.MessageFlowTracingOnly)
                {
                    Guid activityId = TraceUtility.GetReceivedActivityId(OperationContext.Current);
                    if (activityId != Guid.Empty)
                    {
                        DiagnosticTraceBase.ActivityId = activityId;
                    }
                }
                else if (TraceUtility.ShouldPropagateActivity)
                {
                    //Message flow tracing only scenarios use a light-weight ActivityID management logic
                    Guid activityId = ActivityIdHeader.ExtractActivityId(OperationContext.Current.IncomingMessage);
                    if (activityId != Guid.Empty)
                    {
                        boundActivity = Activity.CreateActivity(activityId);
                    }
                }

                using (boundActivity)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityExecuteMethod, this.method.DeclaringType.FullName, this.method.Name), ActivityType.ExecuteUserCode);
                    }
                    if (TD.OperationInvokedIsEnabled())
                    {
                        TD.OperationInvoked(eventTraceActivity, this.MethodName, TraceUtility.GetCallerInfo(OperationContext.Current));
                    }
                    returnValue = this.invokeDelegate(instance, inputs, outputs);
                    callSucceeded = true;
                }
            }
            catch (System.ServiceModel.FaultException)
            {
                callFaulted = true;
                throw;
            }
            catch (System.Security.SecurityException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(AuthorizationBehavior.CreateAccessDeniedFaultException());
            }
            finally
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    long elapsedTime = 0;
                    if (startCounter >= 0 && System.ServiceModel.Channels.UnsafeNativeMethods.QueryPerformanceCounter(out stopCounter) != 0)
                    {
                        elapsedTime = stopCounter - startCounter;
                    }

                    if (callSucceeded) // call succeeded
                    {
                        PerformanceCounters.MethodReturnedSuccess(this.MethodName, elapsedTime);
                    }
                    else if (callFaulted) // call faulted
                    {
                        PerformanceCounters.MethodReturnedFault(this.MethodName, elapsedTime);
                    }
                    else // call failed
                    {
                        PerformanceCounters.MethodReturnedError(this.MethodName, elapsedTime);
                    }
                }

                if (beginOperation != 0)
                {
                    if (callSucceeded)
                    {
                        if (TD.OperationCompletedIsEnabled())
                        {
                            TD.OperationCompleted(eventTraceActivity, this.methodName,
                                TraceUtility.GetUtcBasedDurationForTrace(beginOperation));
                        }
                    }
                    else if (callFaulted)
                    {
                        if (TD.OperationFaultedIsEnabled())
                        {
                            TD.OperationFaulted(eventTraceActivity, this.methodName,
                                TraceUtility.GetUtcBasedDurationForTrace(beginOperation));
                        }
                    }
                    else
                    {
                        if (TD.OperationFailedIsEnabled())
                        {
                            TD.OperationFailed(eventTraceActivity, this.methodName,
                                TraceUtility.GetUtcBasedDurationForTrace(beginOperation));
                        }
                    }
                }
            }

            return returnValue;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        void EnsureIsInitialized()
        {
            if (this.invokeDelegate == null)
            {
                EnsureIsInitializedCore();
            }
        }

        void EnsureIsInitializedCore()
        {
            // Only pass locals byref because InvokerUtil may store temporary results in the byref.
            // If two threads both reference this.count, temporary results may interact.
            int inputParameterCount;
            int outputParameterCount;
            InvokeDelegate invokeDelegate = new InvokerUtil().GenerateInvokeDelegate(this.Method, out inputParameterCount, out outputParameterCount);
            this.outputParameterCount = outputParameterCount;
            this.inputParameterCount = inputParameterCount;
            this.invokeDelegate = invokeDelegate;  // must set this last due to ----
        }
    }
}
