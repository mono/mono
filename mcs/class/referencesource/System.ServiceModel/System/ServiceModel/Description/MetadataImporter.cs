//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Xml;

    public abstract partial class MetadataImporter
    {
        readonly KeyedByTypeCollection<IPolicyImportExtension> policyExtensions;
        readonly Dictionary<XmlQualifiedName, ContractDescription> knownContracts = new Dictionary<XmlQualifiedName, ContractDescription>();
        readonly Collection<MetadataConversionError> errors = new Collection<MetadataConversionError>();
        readonly Dictionary<object, object> state = new Dictionary<object, object>();

        //prevent inheritance until we are ready to allow it.
        internal MetadataImporter()
            : this (null, MetadataImporterQuotas.Defaults)
        {
        }

        internal MetadataImporter(IEnumerable<IPolicyImportExtension> policyImportExtensions)
            : this (policyImportExtensions, MetadataImporterQuotas.Defaults)
        {
        }

        internal MetadataImporter(IEnumerable<IPolicyImportExtension> policyImportExtensions,
            MetadataImporterQuotas quotas)
        {
            if (quotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("quotas");
            }

            if (policyImportExtensions == null)
            {
                policyImportExtensions = LoadPolicyExtensionsFromConfig();
            }

            this.Quotas = quotas;
            this.policyExtensions = new KeyedByTypeCollection<IPolicyImportExtension>(policyImportExtensions);
        }

        public KeyedByTypeCollection<IPolicyImportExtension> PolicyImportExtensions
        {
            get { return this.policyExtensions; }
        }

        public Collection<MetadataConversionError> Errors
        {
            get { return this.errors; }
        }

        public Dictionary<object, object> State
        {
            get { return this.state; }
        }

        public Dictionary<XmlQualifiedName, ContractDescription> KnownContracts
        {
            get { return this.knownContracts; }
        }

        // Abstract Building Methods
        public abstract Collection<ContractDescription> ImportAllContracts();
        public abstract ServiceEndpointCollection ImportAllEndpoints();

        internal virtual XmlElement ResolvePolicyReference(string policyReference, XmlElement contextAssertion)
        {
            return null;
        }
        internal BindingElementCollection ImportPolicy(ServiceEndpoint endpoint, Collection<Collection<XmlElement>> policyAlternatives)
        {
            foreach (Collection<XmlElement> selectedPolicy in policyAlternatives)
            {
                BindingOnlyPolicyConversionContext policyConversionContext = new BindingOnlyPolicyConversionContext(endpoint, selectedPolicy);

                if (TryImportPolicy(policyConversionContext))
                {
                    return policyConversionContext.BindingElements;
                }
            }
            return null;
        }

        internal bool TryImportPolicy(PolicyConversionContext policyContext)
        {
            foreach (IPolicyImportExtension policyImporter in policyExtensions)
                try
                {
                    policyImporter.ImportPolicy(this, policyContext);
                }
#pragma warning suppress 56500 // covered by FxCOP
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateExtensionException(policyImporter, e));
                }

            if (policyContext.GetBindingAssertions().Count != 0)
                return false;

            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                if (policyContext.GetOperationBindingAssertions(operation).Count != 0)
                    return false;

                foreach (MessageDescription message in operation.Messages)
                    if (policyContext.GetMessageBindingAssertions(message).Count != 0)
                        return false;
            }

            return true;
        }

        [Fx.Tag.SecurityNote(Critical = "uses ClientSection.UnsafeGetSection to get config in PT",
            Safe = "does not leak config object, just picks up extensions")]
        [SecuritySafeCritical]
        static Collection<IPolicyImportExtension> LoadPolicyExtensionsFromConfig()
        {
            return ClientSection.UnsafeGetSection().Metadata.LoadPolicyImportExtensions();
        }

        Exception CreateExtensionException(IPolicyImportExtension importer, Exception e)
        {
            string errorMessage = SR.GetString(SR.PolicyExtensionImportError, importer.GetType(), e.Message);
            return new InvalidOperationException(errorMessage, e);
        }

        internal class BindingOnlyPolicyConversionContext : PolicyConversionContext
        {
            static readonly PolicyAssertionCollection noPolicy = new PolicyAssertionCollection();
            readonly BindingElementCollection bindingElements = new BindingElementCollection();
            readonly PolicyAssertionCollection bindingPolicy;

            internal BindingOnlyPolicyConversionContext(ServiceEndpoint endpoint, IEnumerable<XmlElement> bindingPolicy)
                : base(endpoint)
            {
                this.bindingPolicy = new PolicyAssertionCollection(bindingPolicy);
            }

            public override BindingElementCollection BindingElements { get { return this.bindingElements; } }

            public override PolicyAssertionCollection GetBindingAssertions()
            {
                return this.bindingPolicy;
            }

            public override PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation)
            {
                return noPolicy;
            }

            public override PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message)
            {
                return noPolicy;
            }

            public override PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription fault)
            {
                return noPolicy;
            }
        }
    }

}
