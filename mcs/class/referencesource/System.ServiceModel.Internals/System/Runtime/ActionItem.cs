//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime
{
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Threading;

    abstract class ActionItem
    {
        [Fx.Tag.SecurityNote(Critical = "Stores the security context, used later in binding back into")]
        [SecurityCritical]
        SecurityContext context;
        bool isScheduled;

        bool lowPriority;

        protected ActionItem()
        {
        }

        public bool LowPriority
        {
            get
            {
                return this.lowPriority;
            }
            protected set
            {
                this.lowPriority = value;
            }
        }

        public static void Schedule(Action<object> callback, object state)
        {
            Schedule(callback, state, false);
        }
        [Fx.Tag.SecurityNote(Critical = "Calls into critical method ScheduleCallback",
            Safe = "Schedule invoke of the given delegate under the current context")]
        [SecuritySafeCritical]
        public static void Schedule(Action<object> callback, object state, bool lowPriority)
        {
            Fx.Assert(callback != null, "A null callback was passed for Schedule!");

            if (PartialTrustHelpers.ShouldFlowSecurityContext ||
                WaitCallbackActionItem.ShouldUseActivity ||
                Fx.Trace.IsEnd2EndActivityTracingEnabled)
            {
                new DefaultActionItem(callback, state, lowPriority).Schedule();
            }
            else
            {
                ScheduleCallback(callback, state, lowPriority);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Called after applying the user context on the stack or (potentially) " +
            "without any user context on the stack")]
        [SecurityCritical]
        protected abstract void Invoke();

        [Fx.Tag.SecurityNote(Critical = "Access critical field context and critical property " +
            "CallbackHelper.InvokeWithContextCallback, calls into critical method " +
            "PartialTrustHelpers.CaptureSecurityContextNoIdentityFlow, calls into critical method ScheduleCallback; " +
            "since the invoked method and the capturing of the security contex are de-coupled, can't " +
            "be treated as safe")]
        [SecurityCritical]
        protected void Schedule()
        {
            if (isScheduled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.ActionItemIsAlreadyScheduled));
            }

            this.isScheduled = true;
            if (PartialTrustHelpers.ShouldFlowSecurityContext)
            {
                this.context = PartialTrustHelpers.CaptureSecurityContextNoIdentityFlow();
            }
            if (this.context != null)
            {
                ScheduleCallback(CallbackHelper.InvokeWithContextCallback);
            }
            else
            {
                ScheduleCallback(CallbackHelper.InvokeWithoutContextCallback);
            }
        }
        [Fx.Tag.SecurityNote(Critical = "Access critical field context and critical property " +
            "CallbackHelper.InvokeWithContextCallback, calls into critical method ScheduleCallback; " +
            "since nothing is known about the given context, can't be treated as safe")]
        [SecurityCritical]
        protected void ScheduleWithContext(SecurityContext context)
        {
            if (context == null)
            {
                throw Fx.Exception.ArgumentNull("context");
            }
            if (isScheduled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.ActionItemIsAlreadyScheduled));
            }

            this.isScheduled = true;
            this.context = context.CreateCopy();
            ScheduleCallback(CallbackHelper.InvokeWithContextCallback);
        }
        [Fx.Tag.SecurityNote(Critical = "Access critical property CallbackHelper.InvokeWithoutContextCallback, " +
            "Calls into critical method ScheduleCallback; not bound to a security context")]
        [SecurityCritical]
        protected void ScheduleWithoutContext()
        {
            if (isScheduled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(InternalSR.ActionItemIsAlreadyScheduled));
            }

            this.isScheduled = true;
            ScheduleCallback(CallbackHelper.InvokeWithoutContextCallback);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into critical methods IOThreadScheduler.ScheduleCallbackNoFlow, " +
            "IOThreadScheduler.ScheduleCallbackLowPriNoFlow")]
        [SecurityCritical]
        static void ScheduleCallback(Action<object> callback, object state, bool lowPriority)
        {
            Fx.Assert(callback != null, "Cannot schedule a null callback");
            if (lowPriority)
            {
                IOThreadScheduler.ScheduleCallbackLowPriNoFlow(callback, state);
            }
            else
            {
                IOThreadScheduler.ScheduleCallbackNoFlow(callback, state);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Extract the security context stored and reset the critical field")]
        [SecurityCritical]
        SecurityContext ExtractContext()
        {
            Fx.Assert(this.context != null, "Cannot bind to a null context; context should have been set by now");
            Fx.Assert(this.isScheduled, "Context is extracted only while the object is scheduled");
            SecurityContext result = this.context;
            this.context = null;
            return result;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls into critical static method ScheduleCallback")]
        [SecurityCritical]
        void ScheduleCallback(Action<object> callback)
        {
            ScheduleCallback(callback, this, this.lowPriority);
        }

        [SecurityCritical]
        static class CallbackHelper
        {
            [Fx.Tag.SecurityNote(Critical = "Stores a delegate to a critical method")]
            static Action<object> invokeWithContextCallback;
            [Fx.Tag.SecurityNote(Critical = "Stores a delegate to a critical method")]
            static Action<object> invokeWithoutContextCallback;
            [Fx.Tag.SecurityNote(Critical = "Stores a delegate to a critical method")]
            static ContextCallback onContextAppliedCallback;
            [Fx.Tag.SecurityNote(Critical = "Provides access to a critical field; Initialize it with " +
                "a delegate to a critical method")]
            public static Action<object> InvokeWithContextCallback
            {
                get
                {
                    if (invokeWithContextCallback == null)
                    {
                        invokeWithContextCallback = new Action<object>(InvokeWithContext);
                    }
                    return invokeWithContextCallback;
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Provides access to a critical field; Initialize it with " +
                "a delegate to a critical method")]
            public static Action<object> InvokeWithoutContextCallback
            {
                get
                {
                    if (invokeWithoutContextCallback == null)
                    {
                        invokeWithoutContextCallback = new Action<object>(InvokeWithoutContext);
                    }
                    return invokeWithoutContextCallback;
                }
            }
            [Fx.Tag.SecurityNote(Critical = "Provides access to a critical field; Initialize it with " +
                "a delegate to a critical method")]
            public static ContextCallback OnContextAppliedCallback
            {
                get
                {
                    if (onContextAppliedCallback == null)
                    {
                        onContextAppliedCallback = new ContextCallback(OnContextApplied);
                    }
                    return onContextAppliedCallback;
                }
            }
            [Fx.Tag.SecurityNote(Critical = "Called by the scheduler without any user context on the stack")]
            static void InvokeWithContext(object state)
            {
                SecurityContext context = ((ActionItem)state).ExtractContext();
                SecurityContext.Run(context, OnContextAppliedCallback, state);
            }

            [Fx.Tag.SecurityNote(Critical = "Called by the scheduler without any user context on the stack")]
            static void InvokeWithoutContext(object state)
            {
                ((ActionItem)state).Invoke();
                ((ActionItem)state).isScheduled = false;
            }
            [Fx.Tag.SecurityNote(Critical = "Called after applying the user context on the stack")]
            static void OnContextApplied(object o)
            {
                ((ActionItem)o).Invoke();
                ((ActionItem)o).isScheduled = false;
            }
        }

        class DefaultActionItem : ActionItem
        {
            [Fx.Tag.SecurityNote(Critical = "Stores a delegate that will be called later, at a particular context")]
            [SecurityCritical]
            Action<object> callback;
            [Fx.Tag.SecurityNote(Critical = "Stores an object that will be passed to the delegate that will be " +
                "called later, at a particular context")]
            [SecurityCritical]
            object state;

            bool flowLegacyActivityId;
            Guid activityId;
            EventTraceActivity eventTraceActivity;

            [Fx.Tag.SecurityNote(Critical = "Access critical fields callback and state",
                Safe = "Doesn't leak information or resources")]
            [SecuritySafeCritical]
            public DefaultActionItem(Action<object> callback, object state, bool isLowPriority)
            {
                Fx.Assert(callback != null, "Shouldn't instantiate an object to wrap a null callback");
                base.LowPriority = isLowPriority;
                this.callback = callback;
                this.state = state;
                if (WaitCallbackActionItem.ShouldUseActivity)
                {
                    this.flowLegacyActivityId = true;
                    this.activityId = EtwDiagnosticTrace.ActivityId;
                }
                if (Fx.Trace.IsEnd2EndActivityTracingEnabled)
                {
                    this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                    if (TraceCore.ActionItemScheduledIsEnabled(Fx.Trace))
                    {
                        TraceCore.ActionItemScheduled(Fx.Trace, this.eventTraceActivity);
                    }
                }

            }

            [Fx.Tag.SecurityNote(Critical = "Implements a the critical abstract ActionItem.Invoke method, " +
                "Access critical fields callback and state")]
            [SecurityCritical]
            protected override void Invoke()
            {
                if (this.flowLegacyActivityId || Fx.Trace.IsEnd2EndActivityTracingEnabled)
                {
                    TraceAndInvoke();
                }
                else
                {
                    this.callback(this.state);
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Implements a the critical abstract Trace method, " +
                    "Access critical fields callback and state")]
            [SecurityCritical]
            void TraceAndInvoke()
            {
                //
                if (this.flowLegacyActivityId)
                {
                    Guid currentActivityId = EtwDiagnosticTrace.ActivityId;
                    try
                    {
                        EtwDiagnosticTrace.ActivityId = this.activityId;
                        this.callback(this.state);
                    }
                    finally
                    {
                        EtwDiagnosticTrace.ActivityId = currentActivityId;
                    }
                }
                else
                {
                    Guid previous = Guid.Empty;
                    bool restoreActivityId = false;
                    try
                    {
                        if (this.eventTraceActivity != null)
                        {
                            previous = Trace.CorrelationManager.ActivityId;
                            restoreActivityId = true;
                            Trace.CorrelationManager.ActivityId = this.eventTraceActivity.ActivityId;
                            if (TraceCore.ActionItemCallbackInvokedIsEnabled(Fx.Trace))
                            {
                                TraceCore.ActionItemCallbackInvoked(Fx.Trace, this.eventTraceActivity);
                            }
                        }
                        this.callback(this.state);
                    }
                    finally
                    {
                        if (restoreActivityId)
                        {
                            Trace.CorrelationManager.ActivityId = previous;
                        }
                    }
                }
            }
        }
    }
}
