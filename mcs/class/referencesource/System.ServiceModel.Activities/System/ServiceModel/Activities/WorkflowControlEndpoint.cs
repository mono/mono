//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities
{
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Runtime;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
    
    [Fx.Tag.XamlVisible(false)]
    public class WorkflowControlEndpoint : ServiceEndpoint
    {
        static Uri defaultBaseUri;

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile ContractDescription workflowControlServiceBaseContract;
        static volatile ContractDescription workflowControlServiceContract;
        static object workflowContractDescriptionLock = new object();

        public WorkflowControlEndpoint()
            : this(WorkflowControlEndpoint.GetDefaultBinding(),
            new EndpointAddress(new Uri(WorkflowControlEndpoint.DefaultBaseUri, new Uri(Guid.NewGuid().ToString(), UriKind.Relative))))
        {
        }

        public WorkflowControlEndpoint(Binding binding, EndpointAddress address)
            : base(WorkflowControlEndpoint.WorkflowControlServiceContract, binding, address)
        {
            this.IsSystemEndpoint = true;
        }

        internal static ContractDescription WorkflowControlServiceBaseContract
        {
            get
            {
                if (workflowControlServiceBaseContract == null)
                {
                    lock (workflowContractDescriptionLock)
                    {
                        if (workflowControlServiceBaseContract == null)
                        {
                            foreach (OperationDescription operation in WorkflowControlServiceContract.Operations)
                            {
                                if (operation.DeclaringContract != WorkflowControlServiceContract)
                                {
                                    workflowControlServiceBaseContract = operation.DeclaringContract;
                                    break;
                                }
                            }
                        }
                    }
                }
                return workflowControlServiceBaseContract;
            }
        }

        internal static ContractDescription WorkflowControlServiceContract
        {
            get
            {
                if (workflowControlServiceContract == null)
                {
                    lock (workflowContractDescriptionLock)
                    {
                        if (workflowControlServiceContract == null)
                        {
                            ContractDescription tempControlServiceContract = ContractDescription.GetContract(
                                typeof(IWorkflowUpdateableInstanceManagement));
                            tempControlServiceContract.Behaviors.Add(new ServiceMetadataContractBehavior(true));
                            ApplyOperationBehaviors(tempControlServiceContract);
                            // For back-compat, need to support existing code which expects the old contract type
                            tempControlServiceContract.ContractType = typeof(IWorkflowInstanceManagement);
                            workflowControlServiceContract = tempControlServiceContract;
                        }
                    }
                }
                return workflowControlServiceContract;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because it retreives the Process Id via Process.Id, which has a link demand for Full Trust.",
            Safe = "Safe because it demands FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        static void RetrieveProcessIdAndAppDomainId(out int processId, out int appDomainId)
        {
            processId = Process.GetCurrentProcess().Id;
            appDomainId = AppDomain.CurrentDomain.Id;
        }

        static Uri DefaultBaseUri
        {
            get
            {
                if (defaultBaseUri == null)
                {
                    // If we are running in full trust, use the ProcessId and AppDomainId. If partial trust, use a new guid to make the URI unique and avoid
                    // the usage of ProcessId and AppDomainId. We are doing this for back compat.
                    if (PartialTrustHelpers.AppDomainFullyTrusted)
                    {
                        int processId;
                        int appDomainId;
                        RetrieveProcessIdAndAppDomainId(out processId, out appDomainId);
                        defaultBaseUri = new Uri(string.Format(CultureInfo.InvariantCulture, "net.pipe://localhost/workflowControlServiceEndpoint/{0}/{1}",
                            processId,
                            appDomainId));
                    }
                    else
                    {
                        Uri tempUri = new Uri(string.Format(CultureInfo.InvariantCulture, "net.pipe://localhost/workflowControlServiceEndpoint/{0}",
                            Guid.NewGuid().ToString()));
                        // Using Interlocked.CompareExchange because new Guid.NewGuid is not atomic.
                        Interlocked.CompareExchange(ref defaultBaseUri, tempUri, null);
                    }
                }
                return defaultBaseUri;
            }
        }

        static Binding GetDefaultBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { TransactionFlow = true };
        }

        static void ApplyOperationBehaviors(ContractDescription contractDescription)
        {
            foreach (OperationDescription operationDescription in contractDescription.Operations)
            {
                //Except "Abandon" all the operations in this contract are Async.
                //All Transacted* operation are Transacted & Async.
                switch (operationDescription.Name)
                {
                    case XD2.WorkflowInstanceManagementService.Abandon:
                    case XD2.WorkflowInstanceManagementService.Cancel:
                    case XD2.WorkflowInstanceManagementService.Run:
                    case XD2.WorkflowInstanceManagementService.Suspend:
                    case XD2.WorkflowInstanceManagementService.Terminate:
                    case XD2.WorkflowInstanceManagementService.Unsuspend:
                    case XD2.WorkflowInstanceManagementService.Update:
                        EnsureDispatch(operationDescription);
                        break;
                    case XD2.WorkflowInstanceManagementService.TransactedCancel:
                    case XD2.WorkflowInstanceManagementService.TransactedRun:
                    case XD2.WorkflowInstanceManagementService.TransactedSuspend:
                    case XD2.WorkflowInstanceManagementService.TransactedTerminate:
                    case XD2.WorkflowInstanceManagementService.TransactedUnsuspend:
                    case XD2.WorkflowInstanceManagementService.TransactedUpdate:
                        EnsureDispatch(operationDescription);
                        EnsureTransactedInvoke(operationDescription);
                        break;
                }
            }
        }

        static void EnsureDispatch(OperationDescription operationDescription)
        {
            operationDescription.Behaviors.Add(new ControlOperationBehavior(false));
        }

        static void EnsureTransactedInvoke(OperationDescription operationDescription)
        {
            OperationBehaviorAttribute operationAttribute = operationDescription.Behaviors.Find<OperationBehaviorAttribute>();
            operationAttribute.TransactionScopeRequired = true;
        }
    }
}
