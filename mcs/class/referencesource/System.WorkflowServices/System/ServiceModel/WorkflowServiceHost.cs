//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class WorkflowServiceHost : ServiceHostBase
    {
        IList<Type> reflectedContracts;
        WorkflowDefinitionContext workflowDefinitionContext;

        public WorkflowServiceHost(Type workflowType, params Uri[] baseAddress) :
            this(new CompiledWorkflowDefinitionContext(workflowType), baseAddress)
        {

        }

        public WorkflowServiceHost(string workflowDefinitionPath, params Uri[] baseAddress) :
            this(new StreamedWorkflowDefinitionContext(workflowDefinitionPath, null, null), baseAddress)
        {

        }

        public WorkflowServiceHost(string workflowDefinitionPath, string ruleDefinitionPath, params Uri[] baseAddress)
            : this(new StreamedWorkflowDefinitionContext(workflowDefinitionPath, ruleDefinitionPath, null), baseAddress)
        {

        }

        public WorkflowServiceHost(string workflowDefinitionPath, string ruleDefinitionPath, ITypeProvider typeProvider, params Uri[] baseAddress)
            : this(new StreamedWorkflowDefinitionContext(workflowDefinitionPath, ruleDefinitionPath, typeProvider), baseAddress)
        {

        }


        public WorkflowServiceHost(Stream workflowDefinition, params Uri[] baseAddress) :
            this(new StreamedWorkflowDefinitionContext(workflowDefinition, null, null), baseAddress)
        {

        }

        public WorkflowServiceHost(Stream workflowDefinition, Stream ruleDefinition, params Uri[] baseAddress) :
            this(new StreamedWorkflowDefinitionContext(workflowDefinition, ruleDefinition, null), baseAddress)
        {

        }

        public WorkflowServiceHost(Stream workflowDefinition, Stream ruleDefinition, ITypeProvider typeProvider, params Uri[] baseAddress)
            : this(new StreamedWorkflowDefinitionContext(workflowDefinition, ruleDefinition, typeProvider), baseAddress)
        {

        }

        // Based on prior art from WCF:
        // ServiceModel.lst:System.ServiceModel.ServiceHost..ctor(System.Object,System.Uri[])
        // |DoNotCallOverridableMethodsInConstructors
        // |Microsoft|By design, don't want to complicate ServiceHost state model
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal WorkflowServiceHost(WorkflowDefinitionContext workflowDefinitionContext, params Uri[] baseAddress)
            : base()
        {
            InitializeDescription(workflowDefinitionContext, new UriSchemeKeyedCollection(baseAddress));
        }

        protected WorkflowServiceHost()
        {

        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, string address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }

            return this.AddServiceEndpoint(implementedContract, binding, new Uri(address, UriKind.RelativeOrAbsolute));
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, Uri address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, null);
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, string address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }

            return this.AddServiceEndpoint(implementedContract, binding, new Uri(address, UriKind.RelativeOrAbsolute), listenUri);
        }

        public ServiceEndpoint AddServiceEndpoint(Type implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            if (implementedContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("implementedContract"));
            }
            if (!implementedContract.IsDefined(typeof(ServiceContractAttribute), false))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.ServiceContractAttributeNotFound, new object[] { implementedContract.FullName })));
            }
            if (this.reflectedContracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.ReflectedContractsNotInitialized, new object[] { implementedContract.FullName })));
            }

            if (!reflectedContracts.Contains(implementedContract))
            {
                if (ServiceMetadataBehavior.IsMetadataImplementedType(implementedContract))
                {
                    if (!this.Description.Behaviors.Contains(
                        typeof(ServiceMetadataBehavior)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.MetadataEndpointCannotBeAdded, new object[] { implementedContract.FullName })));
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR2.GetString(SR2.ReflectedContractKeyNotFound, new object[] { implementedContract.FullName, this.workflowDefinitionContext.WorkflowName })));
                }
            }
            ServiceEndpoint endpoint = base.AddServiceEndpoint(ContractDescription.GetContract(implementedContract).ConfigurationName, binding, address);

            if (listenUri != null)
            {
                listenUri = base.MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }

            return endpoint;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#", Justification = "This is defined by the ServiceHost base class")]
        protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            Fx.Assert(this.workflowDefinitionContext != null, "Null Workflow Definition");
            return new DescriptionCreator(this.workflowDefinitionContext).BuildServiceDescription(out implementedContracts, out this.reflectedContracts);
        }

        protected override void OnClosing()
        {
            WorkflowRuntimeBehavior workflowRuntimeBehavior = this.Description.Behaviors.Find<WorkflowRuntimeBehavior>();

            if (workflowRuntimeBehavior != null)
            {
                workflowRuntimeBehavior.WorkflowRuntime.StopRuntime();
            }
            base.OnClosing();
        }

        void InitializeDescription(WorkflowDefinitionContext workflowDefinitionContext, UriSchemeKeyedCollection baseAddresses)
        {
            this.workflowDefinitionContext = workflowDefinitionContext;
            this.InitializeDescription(baseAddresses);

            if (!this.Description.Behaviors.Contains(typeof(WorkflowRuntimeBehavior)))
            {
                this.Description.Behaviors.Add(new WorkflowRuntimeBehavior());
            }
        }
    }
}
