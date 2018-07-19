//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    public class ServiceMetadataBehavior : IServiceBehavior
    {
        public const string MexContractName = "IMetadataExchange";
        internal const string MexContractNamespace = "http://schemas.microsoft.com/2006/04/mex";

        static readonly Uri emptyUri = new Uri(String.Empty, UriKind.Relative);

        bool httpGetEnabled = false;
        bool httpsGetEnabled = false;
        Uri httpGetUrl;
        Uri httpsGetUrl;
        Binding httpGetBinding;
        Binding httpsGetBinding;
        Uri externalMetadataLocation = null;
        MetadataExporter metadataExporter = null;

        static ContractDescription mexContract = null;
        static object thisLock = new object();

        public bool HttpGetEnabled
        {
            get { return this.httpGetEnabled; }
            set { this.httpGetEnabled = value; }
        }

        [TypeConverter(typeof(UriTypeConverter))]
        public Uri HttpGetUrl
        {
            get { return this.httpGetUrl; }
            set
            {
                if (value != null && value.IsAbsoluteUri && value.Scheme != Uri.UriSchemeHttp)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxServiceMetadataBehaviorUrlMustBeHttpOrRelative,
                            "HttpGetUrl", Uri.UriSchemeHttp, value.ToString(), value.Scheme));
                }
                this.httpGetUrl = value;
            }
        }

        public bool HttpsGetEnabled
        {
            get { return this.httpsGetEnabled; }
            set { this.httpsGetEnabled = value; }
        }

        [TypeConverter(typeof(UriTypeConverter))]
        public Uri HttpsGetUrl
        {
            get { return this.httpsGetUrl; }
            set
            {
                if (value != null && value.IsAbsoluteUri && value.Scheme != Uri.UriSchemeHttps)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxServiceMetadataBehaviorUrlMustBeHttpOrRelative,
                        "HttpsGetUrl", Uri.UriSchemeHttps, value.ToString(), value.Scheme));
                }

                this.httpsGetUrl = value;
            }
        }

        public Binding HttpGetBinding
        {
            get { return this.httpGetBinding; }
            set
            {
                if (value != null)
                {
                    if (!value.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxBindingSchemeDoesNotMatch,
                            value.Scheme, value.GetType().ToString(), Uri.UriSchemeHttp));
                    }
                    CustomBinding customBinding = new CustomBinding(value);
                    TextMessageEncodingBindingElement textMessageEncodingBindingElement = customBinding.Elements.Find<TextMessageEncodingBindingElement>();
                    if (textMessageEncodingBindingElement != null && !textMessageEncodingBindingElement.MessageVersion.IsMatch(MessageVersion.None))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxIncorrectMessageVersion,
                            textMessageEncodingBindingElement.MessageVersion.ToString(), MessageVersion.None.ToString()));
                    }
                    HttpTransportBindingElement httpTransportBindingElement = customBinding.Elements.Find<HttpTransportBindingElement>();
                    if (httpTransportBindingElement != null)
                    {
                        httpTransportBindingElement.Method = "GET";
                    }
                    this.httpGetBinding = customBinding;
                }
            }
        }

        public Binding HttpsGetBinding
        {
            get { return this.httpsGetBinding; }
            set
            {
                if (value != null)
                {
                    if (!value.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxBindingSchemeDoesNotMatch,
                            value.Scheme, value.GetType().ToString(), Uri.UriSchemeHttps));
                    }
                    CustomBinding customBinding = new CustomBinding(value);
                    TextMessageEncodingBindingElement textMessageEncodingBindingElement = customBinding.Elements.Find<TextMessageEncodingBindingElement>();
                    if (textMessageEncodingBindingElement != null && !textMessageEncodingBindingElement.MessageVersion.IsMatch(MessageVersion.None))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SFxIncorrectMessageVersion,
                            textMessageEncodingBindingElement.MessageVersion.ToString(), MessageVersion.None.ToString()));
                    }
                    HttpsTransportBindingElement httpsTransportBindingElement = customBinding.Elements.Find<HttpsTransportBindingElement>();
                    if (httpsTransportBindingElement != null)
                    {
                        httpsTransportBindingElement.Method = "GET";
                    }
                    this.httpsGetBinding = customBinding;
                }
            }
        }

        [TypeConverter(typeof(UriTypeConverter))]

        public Uri ExternalMetadataLocation
        {
            get { return this.externalMetadataLocation; }
            set
            {
                if (value != null && value.IsAbsoluteUri && !(value.Scheme == Uri.UriSchemeHttp || value.Scheme == Uri.UriSchemeHttps))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ExternalMetadataLocation", SR.GetString(SR.SFxBadMetadataLocationUri, value.OriginalString, value.Scheme));
                }
                this.externalMetadataLocation = value;
            }
        }

        public MetadataExporter MetadataExporter
        {
            get
            {
                if (this.metadataExporter == null)
                    this.metadataExporter = new WsdlExporter();

                return this.metadataExporter;
            }
            set
            {
                this.metadataExporter = value;
            }
        }

        static internal ContractDescription MexContract
        {
            get
            {
                EnsureMexContractDescription();
                return ServiceMetadataBehavior.mexContract;
            }
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            if (serviceHostBase == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");

            ApplyBehavior(description, serviceHostBase);
        }

        void ApplyBehavior(ServiceDescription description, ServiceHostBase host)
        {
            ServiceMetadataExtension mex = ServiceMetadataExtension.EnsureServiceMetadataExtension(description, host);
            SetExtensionProperties(description, host, mex);
            CustomizeMetadataEndpoints(description, host, mex);
            CreateHttpGetEndpoints(description, host, mex);
        }

        private void CreateHttpGetEndpoints(ServiceDescription description, ServiceHostBase host, ServiceMetadataExtension mex)
        {
            bool httpDispatcherEnabled = false;
            bool httpsDispatcherEnabled = false;

            if (this.httpGetEnabled)
            {
                httpDispatcherEnabled = EnsureGetDispatcher(host, mex, this.httpGetUrl, Uri.UriSchemeHttp);
            }

            if (this.httpsGetEnabled)
            {
                httpsDispatcherEnabled = EnsureGetDispatcher(host, mex, this.httpsGetUrl, Uri.UriSchemeHttps);
            }

            if (!httpDispatcherEnabled && !httpsDispatcherEnabled)
            {
                if (this.httpGetEnabled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceMetadataBehaviorNoHttpBaseAddress)));
                }

                if (this.httpsGetEnabled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxServiceMetadataBehaviorNoHttpsBaseAddress)));
                }
            }
        }

        static bool EnsureGetDispatcher(ServiceHostBase host, ServiceMetadataExtension mex, Uri url, string scheme)
        {
            Uri address = host.GetVia(scheme, url == null ? new Uri(string.Empty, UriKind.Relative) : url);

            if (address != null)
            {
                ChannelDispatcher channelDispatcher = mex.EnsureGetDispatcher(address, false /* isServiceDebugBehavior */);
                ((ServiceMetadataExtension.HttpGetImpl)channelDispatcher.Endpoints[0].DispatchRuntime.SingletonInstanceContext.UserObject).GetWsdlEnabled = true;
                return true;
            }

            return false;
        }

        void SetExtensionProperties(ServiceDescription description, ServiceHostBase host, ServiceMetadataExtension mex)
        {
            mex.ExternalMetadataLocation = this.ExternalMetadataLocation;
            mex.Initializer = new MetadataExtensionInitializer(this, description, host);
            mex.HttpGetEnabled = this.httpGetEnabled;
            mex.HttpsGetEnabled = this.httpsGetEnabled;

            mex.HttpGetUrl = host.GetVia(Uri.UriSchemeHttp, this.httpGetUrl == null ? new Uri(string.Empty, UriKind.Relative) : this.httpGetUrl);
            mex.HttpsGetUrl = host.GetVia(Uri.UriSchemeHttps, this.httpsGetUrl == null ? new Uri(string.Empty, UriKind.Relative) : this.httpsGetUrl);

            mex.HttpGetBinding = this.httpGetBinding;
            mex.HttpsGetBinding = this.httpsGetBinding;

            UseRequestHeadersForMetadataAddressBehavior dynamicUpdateBehavior = description.Behaviors.Find<UseRequestHeadersForMetadataAddressBehavior>();
            if (dynamicUpdateBehavior != null)
            {
                mex.UpdateAddressDynamically = true;
                mex.UpdatePortsByScheme = new Dictionary<string, int>(dynamicUpdateBehavior.DefaultPortsByScheme);
            }

            foreach (ChannelDispatcherBase dispatcherBase in host.ChannelDispatchers)
            {
                ChannelDispatcher dispatcher = dispatcherBase as ChannelDispatcher;
                if (dispatcher != null && IsMetadataTransferDispatcher(description, dispatcher))
                {
                    mex.MexEnabled = true;
                    mex.MexUrl = dispatcher.Listener.Uri;
                    if (dynamicUpdateBehavior != null)
                    {
                        foreach (EndpointDispatcher endpointDispatcher in dispatcher.Endpoints)
                        {
                            if (!endpointDispatcher.AddressFilterSetExplicit)
                            {
                                endpointDispatcher.AddressFilter = new MatchAllMessageFilter();
                            }
                        }
                    }
                    break;
                }
            }

        }

        private static void CustomizeMetadataEndpoints(ServiceDescription description, ServiceHostBase host, ServiceMetadataExtension mex)
        {
            for (int i = 0; i < host.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = host.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null && ServiceMetadataBehavior.IsMetadataTransferDispatcher(description, channelDispatcher))
                {
                    if (channelDispatcher.Endpoints.Count != 1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.SFxServiceMetadataBehaviorInstancingError, channelDispatcher.Listener.Uri, channelDispatcher.CreateContractListString())));
                    }

                    DispatchRuntime dispatcher = channelDispatcher.Endpoints[0].DispatchRuntime;

                    // set instancing
                    dispatcher.InstanceContextProvider =
                       InstanceContextProviderBase.GetProviderForMode(InstanceContextMode.Single, dispatcher);

                    bool isListeningOnHttps = channelDispatcher.Listener.Uri.Scheme == Uri.UriSchemeHttps;
                    Uri listenUri = channelDispatcher.Listener.Uri;
                    ServiceMetadataExtension.WSMexImpl impl = new ServiceMetadataExtension.WSMexImpl(mex, isListeningOnHttps, listenUri);
                    dispatcher.SingletonInstanceContext = new InstanceContext(host, impl, false);
                }
            }
        }

        static EndpointDispatcher GetListenerByID(SynchronizedCollection<ChannelDispatcherBase> channelDispatchers, string id)
        {
            for (int i = 0; i < channelDispatchers.Count; ++i)
            {
                ChannelDispatcher channelDispatcher = channelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    for (int j = 0; j < channelDispatcher.Endpoints.Count; ++j)
                    {
                        EndpointDispatcher endpointDispatcher = channelDispatcher.Endpoints[j];
                        if (endpointDispatcher.Id == id)
                            return endpointDispatcher;
                    }
                }
            }
            return null;
        }

        internal static bool IsMetadataDispatcher(ServiceDescription description, ChannelDispatcher channelDispatcher)
        {
            foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
            {
                if (IsMetadataTransferDispatcher(description, channelDispatcher)
                    || IsHttpGetMetadataDispatcher(description, channelDispatcher))
                    return true;
            }
            return false;
        }

        static bool IsMetadataTransferDispatcher(ServiceDescription description, ChannelDispatcher channelDispatcher)
        {
            if (BehaviorMissingObjectNullOrServiceImplements(description, channelDispatcher))
                return false;

            foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
            {
                if (endpointDispatcher.ContractName == ServiceMetadataBehavior.MexContractName
                    && endpointDispatcher.ContractNamespace == ServiceMetadataBehavior.MexContractNamespace)
                    return true;
            }
            return false;
        }

        private static bool BehaviorMissingObjectNullOrServiceImplements(ServiceDescription description, object obj)
        {
            if (obj == null)
                return true;
            if (description.Behaviors != null && description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                return true;
            if (description.ServiceType != null && description.ServiceType.GetInterface(typeof(IMetadataExchange).Name) != null)
                return true;

            return false;
        }

        internal static bool IsHttpGetMetadataDispatcher(ServiceDescription description, ChannelDispatcher channelDispatcher)
        {
            if (description.Behaviors.Find<ServiceMetadataBehavior>() == null)
                return false;

            foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
            {
                if (endpointDispatcher.ContractName == ServiceMetadataExtension.HttpGetImpl.ContractName
                    && endpointDispatcher.ContractNamespace == ServiceMetadataExtension.HttpGetImpl.ContractNamespace)
                    return true;
            }
            return false;
        }

        internal static bool IsMetadataEndpoint(ServiceDescription description, ServiceEndpoint endpoint)
        {
            if (BehaviorMissingObjectNullOrServiceImplements(description, endpoint))
                return false;

            return IsMetadataEndpoint(endpoint);

        }

        static bool IsMetadataEndpoint(ServiceEndpoint endpoint)
        {
            return (endpoint.Contract.Name == ServiceMetadataBehavior.MexContractName
                    && endpoint.Contract.Namespace == ServiceMetadataBehavior.MexContractNamespace);
        }

        internal static bool IsMetadataImplementedType(ServiceDescription description, Type type)
        {
            if (BehaviorMissingObjectNullOrServiceImplements(description, type))
                return false;

            return type == typeof(IMetadataExchange);
        }

        internal static bool IsMetadataImplementedType(Type type)
        {
            return type == typeof(IMetadataExchange);
        }

        internal void AddImplementedContracts(ServiceHostBase.ServiceAndBehaviorsContractResolver resolver)
        {
            if (!resolver.BehaviorContracts.ContainsKey(MexContractName))
            {
                resolver.BehaviorContracts.Add(MexContractName, ServiceMetadataBehavior.MexContract);
            }
        }

        static void EnsureMexContractDescription()
        {
            if (ServiceMetadataBehavior.mexContract == null)
            {
                lock (thisLock)
                {
                    if (ServiceMetadataBehavior.mexContract == null)
                    {
                        ServiceMetadataBehavior.mexContract = CreateMexContract();
                    }
                }
            }
        }

        static ContractDescription CreateMexContract()
        {
            ContractDescription mexContract = ContractDescription.GetContract(typeof(IMetadataExchange));
            foreach (OperationDescription operation in mexContract.Operations)
            {
                operation.Behaviors.Find<OperationBehaviorAttribute>().Impersonation = ImpersonationOption.Allowed;
            }
            mexContract.Behaviors.Add(new ServiceMetadataContractBehavior(true));

            return mexContract;

        }

        internal class MetadataExtensionInitializer
        {
            ServiceMetadataBehavior behavior;
            ServiceDescription description;
            ServiceHostBase host;
            Exception metadataGenerationException = null;

            internal MetadataExtensionInitializer(ServiceMetadataBehavior behavior, ServiceDescription description, ServiceHostBase host)
            {
                this.behavior = behavior;
                this.description = description;
                this.host = host;
            }

            internal MetadataSet GenerateMetadata()
            {
                if (this.behavior.ExternalMetadataLocation == null || this.behavior.ExternalMetadataLocation.ToString() == string.Empty)
                {
                    if (this.metadataGenerationException != null)
                        throw this.metadataGenerationException;

                    try
                    {
                        MetadataExporter exporter = this.behavior.MetadataExporter;
                        XmlQualifiedName serviceName = new XmlQualifiedName(this.description.Name, this.description.Namespace);
                        Collection<ServiceEndpoint> exportedEndpoints = new Collection<ServiceEndpoint>();
                        foreach (ServiceEndpoint endpoint in this.description.Endpoints)
                        {
                            ServiceMetadataContractBehavior contractBehavior = endpoint.Contract.Behaviors.Find<ServiceMetadataContractBehavior>();

                            // if contract behavior exists, generate metadata when the behavior allows metadata generation
                            // if contract behavior doesn't exist, generate metadata only for non system endpoints
                            if ((contractBehavior != null && !contractBehavior.MetadataGenerationDisabled) ||
                                (contractBehavior == null && !endpoint.IsSystemEndpoint))
                            {
                                EndpointAddress address = null;
                                EndpointDispatcher endpointDispatcher = GetListenerByID(this.host.ChannelDispatchers, endpoint.Id);
                                if (endpointDispatcher != null)
                                {
                                    address = endpointDispatcher.EndpointAddress;
                                }
                                ServiceEndpoint exportedEndpoint = new ServiceEndpoint(endpoint.Contract);
                                exportedEndpoint.Binding = endpoint.Binding;
                                exportedEndpoint.Name = endpoint.Name;
                                exportedEndpoint.Address = address;
                                foreach (IEndpointBehavior behavior in endpoint.Behaviors)
                                {
                                    exportedEndpoint.Behaviors.Add(behavior);
                                }
                                exportedEndpoints.Add(exportedEndpoint);
                            }
                        }
                        WsdlExporter wsdlExporter = exporter as WsdlExporter;
                        if (wsdlExporter != null)
                        {
                            // Pass the BindingParameterCollection into the ExportEndpoints method so that the binding parameters can be using to export WSDL correctly.
                            // The binding parameters are used in BuildChannelListener, during which they can modify the configuration of the channel in ways that might have to
                            // be communicated in the WSDL. For example, in the case of Multi-Auth, the AuthenticationSchemesBindingParameter is used during BuildChannelListener
                            // to set the AuthenticationSchemes supported by the virtual directory on the HttpTransportBindingElement.  These authentication schemes also need
                            // to be in the WSDL, so that clients know what authentication schemes are supported by the service.  (see CSDMain #180381)
                            Fx.Assert(this.host != null, "ServiceHostBase field on MetadataExtensionInitializer should never be null.");
                            wsdlExporter.ExportEndpoints(exportedEndpoints, serviceName, this.host.GetBindingParameters(exportedEndpoints));
                        }
                        else
                        {
                            foreach (ServiceEndpoint endpoint in exportedEndpoints)
                            {
                                exporter.ExportEndpoint(endpoint);
                            }
                        }

                        if (exporter.Errors.Count > 0 && DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceWsdlExportErrors(exporter);
                        }

                        return exporter.GetGeneratedMetadata();
                    }
                    catch (Exception e)
                    {
                        this.metadataGenerationException = e;
                        throw;
                    }
                }
                return null;
            }

            static void TraceWsdlExportErrors(MetadataExporter exporter)
            {
                foreach (MetadataConversionError error in exporter.Errors)
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
        }

    }
}
