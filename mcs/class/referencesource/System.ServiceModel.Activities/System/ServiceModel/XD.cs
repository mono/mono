//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

// NOTE: will eventually get generated from 'xd.xml' if we get extra performance from XML Dictionary strings
// This would entail elevating the most common strings into a "Main" dictionary

namespace System.ServiceModel
{
    using System;

    static class XD2
    {
        public static class WorkflowServices
        {
            public const string Namespace = "http://schemas.datacontract.org/2008/10/WorkflowServices";
        }

        public static class DurableInstancing
        {
            public const string Namespace = "http://schemas.datacontract.org/2004/07/System.Runtime.DurableInstancing";
        }

        public static class ContextHeader
        {
            public const string DurableInstanceContext = "InstanceId";
            public const string Namespace = "http://schemas.microsoft.com/ws/2006/05/context";
            public const string MissingContextHeader = "MissingContext";
            public const string IsNewInstance = "IsNewInstance";
        }

        public static class WorkflowInstanceManagementService
        {
            public const string ConfigurationName = "System.ServiceModel.Activities.IWorkflowInstanceManagement";
            public const string ContractName = "IWorkflowInstanceManagement";

            public const string Abandon = "Abandon";
            public const string Cancel = "Cancel";
            public const string Run = "Run";
            public const string Suspend = "Suspend";
            public const string Terminate = "Terminate";
            public const string Unsuspend = "Unsuspend";
            public const string Update = "Update";
            public const string TransactedCancel = "TransactedCancel";
            public const string TransactedRun = "TransactedRun";
            public const string TransactedSuspend = "TransactedSuspend";
            public const string TransactedTerminate = "TransactedTerminate";
            public const string TransactedUnsuspend = "TransactedUnsuspend";
            public const string TransactedUpdate = "TransactedUpdate";
        }

        public static class WorkflowControlServiceFaults
        {
            public const string InstanceNotFound = "InstanceNotFound";
            public const string InstanceUnloaded = "InstanceUnloaded";
            public const string InstanceSuspended = "InstanceSuspended";
            public const string InstanceLockedUnderTransaction = "InstanceLockedUnderTransaction";
            public const string OperationNotAvailable = "OperationNotAvailable";
            public const string InstanceNotSuspended = "InstanceNotSuspended";
            public const string InstanceCompleted = "InstanceCompleted";
            public const string InstanceTerminated = "InstanceTerminated";
            public const string InstanceAborted = "InstanceAborted";
            public const string UpdateFailed = "UpdateFailed";
        }
    }
}


