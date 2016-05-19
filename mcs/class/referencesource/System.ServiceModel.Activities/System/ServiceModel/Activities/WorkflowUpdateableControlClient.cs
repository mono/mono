//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Transactions;
    using SR2 = System.ServiceModel.Activities.SR;

    public class WorkflowUpdateableControlClient : ClientBase<IWorkflowUpdateableInstanceManagement>
    {
        BeginOperationDelegate onBeginAbandonDelegate;
        EndOperationDelegate onEndAbandonDelegate;
        SendOrPostCallback onAbandonCompleteDelegate;

        BeginOperationDelegate onBeginCancelDelegate;
        EndOperationDelegate onEndCancelDelegate;
        SendOrPostCallback onCancelCompleteDelegate;

        BeginOperationDelegate onBeginRunDelegate;
        EndOperationDelegate onEndRunDelegate;
        SendOrPostCallback onRunCompleteDelegate;

        BeginOperationDelegate onBeginSuspendDelegate;
        EndOperationDelegate onEndSuspendDelegate;
        SendOrPostCallback onSuspendCompleteDelegate;

        BeginOperationDelegate onBeginUnsuspendDelegate;
        EndOperationDelegate onEndUnsuspendDelegate;
        SendOrPostCallback onUnsuspendCompleteDelegate;

        BeginOperationDelegate onBeginTerminateDelegate;
        EndOperationDelegate onEndTerminateDelegate;
        SendOrPostCallback onTerminateCompleteDelegate;

        BeginOperationDelegate onBeginUpdateDelegate;
        EndOperationDelegate onEndUpdateDelegate;
        SendOrPostCallback onUpdateCompleteDelegate;

        bool checkedBinding;
        bool supportsTransactedInvoke;

        public WorkflowUpdateableControlClient()
            : base()
        {

        }

        public WorkflowUpdateableControlClient(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {

        }

        public WorkflowUpdateableControlClient(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {

        }

        public WorkflowUpdateableControlClient(string endpointConfigurationName, string remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {

        }

        public WorkflowUpdateableControlClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {

        }

        public WorkflowUpdateableControlClient(WorkflowControlEndpoint workflowEndpoint)
            : base(workflowEndpoint.Binding, workflowEndpoint.Address)
        {

        }

        public event EventHandler<AsyncCompletedEventArgs> AbandonCompleted;
        public event EventHandler<AsyncCompletedEventArgs> CancelCompleted;
        public event EventHandler<AsyncCompletedEventArgs> RunCompleted;
        public event EventHandler<AsyncCompletedEventArgs> SuspendCompleted;
        public event EventHandler<AsyncCompletedEventArgs> UnsuspendCompleted;
        public event EventHandler<AsyncCompletedEventArgs> TerminateCompleted;
        public event EventHandler<AsyncCompletedEventArgs> UpdateCompleted;

        bool SupportsTransactedInvoke
        {
            get
            {
                if (!this.checkedBinding)
                {
                    foreach (BindingElement bindingElement in base.Endpoint.Binding.CreateBindingElements())
                    {
                        TransactionFlowBindingElement transactionFlowElement = bindingElement as TransactionFlowBindingElement;

                        if (transactionFlowElement != null)
                        {
                            this.supportsTransactedInvoke = true;
                            break;
                        }
                    }
                    this.checkedBinding = true;
                }
                return this.supportsTransactedInvoke;
            }
        }

        bool IsTransactedInvoke
        {
            get
            {
                return this.SupportsTransactedInvoke && Transaction.Current != null;
            }
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Abandon")]
        public void Abandon(Guid instanceId)
        {
            Abandon(instanceId, null);
        }

        [Fx.Tag.InheritThrows(From = "Abandon")]
        public void Abandon(Guid instanceId, string reason)
        {
            base.Channel.Abandon(instanceId, reason);
        }

        [Fx.Tag.InheritThrows(From = "Abandon")]
        public void AbandonAsync(Guid instanceId)
        {
            AbandonAsync(instanceId, null, null);
        }

        [Fx.Tag.InheritThrows(From = "Abandon")]
        public void AbandonAsync(Guid instanceId, object userState)
        {
            AbandonAsync(instanceId, null, userState);
        }

        [Fx.Tag.InheritThrows(From = "Abandon")]
        public void AbandonAsync(Guid instanceId, string reason)
        {
            AbandonAsync(instanceId, reason, null);
        }

        [Fx.Tag.InheritThrows(From = "Abandon")]
        public void AbandonAsync(Guid instanceId, string reason, object userState)
        {
            if (this.onBeginAbandonDelegate == null)
            {
                this.onBeginAbandonDelegate = new BeginOperationDelegate(OnBeginAbandon);
                this.onEndAbandonDelegate = new EndOperationDelegate(OnEndAbandon);
                this.onAbandonCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnAbandonCompleted));
            }

            base.InvokeAsync(this.onBeginAbandonDelegate, new object[] { instanceId, reason },
                this.onEndAbandonDelegate, this.onAbandonCompleteDelegate, userState);
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Cancel")]
        public void Cancel(Guid instanceId)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedCancel(instanceId);
            }
            else
            {
                base.Channel.Cancel((instanceId));
            }
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Cancel")]
        public void CancelAsync(Guid instanceId)
        {
            this.CancelAsync(instanceId, null);
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Cancel")]
        public void CancelAsync(Guid instanceId, object userState)
        {
            if (this.onBeginCancelDelegate == null)
            {
                this.onBeginCancelDelegate = new BeginOperationDelegate(OnBeginCancel);
                this.onEndCancelDelegate = new EndOperationDelegate(OnEndCancel);
                this.onCancelCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnCancelCompleted));
            }

            base.InvokeAsync(this.onBeginCancelDelegate, new object[] { instanceId },
                this.onEndCancelDelegate, this.onCancelCompleteDelegate, userState);
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Run")]
        public void Run(Guid instanceId)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedRun((instanceId));
            }
            else
            {
                base.Channel.Run((instanceId));
            }
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Run")]
        public void RunAsync(Guid instanceId)
        {
            this.RunAsync(instanceId, null);
        }
        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Run")]
        public void RunAsync(Guid instanceId, object userState)
        {
            if (this.onBeginRunDelegate == null)
            {
                this.onBeginRunDelegate = new BeginOperationDelegate(OnBeginRun);
                this.onEndRunDelegate = new EndOperationDelegate(OnEndRun);
                this.onRunCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnRunCompleted));
            }

            base.InvokeAsync(this.onBeginRunDelegate, new object[] { instanceId },
                this.onEndRunDelegate, this.onRunCompleteDelegate, userState);
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Suspend")]
        public void Suspend(Guid instanceId)
        {
            Suspend(instanceId, SR2.DefaultSuspendReason);
        }
        [Fx.Tag.InheritThrows(From = "Suspend")]
        public void Suspend(Guid instanceId, string reason)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedSuspend(instanceId, reason);
            }
            else
            {
                base.Channel.Suspend(instanceId, reason);
            }
        }
        [Fx.Tag.InheritThrows(From = "Suspend")]
        public void SuspendAsync(Guid instanceId)
        {
            this.SuspendAsync(instanceId, SR2.DefaultSuspendReason);
        }
        [Fx.Tag.InheritThrows(From = "Suspend")]
        public void SuspendAsync(Guid instanceId, string reason)
        {
            this.SuspendAsync(instanceId, reason, null);
        }
        [Fx.Tag.InheritThrows(From = "Suspend")]
        public void SuspendAsync(Guid instanceId, object userState)
        {
            this.SuspendAsync(instanceId, SR2.DefaultSuspendReason, userState);
        }
        [Fx.Tag.InheritThrows(From = "Suspend")]
        public void SuspendAsync(Guid instanceId, string reason, object userState)
        {
            if (this.onBeginSuspendDelegate == null)
            {
                this.onEndSuspendDelegate = new EndOperationDelegate(OnEndSuspend);
                this.onSuspendCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnSuspendCompleted));
                this.onBeginSuspendDelegate = new BeginOperationDelegate(OnBeginSuspend);
            }

            base.InvokeAsync(this.onBeginSuspendDelegate, new object[] { instanceId, reason },
                this.onEndSuspendDelegate, this.onSuspendCompleteDelegate, userState);

        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Unsuspend")]
        public void Unsuspend(Guid instanceId)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedUnsuspend(instanceId);
            }
            else
            {
                base.Channel.Unsuspend(instanceId);
            }
        }
        [Fx.Tag.InheritThrows(From = "Unsuspend")]
        public void UnsuspendAsync(Guid instanceId)
        {
            this.UnsuspendAsync(instanceId, null);
        }
        [Fx.Tag.InheritThrows(From = "Unsuspend")]
        public void UnsuspendAsync(Guid instanceId, object userState)
        {
            if (this.onBeginUnsuspendDelegate == null)
            {
                this.onBeginUnsuspendDelegate = new BeginOperationDelegate(OnBeginUnsuspend);
                this.onEndUnsuspendDelegate = new EndOperationDelegate(OnEndUnsuspend);
                this.onUnsuspendCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnUnsuspendCompleted));
            }

            base.InvokeAsync(this.onBeginUnsuspendDelegate, new object[] { instanceId },
                this.onEndUnsuspendDelegate, this.onUnsuspendCompleteDelegate, userState);

        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Terminate")]
        public void Terminate(Guid instanceId)
        {
            Terminate(instanceId, SR2.DefaultTerminationReason);
        }
        [Fx.Tag.InheritThrows(From = "Terminate")]
        public void Terminate(Guid instanceId, string reason)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedTerminate(instanceId, reason);
            }
            else
            {
                base.Channel.Terminate(instanceId, reason);
            }
        }
        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Terminate")]
        public void TerminateAsync(Guid instanceId)
        {
            this.TerminateAsync(instanceId, SR2.DefaultTerminationReason);
        }
        [Fx.Tag.InheritThrows(From = "Terminate")]
        public void TerminateAsync(Guid instanceId, string reason)
        {
            this.TerminateAsync(instanceId, reason, null);
        }
        [Fx.Tag.InheritThrows(From = "Terminate")]
        public void TerminateAsync(Guid instanceId, object userState)
        {
            this.TerminateAsync(instanceId, SR2.DefaultTerminationReason, userState);
        }
        [Fx.Tag.InheritThrows(From = "Terminate")]
        public void TerminateAsync(Guid instanceId, string reason, object userState)
        {
            if (this.onBeginTerminateDelegate == null)
            {
                this.onEndTerminateDelegate = new EndOperationDelegate(OnEndTerminate);
                this.onTerminateCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnTerminateCompleted));
                this.onBeginTerminateDelegate = new BeginOperationDelegate(OnBeginTerminate);
            }

            base.InvokeAsync(this.onBeginTerminateDelegate, new object[] { instanceId, reason },
                this.onEndTerminateDelegate, this.onTerminateCompleteDelegate, userState);
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Update")]
        public void Update(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity)
        {
            if (this.IsTransactedInvoke)
            {
                base.Channel.TransactedUpdate(instanceId, updatedDefinitionIdentity);
            }
            else
            {
                base.Channel.Update(instanceId, updatedDefinitionIdentity);
            }
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowUpdateableInstanceManagement), From = "Update")]
        public void UpdateAsync(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity)
        {
            this.UpdateAsync(instanceId, updatedDefinitionIdentity, null);
        }

        [Fx.Tag.InheritThrows(FromDeclaringType = typeof(IWorkflowInstanceManagement), From = "Update")]
        public void UpdateAsync(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity, object userState)
        {
            if (this.onBeginUpdateDelegate == null)
            {
                this.onBeginUpdateDelegate = new BeginOperationDelegate(OnBeginUpdate);
                this.onEndUpdateDelegate = new EndOperationDelegate(OnEndUpdate);
                this.onUpdateCompleteDelegate = Fx.ThunkCallback(new SendOrPostCallback(OnUpdateCompleted));
            }

            base.InvokeAsync(this.onBeginUpdateDelegate, new object[] { instanceId, updatedDefinitionIdentity },
                this.onEndUpdateDelegate, this.onUpdateCompleteDelegate, userState);
        }

        [Fx.Tag.InheritThrows(From = "Abandon")]
        public IAsyncResult BeginAbandon(Guid instanceId, AsyncCallback callback, object state)
        {
            return BeginAbandon(instanceId, null, callback, state);
        }
        [Fx.Tag.InheritThrows(From = "Abandon")]
        public IAsyncResult BeginAbandon(Guid instanceId, string reason, AsyncCallback callback, object state)
        {
            return base.Channel.BeginAbandon(instanceId, reason, callback, state);
        }
        [Fx.Tag.InheritThrows(From = "Abandon")]
        public void EndAbandon(IAsyncResult result)
        {
            base.Channel.EndAbandon(result);
        }

        [Fx.Tag.InheritThrows(From = "Cancel")]
        public IAsyncResult BeginCancel(Guid instanceId, AsyncCallback callback, object state)
        {
            return new CancelAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Cancel")]
        public void EndCancel(IAsyncResult result)
        {
            CancelAsyncResult.End(result);
        }

        [Fx.Tag.InheritThrows(From = "Run")]
        public IAsyncResult BeginRun(Guid instanceId, AsyncCallback callback, object state)
        {
            return new RunAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Run")]
        public void EndRun(IAsyncResult result)
        {
            RunAsyncResult.End(result);
        }

        [Fx.Tag.InheritThrows(From = "Suspend")]
        public IAsyncResult BeginSuspend(Guid instanceId, AsyncCallback callback, object state)
        {
            return BeginSuspend(instanceId, SR2.DefaultSuspendReason, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Suspend")]
        public IAsyncResult BeginSuspend(Guid instanceId, string reason, AsyncCallback callback, object state)
        {
            return new SuspendAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, reason, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Suspend")]
        public void EndSuspend(IAsyncResult result)
        {
            SuspendAsyncResult.End(result);
        }

        [Fx.Tag.InheritThrows(From = "Unsuspend")]
        public IAsyncResult BeginUnsuspend(Guid instanceId, AsyncCallback callback, object state)
        {
            return new UnsuspendAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Unsuspend")]
        public void EndUnsuspend(IAsyncResult result)
        {
            UnsuspendAsyncResult.End(result);
        }

        [Fx.Tag.InheritThrows(From = "Terminate")]
        public IAsyncResult BeginTerminate(Guid instanceId, AsyncCallback callback, object state)
        {
            return BeginTerminate(instanceId, SR2.DefaultTerminationReason, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Terminate")]
        public IAsyncResult BeginTerminate(Guid instanceId, string reason, AsyncCallback callback, object state)
        {
            return new TerminateAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, reason, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Terminate")]
        public void EndTerminate(IAsyncResult result)
        {
            TerminateAsyncResult.End(result);
        }

        [Fx.Tag.InheritThrows(From = "Update")]
        public IAsyncResult BeginUpdate(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity, AsyncCallback callback, object state)
        {
            return new UpdateAsyncResult(base.Channel, this.IsTransactedInvoke, instanceId, updatedDefinitionIdentity, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Update")]
        public void EndUpdate(IAsyncResult result)
        {
            UpdateAsyncResult.End(result);
        }

        void OnAbandonCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> abortCompleted = this.AbandonCompleted;

            if (abortCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                abortCompleted(this,
                    new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        IAsyncResult OnBeginAbandon(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginAbandon((Guid)inputs[0], (string)inputs[1], callback, state);
        }

        object[] OnEndAbandon(IAsyncResult result)
        {
            this.EndAbandon(result);
            return null;
        }

        void OnCancelCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> cancelCompleted = this.CancelCompleted;

            if (cancelCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                cancelCompleted(this,
                    new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        IAsyncResult OnBeginCancel(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginCancel((Guid)inputs[0], callback, state);
        }

        object[] OnEndCancel(IAsyncResult result)
        {
            this.EndCancel(result);
            return null;
        }

        void OnRunCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> runCompleted = this.RunCompleted;

            if (runCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                runCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        IAsyncResult OnBeginRun(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginRun((Guid)inputs[0], callback, state);
        }
        object[] OnEndRun(IAsyncResult result)
        {
            this.EndRun(result);
            return null;
        }

        void OnSuspendCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> suspendCompleted = this.SuspendCompleted;

            if (suspendCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                suspendCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        IAsyncResult OnBeginSuspend(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginSuspend((Guid)inputs[0], (string)inputs[1], callback, state);
        }
        object[] OnEndSuspend(IAsyncResult result)
        {
            this.EndSuspend(result);
            return null;
        }

        void OnUnsuspendCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> unsuspendCompleted = this.UnsuspendCompleted;

            if (unsuspendCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                unsuspendCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
        IAsyncResult OnBeginUnsuspend(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginUnsuspend((Guid)inputs[0], callback, state);
        }
        object[] OnEndUnsuspend(IAsyncResult result)
        {
            this.EndUnsuspend(result);
            return null;
        }

        void OnTerminateCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> terminateCompleted = this.TerminateCompleted;

            if (terminateCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                terminateCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        IAsyncResult OnBeginTerminate(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginTerminate((Guid)inputs[0], (string)inputs[1], callback, state);
        }

        object[] OnEndTerminate(IAsyncResult result)
        {
            this.EndTerminate(result);
            return null;
        }

        void OnUpdateCompleted(object state)
        {
            EventHandler<AsyncCompletedEventArgs> updateCompleted = this.UpdateCompleted;

            if (updateCompleted != null)
            {
                InvokeAsyncCompletedEventArgs e = (InvokeAsyncCompletedEventArgs)state;
                updateCompleted(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        IAsyncResult OnBeginUpdate(object[] inputs, AsyncCallback callback, object state)
        {
            return this.BeginUpdate((Guid)inputs[0], (WorkflowIdentity)inputs[1], callback, state);
        }
        object[] OnEndUpdate(IAsyncResult result)
        {
            this.EndUpdate(result);
            return null;
        }

        class CancelAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndCancel = new AsyncCompletion(HandleEndCancel);
            bool isTransacted;
            IWorkflowInstanceManagement channel;

            public CancelAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;

                if (Cancel(instanceId))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CancelAsyncResult>(result);
            }

            bool Cancel(Guid instanceId)
            {
                IAsyncResult result;
                AsyncCallback callback = this.PrepareAsyncCompletion(handleEndCancel);

                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedCancel(instanceId, callback, this);
                }
                else
                {
                    result = this.channel.BeginCancel(instanceId, callback, this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndCancel(result);
                }
                return false;
            }

            static bool HandleEndCancel(IAsyncResult result)
            {
                CancelAsyncResult thisPtr = (CancelAsyncResult)result.AsyncState;

                if (thisPtr.isTransacted)
                {
                    thisPtr.channel.EndTransactedCancel(result);
                }
                else
                {
                    thisPtr.channel.EndCancel(result);
                }
                return true;
            }
        }

        class RunAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndResume = new AsyncCompletion(HandleEndRun);
            bool isTransacted;
            IWorkflowInstanceManagement channel;

            public RunAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, Guid instanceId,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;

                if (Run(instanceId))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<RunAsyncResult>(result);
            }


            bool Run(Guid instanceId)
            {
                IAsyncResult result;
                AsyncCallback callback = this.PrepareAsyncCompletion(handleEndResume);

                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedRun(instanceId, callback, this);
                }
                else
                {
                    result = this.channel.BeginRun(instanceId, callback, this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndRun(result);
                }
                return false;
            }

            static bool HandleEndRun(IAsyncResult result)
            {
                RunAsyncResult thisPtr = (RunAsyncResult)result.AsyncState;

                if (thisPtr.isTransacted)
                {
                    thisPtr.channel.EndTransactedRun(result);
                }
                else
                {
                    thisPtr.channel.EndRun(result);
                }
                return true;
            }
        }
        class SuspendAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndSuspend = new AsyncCompletion(HandleEndSuspend);
            bool isTransacted;
            IWorkflowInstanceManagement channel;

            public SuspendAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted,
                Guid instanceId, string reason, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;

                if (Suspend(instanceId, reason))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SuspendAsyncResult>(result);
            }

            bool Suspend(Guid instanceId, string reason)
            {
                IAsyncResult result;
                AsyncCallback callback = this.PrepareAsyncCompletion(handleEndSuspend);

                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedSuspend(instanceId, reason, callback, this);
                }
                else
                {
                    result = this.channel.BeginSuspend(instanceId, reason, callback, this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndSuspend(result);
                }

                return false;
            }

            static bool HandleEndSuspend(IAsyncResult result)
            {
                SuspendAsyncResult thisPtr = (SuspendAsyncResult)result.AsyncState;

                if (thisPtr.isTransacted)
                {
                    thisPtr.channel.EndTransactedSuspend(result);
                }
                else
                {
                    thisPtr.channel.EndSuspend(result);
                }
                return true;
            }
        }
        class UnsuspendAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndUnsuspend = new AsyncCompletion(HandleEndUnsuspend);
            bool isTransacted;
            IWorkflowInstanceManagement channel;

            public UnsuspendAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted,
                Guid instanceId, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;

                if (Unsuspend(instanceId))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<UnsuspendAsyncResult>(result);
            }

            bool Unsuspend(Guid instanceId)
            {
                IAsyncResult result;
                AsyncCallback callback = this.PrepareAsyncCompletion(handleEndUnsuspend);

                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedUnsuspend(instanceId, callback, this);
                }
                else
                {
                    result = this.channel.BeginUnsuspend(instanceId, callback, this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndUnsuspend(result);
                }

                return false;
            }

            static bool HandleEndUnsuspend(IAsyncResult result)
            {
                UnsuspendAsyncResult thisPtr = (UnsuspendAsyncResult)result.AsyncState;

                if (thisPtr.isTransacted)
                {
                    thisPtr.channel.EndTransactedUnsuspend(result);
                }
                else
                {
                    thisPtr.channel.EndUnsuspend(result);
                }
                return true;
            }
        }
        class TerminateAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndTerminate = new AsyncCompletion(HandleEndTerminate);
            bool isTransacted;
            IWorkflowInstanceManagement channel;

            public TerminateAsyncResult(IWorkflowInstanceManagement channel, bool isTransacted, 
                Guid instanceId, string reason, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;

                if (Terminate(instanceId, reason))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TerminateAsyncResult>(result);
            }

            bool Terminate(Guid instanceId, string reason)
            {
                IAsyncResult result;
                AsyncCallback callback = this.PrepareAsyncCompletion(handleEndTerminate);

                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedTerminate(instanceId, reason, callback, this);
                }
                else
                {
                    result = this.channel.BeginTerminate(instanceId, reason, callback, this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndTerminate(result);
                }
                return false;
            }

            static bool HandleEndTerminate(IAsyncResult result)
            {
                TerminateAsyncResult thisPtr = (TerminateAsyncResult)result.AsyncState;

                if (thisPtr.isTransacted)
                {
                    thisPtr.channel.EndTransactedTerminate(result);
                }
                else
                {
                    thisPtr.channel.EndTerminate(result);
                }
                return true;
            }
        }
        class UpdateAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndUpdate = new AsyncCompletion(HandleEndUpdate);
            bool isTransacted;
            IWorkflowUpdateableInstanceManagement channel;

            public UpdateAsyncResult(IWorkflowUpdateableInstanceManagement channel, bool isTransacted, Guid instanceId, WorkflowIdentity updatedDefinitionIdentity,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isTransacted = isTransacted;
                this.channel = channel;

                if (Update(instanceId, updatedDefinitionIdentity))
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<UpdateAsyncResult>(result);
            }


            bool Update(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity)
            {
                IAsyncResult result;
                AsyncCallback callback = this.PrepareAsyncCompletion(handleEndUpdate);

                if (this.isTransacted)
                {
                    result = this.channel.BeginTransactedUpdate(instanceId, updatedDefinitionIdentity, callback, this);
                }
                else
                {
                    result = this.channel.BeginUpdate(instanceId, updatedDefinitionIdentity, callback, this);
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndUpdate(result);
                }
                return false;
            }

            static bool HandleEndUpdate(IAsyncResult result)
            {
                UpdateAsyncResult thisPtr = (UpdateAsyncResult)result.AsyncState;

                if (thisPtr.isTransacted)
                {
                    thisPtr.channel.EndTransactedUpdate(result);
                }
                else
                {
                    thisPtr.channel.EndUpdate(result);
                }
                return true;
            }
        }
    }
}
