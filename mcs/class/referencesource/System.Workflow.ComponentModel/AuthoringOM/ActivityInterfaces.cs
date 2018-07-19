// ****************************************************************************
// Copyright (C) 2000-2001 Microsoft Corporation.  All rights reserved.
//
// CONTENTS
//     Activity interface
// 
// DESCRIPTION
//
// REVISIONS
// Date          Ver     By           Remarks
// ~~~~~~~~~~    ~~~     ~~~~~~~~     ~~~~~~~~~~~~~~
// 03/19/04      1.0     MayankM       interfaces
// ****************************************************************************
namespace System.Workflow.ComponentModel
{
    using System;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.CodeDom;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Security.Principal;
    using System.Security.Cryptography;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Threading;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IDynamicPropertyTypeProvider
    {
        Type GetPropertyType(IServiceProvider serviceProvider, string propertyName);
        AccessTypes GetAccessType(IServiceProvider serviceProvider, string propertyName);
    }

    internal interface ISupportWorkflowChanges
    {
        void OnActivityAdded(ActivityExecutionContext rootContext, Activity addedActivity);
        void OnActivityRemoved(ActivityExecutionContext rootContext, Activity removedActivity);
        void OnWorkflowChangesCompleted(ActivityExecutionContext rootContext);
    }
    internal interface ISupportAlternateFlow
    {
        IList<Activity> AlternateFlowActivities { get; }
    }

    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    internal sealed class ActivityExecutorAttribute : Attribute
    {
        private string executorTypeName = string.Empty;

        public ActivityExecutorAttribute(Type executorType)
        {
            if (executorType != null)
                this.executorTypeName = executorType.AssemblyQualifiedName;
        }

        public ActivityExecutorAttribute(string executorTypeName)
        {
            this.executorTypeName = executorTypeName;
        }

        public string ExecutorTypeName
        {
            get
            {
                return this.executorTypeName;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ActivityExecutionStatus : byte
    {
        Initialized = 0,
        Executing = 1,
        Canceling = 2,
        Closed = 3,
        Compensating = 4,
        Faulting = 5
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ActivityExecutionResult : byte
    {
        None = 0,
        Succeeded = 1,
        Canceled = 2,
        Compensated = 3,
        Faulted = 4,
        Uninitialized = 5
    }

    internal interface IDependencyObjectAccessor
    {
        //This method is invoked during the definition creation time
        void InitializeDefinitionForRuntime(DependencyObject parentDependencyObject);

        //This is invoked for every instance (not necessarily activating)
        void InitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime);

        //This is invoked for every activating instance
        void InitializeActivatingInstanceForRuntime(DependencyObject parentDependencyObject, IWorkflowCoreRuntime workflowCoreRuntime);

        T[] GetInvocationList<T>(DependencyProperty dependencyEvent);
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IStartWorkflow
    {
        Guid StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues);
    }

    internal interface IWorkflowCoreRuntime : IServiceProvider
    {
        // context information
        Activity RootActivity { get; }
        Activity CurrentActivity { get; }
        Activity CurrentAtomicActivity { get; }
        IDisposable SetCurrentActivity(Activity activity);

        void ScheduleItem(SchedulableItem item, bool isInAtomicTransaction, bool transacted, bool queueInTransaction);
        void ActivityStatusChanged(Activity activity, bool transacted, bool committed);
        void RaiseException(Exception e, Activity activity, string responsibleActivity);

        void RaiseActivityExecuting(Activity activity);
        void RaiseHandlerInvoking(Delegate delegateHandler);
        void RaiseHandlerInvoked();

        Guid StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues);

        // context activity related
        int GetNewContextActivityId();
        void RegisterContextActivity(Activity activity);
        void UnregisterContextActivity(Activity activity);
        Activity LoadContextActivity(ActivityExecutionContextInfo contextInfo, Activity outerContextActivity);
        void SaveContextActivity(Activity contextActivity);
        Activity GetContextActivityForId(int id);
        Object GetService(Activity currentActivity, Type serviceType);
        void PersistInstanceState(Activity activity);

        //Dynamic change notifications
        bool OnBeforeDynamicChange(IList<WorkflowChangeAction> changes);
        void OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes);
        bool IsDynamicallyUpdated { get; }

        // root level access
        Guid InstanceID { get; }
        bool SuspendInstance(string suspendDescription);
        void TerminateInstance(Exception e);
        bool Resume();
        void CheckpointInstanceState(Activity currentActivity);
        void RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendReason);
        void DisposeCheckpointState();

        // User Tracking
        void Track(string key, object data);

        // Timer Events
        WaitCallback ProcessTimersCallback { get; }
    }

    internal interface ITimerService
    {
        void ScheduleTimer(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId);
        void CancelTimer(Guid timerId);
    }

    [Serializable()]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowTerminatedException : Exception
    {
        private WorkflowTerminatedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public WorkflowTerminatedException()
            : base(SR.GetString(SR.Error_WorkflowTerminated))
        {
        }

        public WorkflowTerminatedException(string message)
            : base(message)
        {
        }
        public WorkflowTerminatedException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface ICompensatableActivity
    {
        ActivityExecutionStatus Compensate(ActivityExecutionContext executionContext);
    }

    #region Class AlternateFlowActivityAttribute

    [AttributeUsageAttribute(AttributeTargets.Class, AllowMultiple = false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class AlternateFlowActivityAttribute : Attribute
    {
    }
    #endregion

    #region Class SupportsTransactionAttribute

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class SupportsTransactionAttribute : Attribute
    {
    }
    #endregion

    #region Class SupportsSynchronizationAttribute

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class SupportsSynchronizationAttribute : Attribute
    {
    }
    #endregion

    #region Class PersistOnCloseAttribute

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class PersistOnCloseAttribute : Attribute
    {
    }
    #endregion
}
