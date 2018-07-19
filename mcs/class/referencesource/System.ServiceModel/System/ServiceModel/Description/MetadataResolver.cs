//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    public static class MetadataResolver
    {
        public static ServiceEndpointCollection Resolve(Type contract, EndpointAddress address)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }

            return Resolve(CreateContractCollection(contract), address);
        }
        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, EndpointAddress address)
        {
            return Resolve(contracts, address, new MetadataExchangeClient(address));
        }
        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, EndpointAddress address, MetadataExchangeClient client)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);


            MetadataSet metadataSet = client.GetMetadata(address);
            return ImportEndpoints(metadataSet, contracts, client);
        }

        public static ServiceEndpointCollection Resolve(Type contract, Uri address, MetadataExchangeClientMode mode)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }

            return Resolve(CreateContractCollection(contract), address, mode);
        }
        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode)
        {
            return Resolve(contracts, address, mode, new MetadataExchangeClient(address, mode));
        }
        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode, MetadataExchangeClient client)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            MetadataExchangeClientModeHelper.Validate(mode);
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);

            MetadataSet metadataSet = client.GetMetadata(address, mode);
            return ImportEndpoints(metadataSet, contracts, client);
        }

        public static IAsyncResult BeginResolve(Type contract, EndpointAddress address, AsyncCallback callback, object asyncState)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }

            return BeginResolve(CreateContractCollection(contract), address, callback, asyncState);
        }
        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, EndpointAddress address, AsyncCallback callback, object asyncState)
        {
            return BeginResolve(contracts, address, new MetadataExchangeClient(address), callback, asyncState);
        }
        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, EndpointAddress address, MetadataExchangeClient client, AsyncCallback callback, object asyncState)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);

            return new AsyncMetadataResolverHelper(address, MetadataExchangeClientMode.MetadataExchange, client, contracts, callback, asyncState);
        }

        public static IAsyncResult BeginResolve(Type contract, Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }

            return BeginResolve(CreateContractCollection(contract), address, mode, callback, asyncState);
        }
        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
        {
            return BeginResolve(contracts, address, mode, new MetadataExchangeClient(address, mode), callback, asyncState);
        }
        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode, MetadataExchangeClient client,
                                                 AsyncCallback callback, object asyncState)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            MetadataExchangeClientModeHelper.Validate(mode);
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);

            return new AsyncMetadataResolverHelper(new EndpointAddress(address), mode, client, contracts, callback, asyncState);
        }

        public static ServiceEndpointCollection EndResolve(IAsyncResult result)
        {
            return AsyncMetadataResolverHelper.EndAsyncCall(result);
        }

        class AsyncMetadataResolverHelper : AsyncResult
        {
            MetadataExchangeClient client;
            EndpointAddress address;
            ServiceEndpointCollection endpointCollection;
            MetadataExchangeClientMode mode;
            IEnumerable<ContractDescription> knownContracts;

            internal AsyncMetadataResolverHelper(EndpointAddress address, MetadataExchangeClientMode mode, MetadataExchangeClient client, IEnumerable<ContractDescription> knownContracts, AsyncCallback callback, object asyncState)
                : base(callback, asyncState)
            {
                this.address = address;
                this.client = client;
                this.mode = mode;
                this.knownContracts = knownContracts;

                GetMetadataSetAsync();
            }

            internal void GetMetadataSetAsync()
            {
                IAsyncResult result;

                if (this.mode == MetadataExchangeClientMode.HttpGet)
                {
                    result = this.client.BeginGetMetadata(this.address.Uri, MetadataExchangeClientMode.HttpGet, Fx.ThunkCallback(new AsyncCallback(this.EndGetMetadataSet)), null);
                }
                else
                {
                    result = this.client.BeginGetMetadata(this.address, Fx.ThunkCallback(new AsyncCallback(this.EndGetMetadataSet)), null);
                }

                if (result.CompletedSynchronously)
                {
                    HandleResult(result);
                    this.Complete(true);
                }
            }

            internal void EndGetMetadataSet(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                Exception exception = null;
                try
                {
                    HandleResult(result);

                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                    exception = e;
                }
                this.Complete(false, exception);

            }

            private void HandleResult(IAsyncResult result)
            {
                MetadataSet metadataSet = this.client.EndGetMetadata(result);
                endpointCollection = ImportEndpoints(metadataSet, knownContracts, this.client);
            }

            internal static ServiceEndpointCollection EndAsyncCall(IAsyncResult result)
            {
                AsyncMetadataResolverHelper helper = AsyncResult.End<AsyncMetadataResolverHelper>(result);
                return helper.endpointCollection;
            }
        }

        private static ServiceEndpointCollection ImportEndpoints(MetadataSet metadataSet, IEnumerable<ContractDescription> contracts, MetadataExchangeClient client)
        {
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();

            WsdlImporter importer = new WsdlImporter(metadataSet);

            // remember the original proxy so user doesn't need to set it again 
            importer.State.Add(MetadataExchangeClient.MetadataExchangeClientKey, client);

            foreach (ContractDescription cd in contracts)
            {
                importer.KnownContracts.Add(WsdlExporter.WsdlNamingHelper.GetPortTypeQName(cd), cd);
            }

            foreach (ContractDescription cd in contracts)
            {
                ServiceEndpointCollection contractEndpoints;
                contractEndpoints = importer.ImportEndpoints(cd);
                foreach (ServiceEndpoint se in contractEndpoints)
                {
                    endpoints.Add(se);
                }
            }

            //Trace all warnings and errors
            if (importer.Errors.Count > 0)
            {
                TraceWsdlImportErrors(importer);
            }

            return endpoints;
        }

        static void TraceWsdlImportErrors(WsdlImporter importer)
        {
            foreach (MetadataConversionError error in importer.Errors)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    Hashtable h = new Hashtable(2)
                    {
                        { "IsWarning", error.IsWarning },
                        { "Message", error.Message }
                    };
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.WsmexNonCriticalWsdlExportError,
                        SR.GetString(SR.TraceCodeWsmexNonCriticalWsdlExportError), new DictionaryTraceRecord(h), null, null);
                }
            }
        }

        private static void ValidateContracts(IEnumerable<ContractDescription> contracts)
        {
            bool isEmpty = true;
            Collection<XmlQualifiedName> qnames = new Collection<XmlQualifiedName>();
            foreach (ContractDescription cd in contracts)
            {
                isEmpty = false;
                if (cd == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMetadataResolverKnownContractsCannotContainNull));
                }

                XmlQualifiedName qname = WsdlExporter.WsdlNamingHelper.GetPortTypeQName(cd);
                if (qnames.Contains(qname))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMetadataResolverKnownContractsUniqueQNames, qname.Name, qname.Namespace));
                }

                qnames.Add(qname);
            }

            if (isEmpty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxMetadataResolverKnownContractsArgumentCannotBeEmpty));
            }
        }

        private static Collection<ContractDescription> CreateContractCollection(Type contract)
        {
            Collection<ContractDescription> contracts = new Collection<ContractDescription>();
            contracts.Add(ContractDescription.GetContract(contract));
            return contracts;
        }
    }
}

