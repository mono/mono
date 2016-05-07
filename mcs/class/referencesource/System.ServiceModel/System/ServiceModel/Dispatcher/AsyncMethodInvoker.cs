//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Runtime;

    class AsyncMethodInvoker : IOperationInvoker
    {
        MethodInfo beginMethod;
        MethodInfo endMethod;
        InvokeBeginDelegate invokeBeginDelegate;
        InvokeEndDelegate invokeEndDelegate;
        int inputParameterCount;
        int outputParameterCount;

        public AsyncMethodInvoker(MethodInfo beginMethod, MethodInfo endMethod)
        {
            if (beginMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("beginMethod"));
            if (endMethod == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endMethod"));

            this.beginMethod = beginMethod;
            this.endMethod = endMethod;
        }

        public MethodInfo BeginMethod
        {
            get { return this.beginMethod; }
        }

        public MethodInfo EndMethod
        {
            get { return this.endMethod; }
        }

        public bool IsSynchronous
        {
            get { return false; }
        }

        public object[] AllocateInputs()
        {
            return EmptyArray.Allocate(this.InputParameterCount);
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        internal static void CreateActivityInfo(ref ServiceModelActivity activity, ref Activity boundActivity)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateAsyncActivity();
                TraceUtility.UpdateAsyncOperationContextWithActivity(activity);
                boundActivity = ServiceModelActivity.BoundOperation(activity, true);
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
                TraceUtility.UpdateAsyncOperationContextWithActivity(activityId);
            }
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            if (instance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoServiceObject)));
            if (inputs == null)
            {
                if (this.InputParameterCount > 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInputParametersToServiceNull, this.InputParameterCount)));
            }
            else if (inputs.Length != this.InputParameterCount)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxInputParametersToServiceInvalid, this.InputParameterCount, inputs.Length)));

            StartOperationInvokePerformanceCounters(this.beginMethod.Name.Substring(ServiceReflector.BeginMethodNamePrefix.Length));

            IAsyncResult returnValue;
            bool callFailed = true;
            bool callFaulted = false;
            ServiceModelActivity activity = null;
            try
            {
                Activity boundActivity = null;
                CreateActivityInfo(ref activity, ref boundActivity);

                StartOperationInvokeTrace(this.beginMethod.Name);

                using (boundActivity)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        string activityName = null;

                        if (this.endMethod == null)
                        {
                            activityName = SR.GetString(SR.ActivityExecuteMethod,
                                this.beginMethod.DeclaringType.FullName, this.beginMethod.Name);
                        }
                        else
                        {
                            activityName = SR.GetString(SR.ActivityExecuteAsyncMethod,
                                this.beginMethod.DeclaringType.FullName, this.beginMethod.Name,
                                this.endMethod.DeclaringType.FullName, this.endMethod.Name);
                        }

                        ServiceModelActivity.Start(activity, activityName, ActivityType.ExecuteUserCode);
                    }

                    returnValue = this.InvokeBeginDelegate(instance, inputs, callback, state);
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
                TraceUtility.TraceUserCodeException(e, this.beginMethod);
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

                // An exception during the InvokeBegin will not call InvokeEnd,
                // so we complete the trace and performance counters here.
                if (callFailed || callFaulted)
                {
                    StopOperationInvokeTrace(callFailed, callFaulted, this.EndMethod.Name);
                    StopOperationInvokePerformanceCounters(callFailed, callFaulted, endMethod.Name.Substring(ServiceReflector.EndMethodNamePrefix.Length));
                }
            }
            return returnValue;
        }

        internal static void GetActivityInfo(ref ServiceModelActivity activity, ref Activity boundOperation)
        {
            if (TraceUtility.MessageFlowTracingOnly)
            {
                if (null != OperationContext.Current)
                {
                    Guid activityId = TraceUtility.GetReceivedActivityId(OperationContext.Current);
                    if (activityId != Guid.Empty)
                    {
                        DiagnosticTraceBase.ActivityId = activityId;
                    }
                }
            }
            else if (DiagnosticUtility.ShouldUseActivity || TraceUtility.ShouldPropagateActivity)
            {
                object activityInfo = TraceUtility.ExtractAsyncOperationContextActivity();
                if (activityInfo != null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        activity = activityInfo as ServiceModelActivity;
                        boundOperation = ServiceModelActivity.BoundOperation(activity, true);
                    }
                    else if (TraceUtility.ShouldPropagateActivity)
                    {
                        if (activityInfo is Guid)
                        {
                            Guid activityId = (Guid)activityInfo;
                            boundOperation = Activity.CreateActivity(activityId);
                        }
                    }
                }
            }
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            object returnVal;

            if (instance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxNoServiceObject)));

            outputs = EmptyArray.Allocate(this.OutputParameterCount);
            bool callFailed = true;
            bool callFaulted = false;
            ServiceModelActivity activity = null;

            try
            {
                Activity boundOperation = null;
                GetActivityInfo(ref activity, ref boundOperation);
                using (boundOperation)
                {
                    returnVal = this.InvokeEndDelegate(instance, outputs, result);
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
                StopOperationInvokeTrace(callFailed, callFaulted, this.endMethod.Name);
                StopOperationInvokePerformanceCounters(callFailed, callFaulted, this.endMethod.Name.Substring(ServiceReflector.EndMethodNamePrefix.Length));
            }

            return returnVal;
        }

        internal static void StartOperationInvokeTrace(string methodName)
        {
            if (TD.OperationInvokedIsEnabled())
            {
                OperationContext context = OperationContext.Current;
                EventTraceActivity eventTraceActivity = null;
                if (context != null && context.IncomingMessage != null)
                {
                    eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(context.IncomingMessage);
                }
                if (TD.OperationInvokedIsEnabled())
                {
                    TD.OperationInvoked(eventTraceActivity, methodName, TraceUtility.GetCallerInfo(OperationContext.Current));
                }
                if (TD.OperationCompletedIsEnabled() || TD.OperationFaultedIsEnabled() || TD.OperationFailedIsEnabled())
                {
                    TraceUtility.UpdateAsyncOperationContextWithStartTime(eventTraceActivity, DateTime.UtcNow.Ticks);
                }
            }
        }

        internal static void StopOperationInvokeTrace(bool callFailed, bool callFaulted, string methodName)
        {
            if (!(TD.OperationCompletedIsEnabled() ||
                TD.OperationFaultedIsEnabled() ||
                TD.OperationFailedIsEnabled()))
            {
                return;
            }

            EventTraceActivity eventTraceActivity;
            long startTime;
            TraceUtility.ExtractAsyncOperationStartTime(out eventTraceActivity, out startTime);
            long duration = TraceUtility.GetUtcBasedDurationForTrace(startTime);

            if (callFailed)
            {
                if (TD.OperationFailedIsEnabled())
                {
                    TD.OperationFailed(eventTraceActivity, methodName, duration);
                }
            }
            else if (callFaulted)
            {
                if (TD.OperationFaultedIsEnabled())
                {
                    TD.OperationFaulted(eventTraceActivity, methodName, duration);
                }
            }
            else
            {
                if (TD.OperationCompletedIsEnabled())
                {
                    TD.OperationCompleted(eventTraceActivity, methodName, duration);
                }
            }
        }

        internal static void StartOperationInvokePerformanceCounters(string methodName)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MethodCalled(methodName);
            }
        }

        internal static void StopOperationInvokePerformanceCounters(bool callFailed, bool callFaulted, string methodName)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                if (callFailed)
                {
                    PerformanceCounters.MethodReturnedError(methodName);
                }
                else if (callFaulted)
                {
                    PerformanceCounters.MethodReturnedFault(methodName);
                }
                else
                {
                    PerformanceCounters.MethodReturnedSuccess(methodName);
                }
            }
        }

        InvokeBeginDelegate InvokeBeginDelegate
        {
            get
            {
                EnsureIsInitialized();
                return invokeBeginDelegate;
            }
        }

        InvokeEndDelegate InvokeEndDelegate
        {
            get
            {
                EnsureIsInitialized();
                return invokeEndDelegate;
            }
        }

        int InputParameterCount
        {
            get
            {
                EnsureIsInitialized();
                return this.inputParameterCount;
            }
        }

        int OutputParameterCount
        {
            get
            {
                EnsureIsInitialized();
                return this.outputParameterCount;
            }
        }

        void EnsureIsInitialized()
        {
            if (this.invokeBeginDelegate == null)
            {
                // Only pass locals byref because InvokerUtil may store temporary results in the byref.
                // If two threads both reference this.count, temporary results may interact.
                int inputParameterCount;
                InvokeBeginDelegate invokeBeginDelegate = new InvokerUtil().GenerateInvokeBeginDelegate(this.beginMethod, out inputParameterCount);
                this.inputParameterCount = inputParameterCount;

                int outputParameterCount;
                InvokeEndDelegate invokeEndDelegate = new InvokerUtil().GenerateInvokeEndDelegate(this.endMethod, out outputParameterCount);
                this.outputParameterCount = outputParameterCount;
                this.invokeEndDelegate = invokeEndDelegate;
                this.invokeBeginDelegate = invokeBeginDelegate;  // must set this last due to ----
            }
        }
    }
}
