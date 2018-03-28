//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.ServiceModel.Channels;

    //For export we provide a builder that allows the gradual construction of a set of MetadataDocuments
    public abstract class MetadataExporter
    {
        PolicyVersion policyVersion = PolicyVersion.Policy12;
        readonly Collection<MetadataConversionError> errors = new Collection<MetadataConversionError>();
        readonly Dictionary<object, object> state = new Dictionary<object, object>();

        //prevent inheritance until we are ready to allow it.
        internal MetadataExporter()
        {
        }

        public PolicyVersion PolicyVersion
        {
            get
            {
                return this.policyVersion;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.policyVersion = value;
            }
        }

        public Collection<MetadataConversionError> Errors
        {
            get { return this.errors; }
        }
        public Dictionary<object, object> State
        {
            get { return this.state; }
        }

        public abstract void ExportContract(ContractDescription contract);
        public abstract void ExportEndpoint(ServiceEndpoint endpoint);

        public abstract MetadataSet GetGeneratedMetadata();

        internal PolicyConversionContext ExportPolicy(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            PolicyConversionContext policyContext = new ExportedPolicyConversionContext(endpoint, bindingParameters);

            foreach (IPolicyExportExtension exporter in endpoint.Binding.CreateBindingElements().FindAll<IPolicyExportExtension>())
                try
                {
                    exporter.ExportPolicy(this, policyContext);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateExtensionException(exporter, e));
                }

            return policyContext;
        }

        protected internal PolicyConversionContext ExportPolicy(ServiceEndpoint endpoint)
        {
            return this.ExportPolicy(endpoint, null);
        }

        sealed class ExportedPolicyConversionContext : PolicyConversionContext
        {
            readonly BindingElementCollection bindingElements;
            PolicyAssertionCollection bindingAssertions;
            Dictionary<OperationDescription, PolicyAssertionCollection> operationBindingAssertions;
            Dictionary<MessageDescription, PolicyAssertionCollection> messageBindingAssertions;
            Dictionary<FaultDescription, PolicyAssertionCollection> faultBindingAssertions;
            BindingParameterCollection bindingParameters;

            internal ExportedPolicyConversionContext(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
                : base(endpoint)
            {
                this.bindingElements = endpoint.Binding.CreateBindingElements();
                this.bindingAssertions = new PolicyAssertionCollection();
                this.operationBindingAssertions = new Dictionary<OperationDescription, PolicyAssertionCollection>();
                this.messageBindingAssertions = new Dictionary<MessageDescription, PolicyAssertionCollection>();
                this.faultBindingAssertions = new Dictionary<FaultDescription, PolicyAssertionCollection>();
                this.bindingParameters = bindingParameters;
            }

            public override BindingElementCollection BindingElements
            {
                get { return this.bindingElements; }
            }

            internal override BindingParameterCollection BindingParameters
            {
                get { return this.bindingParameters; }
            }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return bindingAssertions;
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                lock (operationBindingAssertions)
                {
                    if (!operationBindingAssertions.ContainsKey(operation))
                        operationBindingAssertions.Add(operation, new PolicyAssertionCollection());
                }

                return operationBindingAssertions[operation];
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                lock (messageBindingAssertions)
                {
                    if (!messageBindingAssertions.ContainsKey(message))
                        messageBindingAssertions.Add(message, new PolicyAssertionCollection());
                }
                return messageBindingAssertions[message];
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription fault)
            {
                lock (faultBindingAssertions)
                {
                    if (!faultBindingAssertions.ContainsKey(fault))
                        faultBindingAssertions.Add(fault, new PolicyAssertionCollection());
                }
                return faultBindingAssertions[fault];
            }

        }

        Exception CreateExtensionException(IPolicyExportExtension exporter, Exception e)
        {
            string errorMessage = SR.GetString(SR.PolicyExtensionExportError, exporter.GetType(), e.Message);
            return new InvalidOperationException(errorMessage, e);
        }
    }
}
