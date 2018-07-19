// <copyright file="WorkflowRuntimeService.cs" company="Microsoft">Copyright (c) Microsoft Corporation.  All rights reserved.</copyright>

using System;
using System.Globalization;

using System.Workflow.Runtime;

namespace System.Workflow.Runtime.Hosting
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum WorkflowRuntimeServiceState
    {
        Stopped,
        Starting,
        Started,
        Stopping
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    abstract public class WorkflowRuntimeService
    {
        private WorkflowRuntime _runtime;
        private WorkflowRuntimeServiceState state = WorkflowRuntimeServiceState.Stopped;

        protected WorkflowRuntime Runtime
        {
            get
            {
                return _runtime;
            }

        }

        internal void SetRuntime(WorkflowRuntime runtime)
        {
            if (runtime == null && _runtime != null)
            {
                _runtime.Started -= this.HandleStarted;
                _runtime.Stopped -= this.HandleStopped;
            }
            _runtime = runtime;
            if (runtime != null)
            {
                _runtime.Started += this.HandleStarted;
                _runtime.Stopped += this.HandleStopped;
            }
        }

        protected void RaiseServicesExceptionNotHandledEvent(Exception exception, Guid instanceId)
        {
            Runtime.RaiseServicesExceptionNotHandledEvent(exception, instanceId);
        }

        internal void RaiseExceptionNotHandledEvent(Exception exception, Guid instanceId)
        {
            Runtime.RaiseServicesExceptionNotHandledEvent(exception, instanceId);
        }

        protected WorkflowRuntimeServiceState State
        {
            get { return state; }
        }

        virtual internal protected void Start()
        {
            if (_runtime == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceNotAddedToRuntime, this.GetType().Name));
            if (state.Equals(WorkflowRuntimeServiceState.Started))
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceAlreadyStarted, this.GetType().Name));

            state = WorkflowRuntimeServiceState.Starting;
        }

        virtual internal protected void Stop()
        {
            if (_runtime == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceNotAddedToRuntime, this.GetType().Name));
            if (state.Equals(WorkflowRuntimeServiceState.Stopped))
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.ServiceNotStarted, this.GetType().Name));

            state = WorkflowRuntimeServiceState.Stopping;
        }

        virtual protected void OnStarted()
        { }

        virtual protected void OnStopped()
        { }

        private void HandleStarted(object source, WorkflowRuntimeEventArgs e)
        {
            state = WorkflowRuntimeServiceState.Started;
            this.OnStarted();
        }

        private void HandleStopped(object source, WorkflowRuntimeEventArgs e)
        {
            state = WorkflowRuntimeServiceState.Stopped;
            this.OnStopped();
        }
    }
}
