//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.MsmqIntegration;
    using System.Xml;

    class EndpointInstanceProvider : ProviderBase, IWmiProvider
    {
        internal static string EndpointReference(Uri uri, string contractName)
        {
            return EndpointReference(null != uri ? uri.ToString() : String.Empty, contractName, true);
        }

        internal static string EndpointReference(string address, string contractName, bool local)
        {
            Fx.Assert(!String.IsNullOrEmpty(address), "address cannot be null");

            string endpointReference = String.Format(CultureInfo.InvariantCulture,
                                                    AdministrationStrings.Endpoint +
                                                        "." +
                                                        AdministrationStrings.ListenUri +
                                                        "='{0}'," +
                                                        AdministrationStrings.ContractName +
                                                        "='{1}'," +
                                                        AdministrationStrings.AppDomainId +
                                                        "='{2}'," +
                                                        AdministrationStrings.ProcessId +
                                                        "={3}",
                                                    address,
#pragma warning suppress 56507
 null != contractName ? contractName : String.Empty,
                                                    AppDomainInfo.Current.Id,
                                                    AppDomainInfo.Current.ProcessId);
            if (!local)
            {
                Uri uri;
                if (Uri.TryCreate(address, UriKind.Absolute, out uri))
                {
                    string host = uri.Host;

                    if (!AdministrationStrings.Localhost.Equals(host, StringComparison.OrdinalIgnoreCase)
                        && !AppDomainInfo.Current.MachineName.Equals(host, StringComparison.OrdinalIgnoreCase))
                    {
                        string machineAddress = String.Format(CultureInfo.InvariantCulture,
                            "\\\\{0}\\" + AdministrationStrings.IndigoNamespace + ":",
                            host);
                        endpointReference = machineAddress + endpointReference;
                    }
                }


            }

            return endpointReference;
        }

        static void FillBindingInfo(EndpointInfo endpoint, IWmiInstance instance)
        {
            Fx.Assert(null != endpoint, "");
            Fx.Assert(null != instance, "");
            IWmiInstance binding = instance.NewInstance(AdministrationStrings.Binding);

            IWmiInstance[] bindings = new IWmiInstance[endpoint.Binding.Elements.Count];
            for (int j = 0; j < bindings.Length; ++j)
            {
                bindings[j] = binding;
                FillBindingInfo(endpoint.Binding.Elements[j], ref bindings[j]);
            }
            binding.SetProperty(AdministrationStrings.BindingElements, bindings);
            binding.SetProperty(AdministrationStrings.Name, endpoint.Binding.Name);
            binding.SetProperty(AdministrationStrings.Namespace, endpoint.Binding.Namespace);
            binding.SetProperty(AdministrationStrings.CloseTimeout, endpoint.Binding.CloseTimeout);
            binding.SetProperty(AdministrationStrings.Scheme, endpoint.Binding.Scheme);
            binding.SetProperty(AdministrationStrings.OpenTimeout, endpoint.Binding.OpenTimeout);
            binding.SetProperty(AdministrationStrings.ReceiveTimeout, endpoint.Binding.ReceiveTimeout);
            binding.SetProperty(AdministrationStrings.SendTimeout, endpoint.Binding.SendTimeout);

            instance.SetProperty(AdministrationStrings.Binding, binding);
        }

        static void FillAddressInfo(EndpointInfo endpoint, IWmiInstance instance)
        {
            Fx.Assert(null != endpoint, "");
            Fx.Assert(null != instance, "");
            string[] headers = new string[endpoint.Headers.Count];
            int i = 0;
            foreach (AddressHeader header in endpoint.Headers)
            {
                PlainXmlWriter xmlWriter = new PlainXmlWriter();
                header.WriteAddressHeader(xmlWriter);
                headers[i++] = xmlWriter.ToString();
            }
            ProviderBase.FillCollectionInfo(headers, instance, AdministrationStrings.AddressHeaders);
            instance.SetProperty(AdministrationStrings.Address, endpoint.Address == null ? String.Empty : endpoint.Address.ToString());
            instance.SetProperty(AdministrationStrings.ListenUri, endpoint.ListenUri == null ? String.Empty : endpoint.ListenUri.ToString());
            instance.SetProperty(AdministrationStrings.Identity, endpoint.Identity == null ? String.Empty : endpoint.Identity.ToString());
        }

        static void FillContractInfo(EndpointInfo endpoint, IWmiInstance instance)
        {
            Fx.Assert(null != endpoint, "");
            Fx.Assert(null != instance, "");

            instance.SetProperty(AdministrationStrings.Contract, ContractInstanceProvider.ContractReference(endpoint.Contract.Name));
        }

        internal static void FillEndpointInfo(EndpointInfo endpoint, IWmiInstance instance)
        {
            Fx.Assert(null != endpoint, "");
            Fx.Assert(null != instance, "");
            instance.SetProperty(AdministrationStrings.CounterInstanceName, PerformanceCounters.PerformanceCountersEnabled ? EndpointPerformanceCounters.CreateFriendlyInstanceName(endpoint.ServiceName, endpoint.Contract.Name, endpoint.Address.AbsoluteUri.ToUpperInvariant()) : String.Empty);
            instance.SetProperty(AdministrationStrings.Name, endpoint.Name);
            instance.SetProperty(AdministrationStrings.ContractName, endpoint.Contract.Name);
            FillAddressInfo(endpoint, instance);
            FillContractInfo(endpoint, instance);
            FillBindingInfo(endpoint, instance);
            FillBehaviorsInfo(endpoint, instance);
        }

        static void FillBindingInfo(BindingElement bindingElement, ref IWmiInstance instance)
        {
            Fx.Assert(null != bindingElement, "");
            Fx.Assert(null != instance, "");

            if (bindingElement is IWmiInstanceProvider)
            {
                IWmiInstanceProvider instanceProvider = (IWmiInstanceProvider)bindingElement;
                instance = instance.NewInstance(instanceProvider.GetInstanceType());
                instanceProvider.FillInstance(instance);
                return;
            }

            Type elementType = AdministrationHelpers.GetServiceModelBaseType(bindingElement.GetType());
            if (null != elementType)
            {
                instance = instance.NewInstance(elementType.Name);
                if (bindingElement is TransportBindingElement)
                {
                    TransportBindingElement transport = (TransportBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.ManualAddressing, transport.ManualAddressing);
                    instance.SetProperty(AdministrationStrings.MaxReceivedMessageSize, transport.MaxReceivedMessageSize);
                    instance.SetProperty(AdministrationStrings.MaxBufferPoolSize, transport.MaxBufferPoolSize);
                    instance.SetProperty(AdministrationStrings.Scheme, transport.Scheme);

                    if (bindingElement is ConnectionOrientedTransportBindingElement)
                    {
                        ConnectionOrientedTransportBindingElement connectionOriented = (ConnectionOrientedTransportBindingElement)bindingElement;
                        instance.SetProperty(AdministrationStrings.ConnectionBufferSize, connectionOriented.ConnectionBufferSize);
                        instance.SetProperty(AdministrationStrings.HostNameComparisonMode, connectionOriented.HostNameComparisonMode.ToString());
                        instance.SetProperty(AdministrationStrings.ChannelInitializationTimeout, connectionOriented.ChannelInitializationTimeout);
                        instance.SetProperty(AdministrationStrings.MaxBufferSize, connectionOriented.MaxBufferSize);
                        instance.SetProperty(AdministrationStrings.MaxPendingConnections, connectionOriented.MaxPendingConnections);
                        instance.SetProperty(AdministrationStrings.MaxOutputDelay, connectionOriented.MaxOutputDelay);
                        instance.SetProperty(AdministrationStrings.MaxPendingAccepts, connectionOriented.MaxPendingAccepts);
                        instance.SetProperty(AdministrationStrings.TransferMode, connectionOriented.TransferMode.ToString());

                        if (bindingElement is TcpTransportBindingElement)
                        {
                            TcpTransportBindingElement tcp = (TcpTransportBindingElement)bindingElement;
                            instance.SetProperty(AdministrationStrings.ListenBacklog, tcp.ListenBacklog);
                            instance.SetProperty(AdministrationStrings.PortSharingEnabled, tcp.PortSharingEnabled);
                            instance.SetProperty(AdministrationStrings.TeredoEnabled, tcp.TeredoEnabled);

                            IWmiInstance connectionPool = instance.NewInstance(AdministrationStrings.TcpConnectionPoolSettings);
                            connectionPool.SetProperty(AdministrationStrings.GroupName, tcp.ConnectionPoolSettings.GroupName);
                            connectionPool.SetProperty(AdministrationStrings.IdleTimeout, tcp.ConnectionPoolSettings.IdleTimeout);
                            connectionPool.SetProperty(AdministrationStrings.LeaseTimeout, tcp.ConnectionPoolSettings.LeaseTimeout);
                            connectionPool.SetProperty(AdministrationStrings.MaxOutboundConnectionsPerEndpoint, tcp.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);

                            instance.SetProperty(AdministrationStrings.ConnectionPoolSettings, connectionPool);

                            FillExtendedProtectionPolicy(instance, tcp.ExtendedProtectionPolicy);
                        }
                        else if (bindingElement is NamedPipeTransportBindingElement)
                        {
                            NamedPipeTransportBindingElement namedPipe = (NamedPipeTransportBindingElement)bindingElement;
                            IWmiInstance connectionPool = instance.NewInstance(AdministrationStrings.NamedPipeConnectionPoolSettings);

                            connectionPool.SetProperty(AdministrationStrings.GroupName, namedPipe.ConnectionPoolSettings.GroupName);
                            connectionPool.SetProperty(AdministrationStrings.IdleTimeout, namedPipe.ConnectionPoolSettings.IdleTimeout);
                            connectionPool.SetProperty(AdministrationStrings.MaxOutboundConnectionsPerEndpoint, namedPipe.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);

                            instance.SetProperty(AdministrationStrings.ConnectionPoolSettings, connectionPool);
                        }
                    }
                    else if (bindingElement is HttpTransportBindingElement)
                    {
                        HttpTransportBindingElement http = (HttpTransportBindingElement)bindingElement;
                        instance.SetProperty(AdministrationStrings.AllowCookies, http.AllowCookies);
                        instance.SetProperty(AdministrationStrings.AuthenticationScheme, http.AuthenticationScheme.ToString());

                        instance.SetProperty(AdministrationStrings.BypassProxyOnLocal, http.BypassProxyOnLocal);
                        instance.SetProperty(AdministrationStrings.DecompressionEnabled, http.DecompressionEnabled);
                        instance.SetProperty(AdministrationStrings.HostNameComparisonMode, http.HostNameComparisonMode.ToString());
                        instance.SetProperty(AdministrationStrings.KeepAliveEnabled, http.KeepAliveEnabled);
                        instance.SetProperty(AdministrationStrings.MaxBufferSize, http.MaxBufferSize);
                        if (null != http.ProxyAddress)
                        {
                            instance.SetProperty(AdministrationStrings.ProxyAddress, http.ProxyAddress.AbsoluteUri.ToString());
                        }
                        instance.SetProperty(AdministrationStrings.ProxyAuthenticationScheme, http.ProxyAuthenticationScheme.ToString());
                        instance.SetProperty(AdministrationStrings.Realm, http.Realm);
                        instance.SetProperty(AdministrationStrings.TransferMode, http.TransferMode.ToString());
                        instance.SetProperty(AdministrationStrings.UnsafeConnectionNtlmAuthentication, http.UnsafeConnectionNtlmAuthentication);
                        instance.SetProperty(AdministrationStrings.UseDefaultWebProxy, http.UseDefaultWebProxy);

                        FillExtendedProtectionPolicy(instance, http.ExtendedProtectionPolicy);

                        if (bindingElement is HttpsTransportBindingElement)
                        {
                            HttpsTransportBindingElement https = (HttpsTransportBindingElement)bindingElement;
                            instance.SetProperty(AdministrationStrings.RequireClientCertificate, https.RequireClientCertificate);
                        }
                    }
                    else if (bindingElement is MsmqBindingElementBase)
                    {
                        MsmqBindingElementBase msmq = (MsmqBindingElementBase)bindingElement;

                        if (null != msmq.CustomDeadLetterQueue)
                        {
                            instance.SetProperty(AdministrationStrings.CustomDeadLetterQueue, msmq.CustomDeadLetterQueue.AbsoluteUri.ToString());
                        }
                        instance.SetProperty(AdministrationStrings.DeadLetterQueue, msmq.DeadLetterQueue);
                        instance.SetProperty(AdministrationStrings.Durable, msmq.Durable);
                        instance.SetProperty(AdministrationStrings.ExactlyOnce, msmq.ExactlyOnce);
                        instance.SetProperty(AdministrationStrings.MaxRetryCycles, msmq.MaxRetryCycles);
                        instance.SetProperty(AdministrationStrings.ReceiveContextEnabled, msmq.ReceiveContextEnabled);
                        instance.SetProperty(AdministrationStrings.ReceiveErrorHandling, msmq.ReceiveErrorHandling);
                        instance.SetProperty(AdministrationStrings.ReceiveRetryCount, msmq.ReceiveRetryCount);
                        instance.SetProperty(AdministrationStrings.RetryCycleDelay, msmq.RetryCycleDelay);
                        instance.SetProperty(AdministrationStrings.TimeToLive, msmq.TimeToLive);
                        instance.SetProperty(AdministrationStrings.UseSourceJournal, msmq.UseSourceJournal);
                        instance.SetProperty(AdministrationStrings.UseMsmqTracing, msmq.UseMsmqTracing);
                        instance.SetProperty(AdministrationStrings.ValidityDuration, msmq.ValidityDuration);

                        MsmqTransportBindingElement msmqTransport = msmq as MsmqTransportBindingElement;
                        if (null != msmqTransport)
                        {
                            instance.SetProperty(AdministrationStrings.MaxPoolSize, msmqTransport.MaxPoolSize);
                            instance.SetProperty(AdministrationStrings.QueueTransferProtocol, msmqTransport.QueueTransferProtocol);
                            instance.SetProperty(AdministrationStrings.UseActiveDirectory, msmqTransport.UseActiveDirectory);
                        }

                        MsmqIntegrationBindingElement msmqIntegration = msmq as MsmqIntegrationBindingElement;
                        if (null != msmqIntegration)
                            instance.SetProperty(AdministrationStrings.SerializationFormat, msmqIntegration.SerializationFormat.ToString());
                    }
#pragma warning disable 0618
                    else if (bindingElement is PeerTransportBindingElement)
                    {
                        PeerTransportBindingElement peer = (PeerTransportBindingElement)bindingElement;
                        instance.SetProperty(AdministrationStrings.ListenIPAddress, peer.ListenIPAddress);
                        instance.SetProperty(AdministrationStrings.Port, peer.Port);

                        IWmiInstance securitySettings = instance.NewInstance(AdministrationStrings.PeerSecuritySettings);
                        securitySettings.SetProperty(AdministrationStrings.PeerSecurityMode, peer.Security.Mode.ToString());
                        IWmiInstance transportSecuritySettings = securitySettings.NewInstance(AdministrationStrings.PeerTransportSecuritySettings);
                        transportSecuritySettings.SetProperty(AdministrationStrings.PeerTransportCredentialType, peer.Security.Transport.CredentialType.ToString());
                        securitySettings.SetProperty(AdministrationStrings.Transport, transportSecuritySettings);
                        instance.SetProperty(AdministrationStrings.Security, securitySettings);
                    }
                }
                else if (bindingElement is PeerResolverBindingElement)
                {
                    PeerResolverBindingElement baseResolver = (PeerResolverBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.ReferralPolicy, baseResolver.ReferralPolicy.ToString());
                    if (bindingElement is PeerCustomResolverBindingElement)
                    {
                        PeerCustomResolverBindingElement specificElement = (PeerCustomResolverBindingElement)bindingElement;
                        if (specificElement.Address != null)
                            instance.SetProperty(AdministrationStrings.Address, specificElement.Address.ToString());
                        if (specificElement.Binding != null)
                            instance.SetProperty(AdministrationStrings.Binding, specificElement.Binding.ToString());
                    }
                }
#pragma warning restore 0618
                else if (bindingElement is ReliableSessionBindingElement)
                {
                    ReliableSessionBindingElement specificElement = (ReliableSessionBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.AcknowledgementInterval, specificElement.AcknowledgementInterval);
                    instance.SetProperty(AdministrationStrings.FlowControlEnabled, specificElement.FlowControlEnabled);
                    instance.SetProperty(AdministrationStrings.InactivityTimeout, specificElement.InactivityTimeout);
                    instance.SetProperty(AdministrationStrings.MaxPendingChannels, specificElement.MaxPendingChannels);
                    instance.SetProperty(AdministrationStrings.MaxRetryCount, specificElement.MaxRetryCount);
                    instance.SetProperty(AdministrationStrings.MaxTransferWindowSize, specificElement.MaxTransferWindowSize);
                    instance.SetProperty(AdministrationStrings.Ordered, specificElement.Ordered);
                    instance.SetProperty(AdministrationStrings.ReliableMessagingVersion, specificElement.ReliableMessagingVersion.ToString());
                }
                else if (bindingElement is SecurityBindingElement)
                {
                    SecurityBindingElement specificElement = (SecurityBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.AllowInsecureTransport, specificElement.AllowInsecureTransport);
                    instance.SetProperty(AdministrationStrings.DefaultAlgorithmSuite, specificElement.DefaultAlgorithmSuite.ToString());
                    instance.SetProperty(AdministrationStrings.EnableUnsecuredResponse, specificElement.EnableUnsecuredResponse);
                    instance.SetProperty(AdministrationStrings.IncludeTimestamp, specificElement.IncludeTimestamp);
                    instance.SetProperty(AdministrationStrings.KeyEntropyMode, specificElement.KeyEntropyMode.ToString());
                    instance.SetProperty(AdministrationStrings.SecurityHeaderLayout, specificElement.SecurityHeaderLayout.ToString());
                    instance.SetProperty(AdministrationStrings.MessageSecurityVersion, specificElement.MessageSecurityVersion.ToString());

                    IWmiInstance localServiceSecuritySettings = instance.NewInstance(AdministrationStrings.LocalServiceSecuritySettings);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.DetectReplays, specificElement.LocalServiceSettings.DetectReplays);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.InactivityTimeout, specificElement.LocalServiceSettings.InactivityTimeout);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.IssuedCookieLifetime, specificElement.LocalServiceSettings.IssuedCookieLifetime);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.MaxCachedCookies, specificElement.LocalServiceSettings.MaxCachedCookies);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.MaxClockSkew, specificElement.LocalServiceSettings.MaxClockSkew);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.MaxPendingSessions, specificElement.LocalServiceSettings.MaxPendingSessions);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.MaxStatefulNegotiations, specificElement.LocalServiceSettings.MaxStatefulNegotiations);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.NegotiationTimeout, specificElement.LocalServiceSettings.NegotiationTimeout);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.ReconnectTransportOnFailure, specificElement.LocalServiceSettings.ReconnectTransportOnFailure);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.ReplayCacheSize, specificElement.LocalServiceSettings.ReplayCacheSize);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.ReplayWindow, specificElement.LocalServiceSettings.ReplayWindow);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.SessionKeyRenewalInterval, specificElement.LocalServiceSettings.SessionKeyRenewalInterval);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.SessionKeyRolloverInterval, specificElement.LocalServiceSettings.SessionKeyRolloverInterval);
                    localServiceSecuritySettings.SetProperty(AdministrationStrings.TimestampValidityDuration, specificElement.LocalServiceSettings.TimestampValidityDuration);
                    instance.SetProperty(AdministrationStrings.LocalServiceSecuritySettings, localServiceSecuritySettings);

                    if (bindingElement is AsymmetricSecurityBindingElement)
                    {
                        AsymmetricSecurityBindingElement specificElement1 = (AsymmetricSecurityBindingElement)bindingElement;

                        instance.SetProperty(AdministrationStrings.MessageProtectionOrder, specificElement1.MessageProtectionOrder.ToString());
                        instance.SetProperty(AdministrationStrings.RequireSignatureConfirmation, specificElement1.RequireSignatureConfirmation);
                    }
                    else if (bindingElement is SymmetricSecurityBindingElement)
                    {
                        SymmetricSecurityBindingElement specificElement1 = (SymmetricSecurityBindingElement)bindingElement;

                        instance.SetProperty(AdministrationStrings.MessageProtectionOrder, specificElement1.MessageProtectionOrder.ToString());
                        instance.SetProperty(AdministrationStrings.RequireSignatureConfirmation, specificElement1.RequireSignatureConfirmation);
                    }
                }
                else if (bindingElement is WindowsStreamSecurityBindingElement)
                {
                    WindowsStreamSecurityBindingElement specificElement
                        = (WindowsStreamSecurityBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.ProtectionLevel, specificElement.ProtectionLevel.ToString());
                }
                else if (bindingElement is SslStreamSecurityBindingElement)
                {
                    SslStreamSecurityBindingElement specificElement = (SslStreamSecurityBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.RequireClientCertificate, specificElement.RequireClientCertificate);
                }
                else if (bindingElement is CompositeDuplexBindingElement)
                {
                    CompositeDuplexBindingElement specificElement = (CompositeDuplexBindingElement)bindingElement;
                    if (specificElement.ClientBaseAddress != null)
                    {
                        instance.SetProperty(AdministrationStrings.ClientBaseAddress, specificElement.ClientBaseAddress.AbsoluteUri);
                    }
                }
                else if (bindingElement is OneWayBindingElement)
                {
                    OneWayBindingElement oneWay = (OneWayBindingElement)bindingElement;
                    IWmiInstance channelPoolSettings = instance.NewInstance(AdministrationStrings.ChannelPoolSettings);
                    channelPoolSettings.SetProperty(AdministrationStrings.IdleTimeout, oneWay.ChannelPoolSettings.IdleTimeout);
                    channelPoolSettings.SetProperty(AdministrationStrings.LeaseTimeout, oneWay.ChannelPoolSettings.LeaseTimeout);
                    channelPoolSettings.SetProperty(AdministrationStrings.MaxOutboundChannelsPerEndpoint, oneWay.ChannelPoolSettings.MaxOutboundChannelsPerEndpoint);
                    instance.SetProperty(AdministrationStrings.ChannelPoolSettings, channelPoolSettings);
                    instance.SetProperty(AdministrationStrings.PacketRoutable, oneWay.PacketRoutable);
                    instance.SetProperty(AdministrationStrings.MaxAcceptedChannels, oneWay.MaxAcceptedChannels);
                }
                else if (bindingElement is MessageEncodingBindingElement)
                {
                    MessageEncodingBindingElement encodingElement = (MessageEncodingBindingElement)bindingElement;

                    instance.SetProperty(AdministrationStrings.MessageVersion, encodingElement.MessageVersion.ToString());

                    if (bindingElement is BinaryMessageEncodingBindingElement)
                    {
                        BinaryMessageEncodingBindingElement specificElement = (BinaryMessageEncodingBindingElement)bindingElement;
                        instance.SetProperty(AdministrationStrings.MaxSessionSize, specificElement.MaxSessionSize);
                        instance.SetProperty(AdministrationStrings.MaxReadPoolSize, specificElement.MaxReadPoolSize);
                        instance.SetProperty(AdministrationStrings.MaxWritePoolSize, specificElement.MaxWritePoolSize);
                        if (null != specificElement.ReaderQuotas)
                        {
                            FillReaderQuotas(instance, specificElement.ReaderQuotas);
                        }
                        instance.SetProperty(AdministrationStrings.CompressionFormat, specificElement.CompressionFormat.ToString());
                    }
                    else if (bindingElement is TextMessageEncodingBindingElement)
                    {
                        TextMessageEncodingBindingElement specificElement = (TextMessageEncodingBindingElement)bindingElement;
                        instance.SetProperty(AdministrationStrings.Encoding, specificElement.WriteEncoding.WebName);
                        instance.SetProperty(AdministrationStrings.MaxReadPoolSize, specificElement.MaxReadPoolSize);
                        instance.SetProperty(AdministrationStrings.MaxWritePoolSize, specificElement.MaxWritePoolSize);
                        if (null != specificElement.ReaderQuotas)
                        {
                            FillReaderQuotas(instance, specificElement.ReaderQuotas);
                        }
                    }
                    else if (bindingElement is MtomMessageEncodingBindingElement)
                    {
                        MtomMessageEncodingBindingElement specificElement = (MtomMessageEncodingBindingElement)bindingElement;
                        instance.SetProperty(AdministrationStrings.Encoding, specificElement.WriteEncoding.WebName);
                        instance.SetProperty(AdministrationStrings.MessageVersion, specificElement.MessageVersion.ToString());
                        instance.SetProperty(AdministrationStrings.MaxReadPoolSize, specificElement.MaxReadPoolSize);
                        instance.SetProperty(AdministrationStrings.MaxWritePoolSize, specificElement.MaxWritePoolSize);
                        if (null != specificElement.ReaderQuotas)
                        {
                            FillReaderQuotas(instance, specificElement.ReaderQuotas);
                        }
                    }
                }
                else if (bindingElement is TransactionFlowBindingElement)
                {
                    TransactionFlowBindingElement specificElement = (TransactionFlowBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.TransactionFlow, specificElement.Transactions);
                    instance.SetProperty(AdministrationStrings.TransactionProtocol, specificElement.TransactionProtocol.ToString());
                    instance.SetProperty(AdministrationStrings.AllowWildcardAction, specificElement.AllowWildcardAction);
                }
                else if (bindingElement is PrivacyNoticeBindingElement)
                {
                    PrivacyNoticeBindingElement specificElement = (PrivacyNoticeBindingElement)bindingElement;
                    instance.SetProperty(AdministrationStrings.Url, specificElement.Url.ToString());
                    instance.SetProperty(AdministrationStrings.PrivacyNoticeVersion, specificElement.Version);
                }
            }
        }

        static void FillBehaviorsInfo(EndpointInfo info, IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            List<IWmiInstance> behaviors = new List<IWmiInstance>(info.Behaviors.Count);
            foreach (IEndpointBehavior behavior in info.Behaviors)
            {
                IWmiInstance behaviorInstance;
                FillBehaviorInfo(behavior, instance, out behaviorInstance);
                if (null != behaviorInstance)
                {
                    behaviors.Add(behaviorInstance);
                }
            }
            instance.SetProperty(AdministrationStrings.Behaviors, behaviors.ToArray());
        }


        static void FillBehaviorInfo(IEndpointBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            Fx.Assert(null != existingInstance, "");
            Fx.Assert(null != behavior, "");
            instance = null;
            if (behavior is ClientCredentials)
            {
                instance = existingInstance.NewInstance("ClientCredentials");
                ClientCredentials specificBehavior = (ClientCredentials)behavior;
                instance.SetProperty(AdministrationStrings.SupportInteractive, specificBehavior.SupportInteractive);
                if (specificBehavior.ClientCertificate != null && specificBehavior.ClientCertificate.Certificate != null)
                {
                    instance.SetProperty(AdministrationStrings.ClientCertificate, specificBehavior.ClientCertificate.Certificate.ToString());
                }
                if (specificBehavior.IssuedToken != null)
                {
                    string result = String.Format(CultureInfo.InvariantCulture,
                                        "{0}: {1}",
                                        AdministrationStrings.CacheIssuedTokens, specificBehavior.IssuedToken.CacheIssuedTokens);
                    instance.SetProperty(AdministrationStrings.IssuedToken, result);
                }
                if (specificBehavior.HttpDigest != null)
                {
                    string result = String.Format(CultureInfo.InvariantCulture,
                                "{0}: {1}",
                                AdministrationStrings.AllowedImpersonationLevel, specificBehavior.HttpDigest.AllowedImpersonationLevel.ToString());
                    instance.SetProperty(AdministrationStrings.HttpDigest, result);
                }
                if (specificBehavior.Peer != null && specificBehavior.Peer.Certificate != null)
                {
                    instance.SetProperty(AdministrationStrings.Peer, specificBehavior.Peer.Certificate.ToString(true));
                }
                if (specificBehavior.UserName != null)
                {
                    instance.SetProperty(AdministrationStrings.UserName, "********");
                }
                if (specificBehavior.Windows != null)
                {

#pragma warning disable 618 // To suppress AllowNtlm warning.
                    string result = String.Format(CultureInfo.InvariantCulture,
                                                    "{0}: {1}, {2}: {3}",
                                                    AdministrationStrings.AllowedImpersonationLevel,
                                                    specificBehavior.Windows.AllowedImpersonationLevel.ToString(),
                                                    AdministrationStrings.AllowNtlm,
                                                    specificBehavior.Windows.AllowNtlm);
#pragma warning restore 618

                    instance.SetProperty(AdministrationStrings.Windows, result);


                }
            }
            else if (behavior is MustUnderstandBehavior)
            {
                instance = existingInstance.NewInstance("MustUnderstandBehavior");
            }
            else if (behavior is SynchronousReceiveBehavior)
            {
                instance = existingInstance.NewInstance("SynchronousReceiveBehavior");
            }
            else if (behavior is DispatcherSynchronizationBehavior)
            {
                instance = existingInstance.NewInstance("DispatcherSynchronizationBehavior");
            }
            else if (behavior is TransactedBatchingBehavior)
            {
                instance = existingInstance.NewInstance("TransactedBatchingBehavior");
                instance.SetProperty(AdministrationStrings.MaxBatchSize, ((TransactedBatchingBehavior)behavior).MaxBatchSize);
            }
            else if (behavior is ClientViaBehavior)
            {
                instance = existingInstance.NewInstance("ClientViaBehavior");
                instance.SetProperty(AdministrationStrings.Uri, ((ClientViaBehavior)behavior).Uri.ToString());
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider instanceProvider = (IWmiInstanceProvider)behavior;
                instance = existingInstance.NewInstance(instanceProvider.GetInstanceType());
                instanceProvider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (null != instance)
            {
                instance.SetProperty(AdministrationStrings.Type, behavior.GetType().FullName);
            }
        }

        static void FillReaderQuotas(IWmiInstance instance, XmlDictionaryReaderQuotas readerQuotas)
        {
            Fx.Assert(null != instance, "");
            Fx.Assert(null != readerQuotas, "");
            IWmiInstance readerQuotasInstance = instance.NewInstance(AdministrationStrings.XmlDictionaryReaderQuotas);
            readerQuotasInstance.SetProperty(AdministrationStrings.MaxArrayLength, readerQuotas.MaxArrayLength);
            readerQuotasInstance.SetProperty(AdministrationStrings.MaxBytesPerRead, readerQuotas.MaxBytesPerRead);
            readerQuotasInstance.SetProperty(AdministrationStrings.MaxDepth, readerQuotas.MaxDepth);
            readerQuotasInstance.SetProperty(AdministrationStrings.MaxNameTableCharCount, readerQuotas.MaxNameTableCharCount);
            readerQuotasInstance.SetProperty(AdministrationStrings.MaxStringContentLength, readerQuotas.MaxStringContentLength);
            instance.SetProperty(AdministrationStrings.ReaderQuotas, readerQuotasInstance);
        }

        static void FillExtendedProtectionPolicy(IWmiInstance instance, ExtendedProtectionPolicy policy)
        {
            IWmiInstance extendedProtectionPolicy = instance.NewInstance(AdministrationStrings.ExtendedProtectionPolicy);
            extendedProtectionPolicy.SetProperty(AdministrationStrings.PolicyEnforcement, policy.PolicyEnforcement.ToString());
            extendedProtectionPolicy.SetProperty(AdministrationStrings.ProtectionScenario, policy.ProtectionScenario.ToString());

            if (policy.CustomServiceNames != null)
            {
                List<string> serviceNames = new List<string>(policy.CustomServiceNames.Count);
                foreach (string serviceName in policy.CustomServiceNames)
                {
                    serviceNames.Add(serviceName);
                }
                extendedProtectionPolicy.SetProperty(AdministrationStrings.CustomServiceNames, serviceNames.ToArray());
            }

            if (policy.CustomChannelBinding != null)
            {
                extendedProtectionPolicy.SetProperty(AdministrationStrings.CustomChannelBinding, policy.CustomChannelBinding.GetType().ToString());
            }

            instance.SetProperty(AdministrationStrings.ExtendedProtectionPolicy, extendedProtectionPolicy);
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            Fx.Assert(null != instances, "");
            int processId = AppDomainInfo.Current.ProcessId;
            int appDomainId = AppDomainInfo.Current.Id;
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                foreach (EndpointInfo endpointInfo in info.Endpoints)
                {
                    IWmiInstance instance = instances.NewInstance(null);

                    instance.SetProperty(AdministrationStrings.ProcessId, processId);
                    instance.SetProperty(AdministrationStrings.AppDomainId, appDomainId);

                    FillEndpointInfo(endpointInfo, instance);

                    instances.AddInstance(instance);
                }
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            Fx.Assert(null != instance, "");
            bool bFound = false;
            if (OwnInstance(instance))
            {
                string address = (string)instance.GetProperty(AdministrationStrings.ListenUri);
                string contractName = (string)instance.GetProperty(AdministrationStrings.ContractName);
                EndpointInfo endpointInfo = FindEndpoint(address, contractName);

                if (null != endpointInfo)
                {
                    FillEndpointInfo(endpointInfo, instance);
                    bFound = true;
                }
            }

            return bFound;
        }

        bool IWmiProvider.InvokeMethod(IWmiMethodContext method)
        {
            bool ownInstance = OwnInstance(method.Instance);

            if (ownInstance)
            {
                if (method.MethodName == AdministrationStrings.GetOperationCounterInstanceName)
                {
                    object argument = method.GetParameter(AdministrationStrings.Operation);
                    string operationName = argument as string;
                    if (String.IsNullOrEmpty(operationName))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidParameterException(AdministrationStrings.Operation));
                    }
                    string result = GetOperationCounterInstanceName(operationName, method.Instance);
                    method.ReturnParameter = result;
                }
                else
                {
                    throw new WbemInvalidMethodException();
                }
            }
            return ownInstance;
        }

        EndpointInfo FindEndpoint(string address, string contractName)
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                foreach (EndpointInfo endpointInfo in info.Endpoints)
                {
                    if (null != endpointInfo.ListenUri && String.Equals(endpointInfo.ListenUri.ToString(), address, StringComparison.OrdinalIgnoreCase)
                        && null != endpointInfo.Contract && null != endpointInfo.Contract.Name && String.CompareOrdinal(endpointInfo.Contract.Name, contractName) == 0)
                    {
                        return endpointInfo;
                    }
                }
            }

            return null;
        }

        string GetOperationCounterInstanceName(string operationName, IWmiInstance endpointInstance)
        {
            Fx.Assert(null != endpointInstance, "");
            string address = (string)endpointInstance.GetProperty(AdministrationStrings.ListenUri);
            string contractName = (string)endpointInstance.GetProperty(AdministrationStrings.ContractName);
            EndpointInfo endpointInfo = FindEndpoint(address, contractName);

            string result = String.Empty;

            if (PerformanceCounters.PerformanceCountersEnabled && null != endpointInfo)
            {
                result = OperationPerformanceCounters.CreateFriendlyInstanceName(endpointInfo.ServiceName, endpointInfo.Contract.Name, operationName, endpointInfo.Address.AbsoluteUri.ToUpperInvariant());
            }

            return result;
        }

        bool OwnInstance(IWmiInstance instance)
        {
            return (int)instance.GetProperty(AdministrationStrings.ProcessId) == AppDomainInfo.Current.ProcessId
                && (int)instance.GetProperty(AdministrationStrings.AppDomainId) == AppDomainInfo.Current.Id;
        }
    }
}
