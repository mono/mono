//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;

    [DataContract]
    class FaultCallbackWrapper : CallbackWrapper
    {
        static readonly Type faultCallbackType = typeof(FaultCallback);
        static readonly Type[] faultCallbackParameters = new Type[] { typeof(NativeActivityFaultContext), typeof(Exception), typeof(ActivityInstance) };

        public FaultCallbackWrapper(FaultCallback callback, ActivityInstance owningInstance)
            : base(callback, owningInstance)
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Because we are calling EnsureCallback",
            Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
        [SecuritySafeCritical]
        public void Invoke(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            EnsureCallback(faultCallbackType, faultCallbackParameters);
            FaultCallback faultCallback = (FaultCallback)this.Callback;
            faultCallback(faultContext, propagatedException, propagatedFrom);
        }

        public WorkItem CreateWorkItem(Exception propagatedException, ActivityInstance propagatedFrom, ActivityInstanceReference originalExceptionSource)
        {
            return new FaultWorkItem(this, propagatedException, propagatedFrom, originalExceptionSource);
        }

        [DataContract]
        internal class FaultWorkItem : ActivityExecutionWorkItem
        {
            FaultCallbackWrapper callbackWrapper;
            Exception propagatedException;
            ActivityInstance propagatedFrom;
            ActivityInstanceReference originalExceptionSource;

            public FaultWorkItem(FaultCallbackWrapper callbackWrapper, Exception propagatedException, ActivityInstance propagatedFrom, ActivityInstanceReference originalExceptionSource)
                : base(callbackWrapper.ActivityInstance)
            {
                this.callbackWrapper = callbackWrapper;
                this.propagatedException = propagatedException;
                this.propagatedFrom = propagatedFrom;
                this.originalExceptionSource = originalExceptionSource;
            }

            public override ActivityInstance OriginalExceptionSource
            {
                get
                {
                    return this.originalExceptionSource.ActivityInstance;
                }
            }

            [DataMember(Name = "callbackWrapper")]
            internal FaultCallbackWrapper SerializedCallbackWrapper
            {
                get { return this.callbackWrapper; }
                set { this.callbackWrapper = value; }
            }

            [DataMember(Name = "propagatedException")]
            internal Exception SerializedPropagatedException
            {
                get { return this.propagatedException; }
                set { this.propagatedException = value; }
            }

            [DataMember(Name = "propagatedFrom")]
            internal ActivityInstance SerializedPropagatedFrom
            {
                get { return this.propagatedFrom; }
                set { this.propagatedFrom = value; }
            }

            [DataMember(Name = "originalExceptionSource")]
            internal ActivityInstanceReference SerializedOriginalExceptionSource
            {
                get { return this.originalExceptionSource; }
                set { this.originalExceptionSource = value; }
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteFaultWorkItemIsEnabled())
                {
                    TD.CompleteFaultWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, this.originalExceptionSource.ActivityInstance.Activity.GetType().ToString(), this.originalExceptionSource.ActivityInstance.Activity.DisplayName, this.originalExceptionSource.ActivityInstance.Id, this.propagatedException);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleFaultWorkItemIsEnabled())
                {
                    TD.ScheduleFaultWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, this.originalExceptionSource.ActivityInstance.Activity.GetType().ToString(), this.originalExceptionSource.ActivityInstance.Activity.DisplayName, this.originalExceptionSource.ActivityInstance.Id, this.propagatedException);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartFaultWorkItemIsEnabled())
                {
                    TD.StartFaultWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id, this.originalExceptionSource.ActivityInstance.Activity.GetType().ToString(), this.originalExceptionSource.ActivityInstance.Activity.DisplayName, this.originalExceptionSource.ActivityInstance.Id, this.propagatedException);
                }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                NativeActivityFaultContext faultContext = null;

                try
                {
                    faultContext = new NativeActivityFaultContext(this.ActivityInstance, executor, bookmarkManager, this.propagatedException, this.originalExceptionSource);
                    this.callbackWrapper.Invoke(faultContext, this.propagatedException, this.propagatedFrom);

                    if (!faultContext.IsFaultHandled)
                    {
                        SetExceptionToPropagateWithoutAbort(this.propagatedException);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.ExceptionToPropagate = e;
                }
                finally
                {
                    if (faultContext != null)
                    {
                        faultContext.Dispose();
                    }

                    // Tell the executor to decrement its no persist count persistence of exceptions is disabled.
                    executor.ExitNoPersistForExceptionPropagation();
                }

                return true;
            }
        }
    }
}
