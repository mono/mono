//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using SR2 = System.ServiceModel.Activities.SR;

    class OperationExecutionFault : MessageFault
    {
        static FaultCode instanceAbortedCode;
        static FaultCode instanceCompletedCode;
        static FaultCode instanceTerminatedCode;
        static FaultCode instanceSuspendedFaultCode;
        static FaultCode instanceUnloadedFaultCode;
        static FaultCode instanceNotFoundCode;
        static FaultCode instanceLockedFaultCode;
        static FaultCode operationNotAvailableFaultCode;
        static FaultCode updatedFailedFaultCode;

        FaultCode faultCode;
        FaultReason faultReason;

        OperationExecutionFault(string description, FaultCode subcode)
        {
            this.faultCode = FaultCode.CreateSenderFaultCode(subcode);
            this.faultReason = new FaultReason(new FaultReasonText(
                description, CultureInfo.CurrentCulture));
        }

        public override FaultCode Code
        {
            get
            {
                return this.faultCode;
            }
        }

        public override bool HasDetail
        {
            get
            {
                return false;
            }
        }

        public override FaultReason Reason
        {
            get
            {
                return this.faultReason;
            }
        }

        public static OperationExecutionFault CreateTransactedLockException(Guid instanceId, string operationName)
        {
            if (instanceLockedFaultCode == null)
            {
                instanceLockedFaultCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceLockedUnderTransaction, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(SR2.InstanceLockedUnderTransaction(operationName, instanceId), instanceLockedFaultCode);
        }

        public static OperationExecutionFault CreateInstanceUnloadedFault(string description)
        {
            if (instanceUnloadedFaultCode == null)
            {
                instanceUnloadedFaultCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceUnloaded, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(description, instanceUnloadedFaultCode);
        }

        public static OperationExecutionFault CreateInstanceNotFoundFault(string description)
        {
            if (instanceNotFoundCode == null)
            {
                instanceNotFoundCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceNotFound, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(description, instanceNotFoundCode);
        }

        public static OperationExecutionFault CreateCompletedFault(string description)
        {
            if (instanceCompletedCode == null)
            {
                instanceCompletedCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceCompleted, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(description, instanceCompletedCode);
        }

        public static OperationExecutionFault CreateTerminatedFault(string description)
        {
            if (instanceTerminatedCode == null)
            {
                instanceTerminatedCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceTerminated, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(description, instanceTerminatedCode);
        }

        public static OperationExecutionFault CreateSuspendedFault(Guid instanceId, string operationName)
        {
            if (instanceSuspendedFaultCode == null)
            {
                instanceSuspendedFaultCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceSuspended, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(SR2.InstanceSuspended(operationName, instanceId), instanceSuspendedFaultCode);
        }

        public static OperationExecutionFault CreateOperationNotAvailableFault(Guid instanceId, string operationName)
        {
            if (operationNotAvailableFaultCode == null)
            {
                operationNotAvailableFaultCode = new FaultCode(XD2.WorkflowControlServiceFaults.OperationNotAvailable, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(SR2.OperationNotAvailable(operationName, instanceId), operationNotAvailableFaultCode);
        }

        public static OperationExecutionFault CreateAbortedFault(string description)
        {
            if (instanceAbortedCode == null)
            {
                instanceAbortedCode = new FaultCode(XD2.WorkflowControlServiceFaults.InstanceAborted, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(description, instanceAbortedCode);
        }

        public static OperationExecutionFault CreateUpdateFailedFault(string description)
        {
            if (updatedFailedFaultCode == null)
            {
                updatedFailedFaultCode = new FaultCode(XD2.WorkflowControlServiceFaults.UpdateFailed, XD2.WorkflowServices.Namespace);
            }
            return new OperationExecutionFault(description, updatedFailedFaultCode);
        }

        public static bool IsAbortedFaultException(FaultException exception)
        {
            if (exception.Code != null && exception.Code.SubCode != null &&
                exception.Code.SubCode.Name == instanceAbortedCode.Name && exception.Code.SubCode.Namespace == instanceAbortedCode.Namespace)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnWriteDetailContents(XmlDictionaryWriter writer)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
