//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.Activities;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;

    [WorkflowContractBehaviorAttribute]
    [ServiceContract(Name = XD2.WorkflowInstanceManagementService.ContractName, Namespace = XD2.WorkflowServices.Namespace)]
    public interface IWorkflowInstanceManagement
    {
        // Non-Transacted operations
        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Abandon)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance not found")]
        void Abandon(Guid instanceId, string reason);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Abandon, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "Abandon")]
        IAsyncResult BeginAbandon(Guid instanceId, string reason, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Abandon")]
        void EndAbandon(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Cancel)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is supended or locked under transaction")]
        void Cancel(Guid instanceId);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Cancel, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "Cancel")]
        IAsyncResult BeginCancel(Guid instanceId, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Cancel")]
        void EndCancel(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Run)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is locked under transaction")]
        void Run(Guid instanceId);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Run, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "Run")]
        IAsyncResult BeginRun(Guid instanceId, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Run")]
        void EndRun(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Suspend)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is locked under transaction")]
        void Suspend(Guid instanceId, string reason);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Suspend, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "Suspend")]
        IAsyncResult BeginSuspend(Guid instanceId, string reason, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Suspend")]
        void EndSuspend(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Terminate)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is supended or locked under transaction")]
        void Terminate(Guid instanceId, string reason);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Terminate, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "Terminate")]
        IAsyncResult BeginTerminate(Guid instanceId, string reason, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Terminate")]
        void EndTerminate(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Unsuspend)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is locked under transaction")]
        void Unsuspend(Guid instanceId);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Unsuspend, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "Unsuspend")]
        IAsyncResult BeginUnsuspend(Guid instanceId, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Unsuspend")]
        void EndUnsuspend(IAsyncResult result);

        //Transacted Operation
        // 

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedCancel)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is supended or locked under transaction")]
        void TransactedCancel(Guid instanceId);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedCancel, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "TransactedCancel")]
        IAsyncResult BeginTransactedCancel(Guid instanceId, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "TransactedCancel")]
        void EndTransactedCancel(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedRun)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is supended or locked under transaction")]
        void TransactedRun(Guid instanceId);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedRun, AsyncPattern = true)]
        [Fx.Tag.InheritThrows(From = "TransactedRun")]
        IAsyncResult BeginTransactedRun(Guid instanceId, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "TransactedRun")]
        void EndTransactedRun(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedSuspend)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is supended or locked under transaction")]
        void TransactedSuspend(Guid instanceId, string reason);

        [OperationContract(AsyncPattern = true, Name = XD2.WorkflowInstanceManagementService.TransactedSuspend)]
        [Fx.Tag.InheritThrows(From = "TransactedSuspend")]
        IAsyncResult BeginTransactedSuspend(Guid instanceId, string reason, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "TransactedSuspend")]
        void EndTransactedSuspend(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedTerminate)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is supended or locked under transaction")]
        void TransactedTerminate(Guid instanceId, string reason);

        [OperationContract(AsyncPattern = true, Name = XD2.WorkflowInstanceManagementService.TransactedTerminate)]
        [Fx.Tag.InheritThrows(From = "TransactedTerminate")]
        IAsyncResult BeginTransactedTerminate(Guid instanceId, string reason, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "TransactedTerminate")]
        void EndTransactedTerminate(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedUnsuspend)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance with specified identifier not found or it is locked under a transaction")]
        void TransactedUnsuspend(Guid instanceId);

        [OperationContract(AsyncPattern = true, Name = XD2.WorkflowInstanceManagementService.TransactedUnsuspend)]
        [Fx.Tag.InheritThrows(From = "TransactedUnsuspend")]
        IAsyncResult BeginTransactedUnsuspend(Guid instanceId, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "TransactedUnsuspend")]
        void EndTransactedUnsuspend(IAsyncResult result);
    }

    [WorkflowContractBehaviorAttribute]
    [ServiceContract(Name = XD2.WorkflowInstanceManagementService.ContractName, Namespace = XD2.WorkflowServices.Namespace,
        ConfigurationName = XD2.WorkflowInstanceManagementService.ConfigurationName)]
    public interface IWorkflowUpdateableInstanceManagement : IWorkflowInstanceManagement
    {
        [OperationContract(Name = XD2.WorkflowInstanceManagementService.Update)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance not found, locked under transaction, or update unsuccessful")]
        void Update(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity);

        [OperationContract(AsyncPattern = true, Name = XD2.WorkflowInstanceManagementService.Update)]
        [Fx.Tag.InheritThrows(From = "Update")]
        IAsyncResult BeginUpdate(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "Update")]
        void EndUpdate(IAsyncResult result);

        [OperationContract(Name = XD2.WorkflowInstanceManagementService.TransactedUpdate)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        [Fx.Tag.Throws(typeof(FaultException), "Instance not found, locked under transaction, or update unsuccessful")]
        void TransactedUpdate(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity);

        [OperationContract(AsyncPattern = true, Name = XD2.WorkflowInstanceManagementService.TransactedUpdate)]
        [Fx.Tag.InheritThrows(From = "TransactedUpdate")]
        IAsyncResult BeginTransactedUpdate(Guid instanceId, WorkflowIdentity updatedDefinitionIdentity, AsyncCallback callback, object state);
        [Fx.Tag.InheritThrows(From = "TransactedUpdate")]
        void EndTransactedUpdate(IAsyncResult result);
    }
}
