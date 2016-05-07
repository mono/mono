//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web.Configuration;

    public class WebServiceHost : ServiceHost
    {
        static readonly Type WebHttpBindingType = typeof(WebHttpBinding);
        static readonly string WebHttpEndpointKind = "webHttpEndpoint";

        public WebServiceHost()
            : base()
        {
        }

        public WebServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {
        }

        public WebServiceHost(Type serviceType, params Uri[] baseAddresses) :
            base(serviceType, baseAddresses)
        {
        }

        // This method adds automatic endpoints at the base addresses, 1 per site binding (http or https). It only configures
        // the security on the binding. It does not add any behaviors.
        // If there are no base addresses, or if endpoints have been configured explicitly, it does not add any
        // automatic endpoints.
        // If it adds automatic endpoints, it validates that the service implements a single contract
        internal static void AddAutomaticWebHttpBindingEndpoints(ServiceHost host, IDictionary<string, ContractDescription> implementedContracts,  string multipleContractsErrorMessage, string noContractErrorMessage, string standardEndpointKind)
        {
            bool enableAutoEndpointCompat = AppSettings.EnableAutomaticEndpointsCompatibility;
            // We do not add an automatic endpoint if an explicit endpoint has been configured unless
            // the user has specifically opted into compat mode.  See CSDMain bugs 176157 & 262728 for history
            if (host.Description.Endpoints != null 
                && host.Description.Endpoints.Count > 0
                && !enableAutoEndpointCompat)
            {
                return;
            }

            AuthenticationSchemes supportedSchemes = AuthenticationSchemes.None;
            if (host.BaseAddresses.Count > 0)
            {
                supportedSchemes = AspNetEnvironment.Current.GetAuthenticationSchemes(host.BaseAddresses[0]);

                if (AspNetEnvironment.Current.IsSimpleApplicationHost)
                {
                    // Cassini always reports the auth scheme as anonymous or Ntlm. Map this to Ntlm, except when forms auth
                    // is requested
                    if (supportedSchemes == (AuthenticationSchemes.Anonymous | AuthenticationSchemes.Ntlm))
                    {
                        if (AspNetEnvironment.Current.IsWindowsAuthenticationConfigured())
                        {
                            supportedSchemes = AuthenticationSchemes.Ntlm;
                        }
                        else
                        {
                            supportedSchemes = AuthenticationSchemes.Anonymous;
                        }
                    }
                }
            }
            Type contractType = null;
            // add an endpoint with the contract at each base address
            foreach (Uri baseAddress in host.BaseAddresses)
            {
                string uriScheme = baseAddress.Scheme;
                
                // HTTP and HTTPs are only supported schemes
                if (Object.ReferenceEquals(uriScheme, Uri.UriSchemeHttp) || Object.ReferenceEquals(uriScheme, Uri.UriSchemeHttps))
                {
                    // bypass adding the automatic endpoint if there's already one at the base address
                    bool isExplicitEndpointConfigured = false;
                    foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                    {
                        if (endpoint.Address != null && EndpointAddress.UriEquals(endpoint.Address.Uri, baseAddress, true, false))
                        {
                            isExplicitEndpointConfigured = true;
                            break;
                        }
                    }
                    if (isExplicitEndpointConfigured)
                    {
                        continue;
                    }

                    if (contractType == null)
                    {
                        if (implementedContracts.Count > 1)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(multipleContractsErrorMessage));
                        }
                        else if (implementedContracts.Count == 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(noContractErrorMessage));
                        }
                        foreach (ContractDescription contract in implementedContracts.Values)
                        {
                            contractType = contract.ContractType;
                            break;
                        }
                    }
                    
                    // Get the default web endpoint
                    ConfigLoader configLoader = new ConfigLoader(host.GetContractResolver(implementedContracts));
                    ServiceEndpointElement serviceEndpointElement = new ServiceEndpointElement();
                    serviceEndpointElement.Contract = contractType.FullName;
                    // Check for a protocol mapping
                    ProtocolMappingItem protocolMappingItem = ConfigLoader.LookupProtocolMapping(baseAddress.Scheme);
                    if (protocolMappingItem != null &&
                        string.Equals(protocolMappingItem.Binding, WebHttpBinding.WebHttpBindingConfigurationStrings.WebHttpBindingCollectionElementName, StringComparison.Ordinal))
                    {
                        serviceEndpointElement.BindingConfiguration = protocolMappingItem.BindingConfiguration;
                    }
                    serviceEndpointElement.Kind = standardEndpointKind;

                    // LookupEndpoint will not set the Endpoint address and listenUri
                    // because omitSettingEndpointAddress is set to true.
                    // We will set them after setting the binding security
                    ServiceEndpoint automaticEndpoint = configLoader.LookupEndpoint(serviceEndpointElement, null, host, host.Description, true /*omitSettingEndpointAddress*/);
                    WebHttpBinding binding = automaticEndpoint.Binding as WebHttpBinding;
                                    
                    bool automaticallyConfigureSecurity = !binding.Security.IsModeSet;
                    if (automaticallyConfigureSecurity)
                    {
                        if (Object.ReferenceEquals(uriScheme, Uri.UriSchemeHttps))
                        {
                            binding.Security.Mode = WebHttpSecurityMode.Transport;
                        }
                        else if (supportedSchemes != AuthenticationSchemes.None && supportedSchemes != AuthenticationSchemes.Anonymous)
                        {
                            binding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                        }
                        else
                        {
                            binding.Security.Mode = WebHttpSecurityMode.None;
                        }
                    }
                    
                    if (automaticallyConfigureSecurity && AspNetEnvironment.Enabled)
                    {
                        SetBindingCredentialBasedOnHostedEnvironment(automaticEndpoint, supportedSchemes);
                    }

                    // Setting the Endpoint address and listenUri now that we've set the binding security
                    ConfigLoader.ConfigureEndpointAddress(serviceEndpointElement, host, automaticEndpoint);
                    ConfigLoader.ConfigureEndpointListenUri(serviceEndpointElement, host, automaticEndpoint);

                    host.AddServiceEndpoint(automaticEndpoint);
                }
            }
        }

        internal static void SetRawContentTypeMapperIfNecessary(ServiceEndpoint endpoint, bool isDispatch)
        {
            Binding binding = endpoint.Binding;
            ContractDescription contract = endpoint.Contract;
            if (binding == null)
            {
                return;
            }
            CustomBinding customBinding = new CustomBinding(binding);
            BindingElementCollection bec = customBinding.Elements;
            WebMessageEncodingBindingElement encodingElement = bec.Find<WebMessageEncodingBindingElement>();
            if (encodingElement == null || encodingElement.ContentTypeMapper != null)
            {
                return;
            }
            bool areAllOperationsRawMapperCompatible = true;
            int numStreamOperations = 0;
            foreach (OperationDescription operation in contract.Operations)
            {
                bool isCompatible = (isDispatch) ? IsRawContentMapperCompatibleDispatchOperation(operation, ref numStreamOperations) : IsRawContentMapperCompatibleClientOperation(operation, ref numStreamOperations);
                if (!isCompatible)
                {
                    areAllOperationsRawMapperCompatible = false;
                    break;
                }
            }
            if (areAllOperationsRawMapperCompatible && numStreamOperations > 0)
            {
                encodingElement.ContentTypeMapper = RawContentTypeMapper.Instance;
                endpoint.Binding = customBinding;
            }
        }

        protected override void OnOpening()
        {
            if (this.Description == null)
            {
                return;
            }
            
            // disable other things that listen for GET at base address and may conflict with auto-endpoints
            ServiceDebugBehavior sdb = this.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb != null)
            {
                sdb.HttpHelpPageEnabled = false;
                sdb.HttpsHelpPageEnabled = false;
            }
            ServiceMetadataBehavior smb = this.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb != null)
            {
                smb.HttpGetEnabled = false;
                smb.HttpsGetEnabled = false;
            }

            AddAutomaticWebHttpBindingEndpoints(this, this.ImplementedContracts, SR2.GetString(SR2.HttpTransferServiceHostMultipleContracts, this.Description.Name), SR2.GetString(SR2.HttpTransferServiceHostNoContract, this.Description.Name), WebHttpEndpointKind);

            // for both user-defined and automatic endpoints, ensure they have the right behavior and content type mapper added
            foreach (ServiceEndpoint serviceEndpoint in this.Description.Endpoints)
            {
                if (serviceEndpoint.Binding != null && serviceEndpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>() != null)
                {
                    SetRawContentTypeMapperIfNecessary(serviceEndpoint, true);
                    if (serviceEndpoint.Behaviors.Find<WebHttpBehavior>() == null)
                    {
                        ConfigLoader.LoadDefaultEndpointBehaviors(serviceEndpoint);
                        if (serviceEndpoint.Behaviors.Find<WebHttpBehavior>() == null)
                        {
                            serviceEndpoint.Behaviors.Add(new WebHttpBehavior());
                        }
                    }
                }
            }

            base.OnOpening();
        }

        static bool IsRawContentMapperCompatibleClientOperation(OperationDescription operation, ref int numStreamOperations)
        {
            // An operation is raw encoder compatible on the client side iff the response is a Stream or void
            // The request is driven by the format property on the message and not by the content type 
            if (operation.Messages.Count > 1 & !IsResponseStreamOrVoid(operation, ref numStreamOperations))
            {
                return false;
            }
            return true;

        }

        static bool IsRawContentMapperCompatibleDispatchOperation(OperationDescription operation, ref int numStreamOperations)
        {
            // An operation is raw encoder compatible on the dispatch side iff the request body is a Stream or void
            // The response is driven by the format property on the message and not by the content type 
            UriTemplateDispatchFormatter throwAway = new UriTemplateDispatchFormatter(operation, null, new QueryStringConverter(), operation.DeclaringContract.Name, new Uri("http://localhost"));
            int numUriVariables = throwAway.pathMapping.Count + throwAway.queryMapping.Count;
            bool isRequestCompatible = false;
            if (numUriVariables > 0)
            {
                // we need the local variable tmp because ref parameters are not allowed to be passed into
                // anonymous methods by the compiler.
                int tmp = 0;
                WebHttpBehavior.HideRequestUriTemplateParameters(operation, throwAway, delegate()
                {
                    isRequestCompatible = IsRequestStreamOrVoid(operation, ref tmp);
                });
                numStreamOperations += tmp;
            }
            else
            {
                isRequestCompatible = IsRequestStreamOrVoid(operation, ref numStreamOperations);
            }
            return isRequestCompatible;
        }

        static bool IsRequestStreamOrVoid(OperationDescription operation, ref int numStreamOperations)
        {
            MessageDescription message = operation.Messages[0];
            if (WebHttpBehavior.IsTypedMessage(message) || WebHttpBehavior.IsUntypedMessage(message))
            {
                return false;
            }
            if (message.Body.Parts.Count == 0)
            {
                return true;
            }
            else if (message.Body.Parts.Count == 1)
            {
                if (IsStreamPart(message.Body.Parts[0].Type))
                {
                    ++numStreamOperations;
                    return true;
                }
                else if (IsVoidPart(message.Body.Parts[0].Type))
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsResponseStreamOrVoid(OperationDescription operation, ref int numStreamOperations)
        {
            if (operation.Messages.Count <= 1)
            {
                return true;
            }
            MessageDescription message = operation.Messages[1];
            if (WebHttpBehavior.IsTypedMessage(message) || WebHttpBehavior.IsUntypedMessage(message))
            {
                return false;
            }
            if (message.Body.Parts.Count == 0)
            {
                if (message.Body.ReturnValue == null || IsVoidPart(message.Body.ReturnValue.Type))
                {
                    return true;
                }
                else if (IsStreamPart(message.Body.ReturnValue.Type))
                {
                    ++numStreamOperations;
                    return true;
                }
            }
            return false;
        }

        static bool IsStreamPart(Type type)
        {
            return (type == typeof(Stream));
        }

        static bool IsVoidPart(Type type)
        {
            return (type == null || type == typeof(void));
        }

        // For automatic endpoints, in the hosted case we configure a credential type based on the vdir settings.
        // For IIS, in IntegratedWindowsAuth mode we pick Negotiate.
        static void SetBindingCredentialBasedOnHostedEnvironment(ServiceEndpoint serviceEndpoint, AuthenticationSchemes supportedSchemes)
        {
            WebHttpBinding whb = serviceEndpoint.Binding as WebHttpBinding;
            Fx.Assert(whb != null, "Automatic endpoint must be WebHttpBinding");
            
            switch (supportedSchemes)
            {
                case AuthenticationSchemes.Digest:
                    whb.Security.Transport.ClientCredentialType = HttpClientCredentialType.Digest;
                    break;
                case AuthenticationSchemes.IntegratedWindowsAuthentication:
                // fall through to Negotiate
                case AuthenticationSchemes.Negotiate:
                    whb.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                    break;
                case AuthenticationSchemes.Ntlm:
                    whb.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                    break;
                case AuthenticationSchemes.Basic:
                    whb.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                    break;
                case AuthenticationSchemes.Anonymous:
                    whb.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    break;
                default:
                    whb.Security.Transport.ClientCredentialType = HttpClientCredentialType.InheritedFromHost;
                    break;
            }
            
        }
    }
}
