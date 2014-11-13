//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Threading;

    class ThreadBehavior
    {
        SendOrPostCallback threadAffinityStartCallback;
        SendOrPostCallback threadAffinityEndCallback;
        static Action<object> cleanThreadCallback;
        readonly SynchronizationContext context;
       
        internal ThreadBehavior(DispatchRuntime dispatch)
        {
            this.context = dispatch.SynchronizationContext;
        }

        SendOrPostCallback ThreadAffinityStartCallbackDelegate
        {
            get
            {
                if (this.threadAffinityStartCallback == null)
                {
                    this.threadAffinityStartCallback = new SendOrPostCallback(this.SynchronizationContextStartCallback);
                }
                return this.threadAffinityStartCallback;
            }
        }
        SendOrPostCallback ThreadAffinityEndCallbackDelegate
        {
            get
            {
                if (this.threadAffinityEndCallback == null)
                {
                    this.threadAffinityEndCallback = new SendOrPostCallback(this.SynchronizationContextEndCallback);
                }
                return this.threadAffinityEndCallback;
            }
        }

        static Action<object> CleanThreadCallbackDelegate
        {
            get
            {
                if (ThreadBehavior.cleanThreadCallback == null)
                {
                    ThreadBehavior.cleanThreadCallback = new Action<object>(ThreadBehavior.CleanThreadCallback);
                }
                return ThreadBehavior.cleanThreadCallback;
            }
        }

        internal void BindThread(ref MessageRpc rpc)
        {
            this.BindCore(ref rpc, true);
        }

        internal void BindEndThread(ref MessageRpc rpc)
        {
            this.BindCore(ref rpc, false);
        }

        void BindCore(ref MessageRpc rpc, bool startOperation)
        {
            SynchronizationContext syncContext = GetSyncContext(rpc.InstanceContext);

            if (syncContext != null)
            {
                IResumeMessageRpc resume = rpc.Pause();
                if (startOperation)
                {
                    syncContext.OperationStarted();
                    syncContext.Post(this.ThreadAffinityStartCallbackDelegate, resume);
                }
                else
                {
                    syncContext.Post(this.ThreadAffinityEndCallbackDelegate, resume);
                }
            }
            else if (rpc.SwitchedThreads)
            {
                IResumeMessageRpc resume = rpc.Pause();
                ActionItem.Schedule(ThreadBehavior.CleanThreadCallbackDelegate, resume);
            }
        }

        SynchronizationContext GetSyncContext(InstanceContext instanceContext)
        {
            Fx.Assert(instanceContext != null, "instanceContext is null !");
            SynchronizationContext syncContext = instanceContext.SynchronizationContext ?? this.context;
            return syncContext;
        }

        void SynchronizationContextStartCallback(object state)
        {
            ResumeProcessing((IResumeMessageRpc)state);
        }
        void SynchronizationContextEndCallback(object state)
        {
            IResumeMessageRpc resume = (IResumeMessageRpc)state;

            ResumeProcessing(resume);

            SynchronizationContext syncContext = GetSyncContext(resume.GetMessageInstanceContext());
            Fx.Assert(syncContext != null, "syncContext is null !?");
            syncContext.OperationCompleted();
        }
        void ResumeProcessing(IResumeMessageRpc resume)
        {
            bool alreadyResumedNoLock;
            resume.Resume(out alreadyResumedNoLock);

            if (alreadyResumedNoLock)
            {
                string text = SR.GetString(SR.SFxMultipleCallbackFromSynchronizationContext, context.GetType().ToString());
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(text));
            }
        }

        static void CleanThreadCallback(object state)
        {
            bool alreadyResumedNoLock;
            ((IResumeMessageRpc)state).Resume(out alreadyResumedNoLock);

            if (alreadyResumedNoLock)
            {
                Fx.Assert("IOThreadScheduler called back twice");
            }
        }

        internal static SynchronizationContext GetCurrentSynchronizationContext()
        {
            if (AspNetEnvironment.IsApplicationDomainHosted())
            {
                return null;
            }
            return SynchronizationContext.Current;
        }
    }
}
