//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Hosting;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Threading;

    [Fx.Tag.XamlVisible(false)]
    public sealed class WorkflowInvoker
    {
        static AsyncCallback cancelCallback;
        static AsyncCallback invokeCallback;
        WorkflowInstanceExtensionManager extensions;
        Dictionary<object, AsyncInvokeContext> pendingInvokes;
        SendOrPostCallback raiseInvokeCompletedCallback;
        object thisLock;
        Activity workflow;

        public WorkflowInvoker(Activity workflow)
        {
            if (workflow == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflow");
            }

            this.workflow = workflow;
            this.thisLock = new object();
        }

        public event EventHandler<InvokeCompletedEventArgs> InvokeCompleted;

        public WorkflowInstanceExtensionManager Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new WorkflowInstanceExtensionManager();
                }

                return this.extensions;
            }
        }

        Dictionary<object, AsyncInvokeContext> PendingInvokes
        {
            get
            {
                if (this.pendingInvokes == null)
                {
                    this.pendingInvokes = new Dictionary<object, AsyncInvokeContext>();
                }
                return this.pendingInvokes;
            }
        }

        SendOrPostCallback RaiseInvokeCompletedCallback
        {
            get
            {
                if (this.raiseInvokeCompletedCallback == null)
                {
                    this.raiseInvokeCompletedCallback =
                        Fx.ThunkCallback(new SendOrPostCallback(this.RaiseInvokeCompleted));
                }
                return this.raiseInvokeCompletedCallback;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        public static IDictionary<string, object> Invoke(Activity workflow)
        {
            return Invoke(workflow, ActivityDefaults.InvokeTimeout);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public static IDictionary<string, object> Invoke(Activity workflow, TimeSpan timeout)
        {
            return Invoke(workflow, timeout, null);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public static IDictionary<string, object> Invoke(Activity workflow, IDictionary<string, object> inputs)
        {
            return Invoke(workflow, inputs, ActivityDefaults.InvokeTimeout, null);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public static IDictionary<string, object> Invoke(Activity workflow, IDictionary<string, object> inputs, TimeSpan timeout)
        {
            return Invoke(workflow, inputs, timeout, null);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public static TResult Invoke<TResult>(Activity<TResult> workflow)
        {
            return Invoke(workflow, null);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public static TResult Invoke<TResult>(Activity<TResult> workflow, IDictionary<string, object> inputs)
        {
            return Invoke(workflow, inputs, ActivityDefaults.InvokeTimeout);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        public static TResult Invoke<TResult>(Activity<TResult> workflow, IDictionary<string, object> inputs, TimeSpan timeout)
        {
            IDictionary<string, object> dummyOutputs;
            return Invoke(workflow, inputs, out dummyOutputs, timeout);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
            Justification = "Generic needed for type inference")]
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters,
            Justification = "Arch approved design. Requires the out argument for extra information provided")]
        public static TResult Invoke<TResult>(Activity<TResult> workflow, IDictionary<string, object> inputs, out IDictionary<string, object> additionalOutputs, TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            if (inputs != null)
            {
                additionalOutputs = Invoke(workflow, inputs, timeout, null);
            }
            else
            {
                additionalOutputs = Invoke(workflow, timeout, null);
            }
            object untypedResult;
            if (additionalOutputs.TryGetValue("Result", out untypedResult))
            {
                additionalOutputs.Remove("Result");
                return (TResult)untypedResult;
            }
            else
            {
                throw Fx.AssertAndThrow("Activity<TResult> should always have a output named \"Result\"");
            }
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public IAsyncResult BeginInvoke(AsyncCallback callback, object state)
        {
            return BeginInvoke(this.workflow, ActivityDefaults.InvokeTimeout, this.extensions, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public IAsyncResult BeginInvoke(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return BeginInvoke(this.workflow, timeout, this.extensions, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public IAsyncResult BeginInvoke(IDictionary<string, object> inputs, AsyncCallback callback, object state)
        {
            return BeginInvoke(this.workflow, inputs, ActivityDefaults.InvokeTimeout, this.extensions, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public IAsyncResult BeginInvoke(IDictionary<string, object> inputs, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return BeginInvoke(this.workflow, inputs, timeout, this.extensions, callback, state);
        }

        public void CancelAsync(object userState)
        {
            if (userState == null)
            {
                throw FxTrace.Exception.ArgumentNull("userState");
            }

            AsyncInvokeContext context = this.RemoveFromPendingInvokes(userState);
            if (context != null)
            {
                // cancel does not need a timeout since it's bounded by the invoke timeout
                if (cancelCallback == null)
                {
                    cancelCallback = Fx.ThunkCallback(new AsyncCallback(CancelCallback));
                }
                // cancel only throws TimeoutException and shouldnt throw at all if timeout is infinite
                // cancel does not need to raise InvokeCompleted since the InvokeAsync invocation would raise it
                IAsyncResult result = context.WorkflowApplication.BeginCancel(TimeSpan.MaxValue, cancelCallback, context);
                if (result.CompletedSynchronously)
                {
                    context.WorkflowApplication.EndCancel(result);
                }
            }
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        public IDictionary<string, object> EndInvoke(IAsyncResult result)
        {
            return WorkflowApplication.EndInvoke(result);
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        public IDictionary<string, object> Invoke()
        {
            return WorkflowInvoker.Invoke(this.workflow, ActivityDefaults.InvokeTimeout, this.extensions);
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        public IDictionary<string, object> Invoke(TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return WorkflowInvoker.Invoke(this.workflow, timeout, this.extensions);
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        public IDictionary<string, object> Invoke(IDictionary<string, object> inputs)
        {
            return WorkflowInvoker.Invoke(this.workflow, inputs, ActivityDefaults.InvokeTimeout, this.extensions);
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        public IDictionary<string, object> Invoke(IDictionary<string, object> inputs, TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return WorkflowInvoker.Invoke(this.workflow, inputs, timeout, this.extensions);
        }

        public void InvokeAsync()
        {
            InvokeAsync(ActivityDefaults.InvokeTimeout, null);
        }

        public void InvokeAsync(TimeSpan timeout)
        {
            InvokeAsync(timeout, null);
        }

        public void InvokeAsync(object userState)
        {
            InvokeAsync(ActivityDefaults.InvokeTimeout, userState);
        }

        public void InvokeAsync(TimeSpan timeout, object userState)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            InternalInvokeAsync(null, timeout, userState);
        }

        public void InvokeAsync(IDictionary<string, object> inputs)
        {
            InvokeAsync(inputs, null);
        }

        public void InvokeAsync(IDictionary<string, object> inputs, TimeSpan timeout)
        {
            InvokeAsync(inputs, timeout, null);
        }

        public void InvokeAsync(IDictionary<string, object> inputs, object userState)
        {
            InvokeAsync(inputs, ActivityDefaults.InvokeTimeout, userState);
        }

        public void InvokeAsync(IDictionary<string, object> inputs, TimeSpan timeout, object userState)
        {
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            InternalInvokeAsync(inputs, timeout, userState);
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        static IDictionary<string, object> Invoke(Activity workflow, TimeSpan timeout, WorkflowInstanceExtensionManager extensions)
        {
            if (workflow == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflow");
            }

            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            IDictionary<string, object> outputs = WorkflowApplication.Invoke(workflow, null, extensions, timeout);

            if (outputs == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            else
            {
                return outputs;
            }
        }

        [Fx.Tag.Throws.Timeout("A timeout occurred when invoking the workflow")]
        static IDictionary<string, object> Invoke(Activity workflow, IDictionary<string, object> inputs, TimeSpan timeout, WorkflowInstanceExtensionManager extensions)
        {
            if (workflow == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflow");
            }

            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }

            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            IDictionary<string, object> outputs = WorkflowApplication.Invoke(workflow, inputs, extensions, timeout);

            if (outputs == null)
            {
                return ActivityUtilities.EmptyParameters;
            }
            else
            {
                return outputs;
            }
        }

        void AddToPendingInvokes(AsyncInvokeContext context)
        {
            lock (ThisLock)
            {
                if (this.PendingInvokes.ContainsKey(context.UserState))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.SameUserStateUsedForMultipleInvokes));
                }
                this.PendingInvokes.Add(context.UserState, context);
            }
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        IAsyncResult BeginInvoke(Activity workflow, IDictionary<string, object> inputs, TimeSpan timeout, WorkflowInstanceExtensionManager extensions, AsyncCallback callback, object state)
        {
            if (inputs == null)
            {
                throw FxTrace.Exception.ArgumentNull("inputs");
            }

            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return WorkflowApplication.BeginInvoke(workflow, inputs, extensions, timeout, null, null, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "Invoke")]
        IAsyncResult BeginInvoke(Activity workflow, TimeSpan timeout, WorkflowInstanceExtensionManager extensions, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            return WorkflowApplication.BeginInvoke(workflow, null, extensions, timeout, null, null, callback, state);
        }

        void CancelCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            AsyncInvokeContext context = (AsyncInvokeContext)result.AsyncState;
            // cancel only throws TimeoutException and shouldnt throw at all if timeout is infinite
            context.WorkflowApplication.EndCancel(result);
        }

        void InternalInvokeAsync(IDictionary<string, object> inputs, TimeSpan timeout, object userState)
        {
            AsyncInvokeContext context = new AsyncInvokeContext(userState, this);
            if (userState != null)
            {
                AddToPendingInvokes(context);
            }
            Exception error = null;
            bool completedSynchronously = false;
            try
            {
                if (invokeCallback == null)
                {
                    invokeCallback = Fx.ThunkCallback(new AsyncCallback(InvokeCallback));
                }
                context.Operation.OperationStarted();
                IAsyncResult result = WorkflowApplication.BeginInvoke(this.workflow, inputs, this.extensions, timeout, SynchronizationContext.Current, context, invokeCallback, context);
                if (result.CompletedSynchronously)
                {
                    context.Outputs = this.EndInvoke(result);
                    completedSynchronously = true;
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            if (error != null || completedSynchronously)
            {
                PostInvokeCompletedAndRemove(context, error);
            }
        }

        void InvokeCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }
            AsyncInvokeContext context = (AsyncInvokeContext)result.AsyncState;
            WorkflowInvoker thisPtr = context.Invoker;
            Exception error = null;
            try
            {
                context.Outputs = thisPtr.EndInvoke(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }
            thisPtr.PostInvokeCompletedAndRemove(context, error);
        }

        void PostInvokeCompleted(AsyncInvokeContext context, Exception error)
        {
            bool cancelled;
            if (error == null)
            {
                context.WorkflowApplication.GetCompletionStatus(out error, out cancelled);
            }
            else
            {
                cancelled = false;
            }
            PostInvokeCompleted(context, cancelled, error);
        }

        void PostInvokeCompleted(AsyncInvokeContext context, bool cancelled, Exception error)
        {
            InvokeCompletedEventArgs e = new InvokeCompletedEventArgs(error, cancelled, context);
            if (this.InvokeCompleted == null)
            {
                context.Operation.OperationCompleted();
            }
            else
            {
                context.Operation.PostOperationCompleted(this.RaiseInvokeCompletedCallback, e);
            }
        }

        void PostInvokeCompletedAndRemove(AsyncInvokeContext context, Exception error)
        {
            if (context.UserState != null)
            {
                RemoveFromPendingInvokes(context.UserState);
            }
            PostInvokeCompleted(context, error);
        }

        void RaiseInvokeCompleted(object state)
        {
            EventHandler<InvokeCompletedEventArgs> handler = this.InvokeCompleted;
            if (handler != null)
            {
                handler(this, (InvokeCompletedEventArgs)state);
            }
        }

        AsyncInvokeContext RemoveFromPendingInvokes(object userState)
        {
            AsyncInvokeContext context;
            lock (ThisLock)
            {
                if (this.PendingInvokes.TryGetValue(userState, out context))
                {
                    this.PendingInvokes.Remove(userState);
                }
            }
            return context;
        }
    }
}
