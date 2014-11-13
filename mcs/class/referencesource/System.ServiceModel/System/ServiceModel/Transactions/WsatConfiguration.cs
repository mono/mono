//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Transactions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.IO;
    using System.Net;
    using System.Security;
    using System.ServiceModel.ComIntegration;
    using System.ServiceModel.Security;
    using System.Transactions;

    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;

    class TransactionManagerConfigurationException : TransactionException
    {
        public TransactionManagerConfigurationException(string error, Exception e)
            :
            base(error, e)
        {
        }

        public TransactionManagerConfigurationException(string error)
            :
            base(error)
        {
        }
    }

    class WsatConfiguration
    {
        static readonly string DisabledRegistrationPath;

        const string WsatKey = @"Software\Microsoft\WSAT\3.0";
        const string OleTxUpgradeEnabledValue = "OleTxUpgradeEnabled";
        const bool OleTxUpgradeEnabledDefault = true;

        bool oleTxUpgradeEnabled;

        EndpointAddress localActivationService10;
        EndpointAddress localActivationService11;

        EndpointAddress remoteActivationService10;
        EndpointAddress remoteActivationService11;

        Uri registrationServiceAddress10;
        Uri registrationServiceAddress11;

        bool protocolService10Enabled = false;
        bool protocolService11Enabled = false;
        bool inboundEnabled;

        bool issuedTokensEnabled;
        TimeSpan maxTimeout;

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to BindingStrings are safe.")]
        static WsatConfiguration()
        {
            DisabledRegistrationPath = string.Concat(BindingStrings.AddressPrefix, "/", BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version10), BindingStrings.DisabledSuffix);
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to ProtocolInformationReader.IsV10Enabled and IsV11Enabled are safe.")]
        public WsatConfiguration()
        {
            // Get whereabouts
            WhereaboutsReader whereabouts = GetWhereabouts();

            ProtocolInformationReader protocol = whereabouts.ProtocolInformation;
            if (protocol != null)
            {
                this.protocolService10Enabled = protocol.IsV10Enabled;
                this.protocolService11Enabled = protocol.IsV11Enabled;
            }

            Initialize(whereabouts);

            // Read local registry flag
            this.oleTxUpgradeEnabled = ReadFlag(WsatKey, OleTxUpgradeEnabledValue, OleTxUpgradeEnabledDefault);
        }

        void Initialize(WhereaboutsReader whereabouts)
        {
            // MB 47153: don't throw system exception if whereabouts data is broken
            try
            {
                InitializeForUnmarshal(whereabouts);
                InitializeForMarshal(whereabouts);
            }
            catch (UriFormatException e)
            {
                // UriBuilder.Uri can throw this if the URI is ultimately invalid 
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionManagerConfigurationException(SR.GetString(SR.WsatUriCreationFailed), e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                // UriBuilder constructor can throw this if port < 0
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionManagerConfigurationException(SR.GetString(SR.WsatUriCreationFailed), e));
            }
        }

        public bool OleTxUpgradeEnabled
        {
            get { return this.oleTxUpgradeEnabled; }
        }

        public TimeSpan MaxTimeout
        {
            get { return this.maxTimeout; }
        }

        public bool IssuedTokensEnabled
        {
            get { return this.issuedTokensEnabled; }
        }

        public bool InboundEnabled
        {
            get { return this.inboundEnabled; }
        }

        public bool IsProtocolServiceEnabled(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return this.protocolService10Enabled;

                case ProtocolVersion.Version11:
                    return this.protocolService11Enabled;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.InvalidWsatProtocolVersion)));
            }
        }

        public EndpointAddress LocalActivationService(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return this.localActivationService10;

                case ProtocolVersion.Version11:
                    return this.localActivationService11;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.InvalidWsatProtocolVersion)));
            }
        }


        public EndpointAddress RemoteActivationService(ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return this.remoteActivationService10;

                case ProtocolVersion.Version11:
                    return this.remoteActivationService11;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.InvalidWsatProtocolVersion)));
            }
        }

        public EndpointAddress CreateRegistrationService(AddressHeader refParam, ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new EndpointAddress(this.registrationServiceAddress10, refParam);

                case ProtocolVersion.Version11:
                    return new EndpointAddress(this.registrationServiceAddress11, refParam);

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.InvalidWsatProtocolVersion)));
            }
        }

        public bool IsLocalRegistrationService(EndpointAddress endpoint, ProtocolVersion protocolVersion)
        {
            if (endpoint.Uri == null)
                return false;

            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return endpoint.Uri == this.registrationServiceAddress10;
                case ProtocolVersion.Version11:
                    return endpoint.Uri == this.registrationServiceAddress11;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.InvalidWsatProtocolVersion)));
            }
        }

        public bool IsDisabledRegistrationService(EndpointAddress endpoint)
        {
            return endpoint.Uri.AbsolutePath == DisabledRegistrationPath;
        }

        //
        // Internals
        //

        WhereaboutsReader GetWhereabouts()
        {
            try
            {
                return new WhereaboutsReader(TransactionInterop.GetWhereabouts());
            }
            catch (SerializationException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionManagerConfigurationException(SR.GetString(SR.WhereaboutsReadFailed), e));
            }
            // If GetWhereabouts throws TransactionException, let it propagate
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to the ProtocolInformationReader properties and to BindingStrings.RegistrationCoordinatorSuffix(..) are safe.")]
        void InitializeForUnmarshal(WhereaboutsReader whereabouts)
        {
            ProtocolInformationReader protocol = whereabouts.ProtocolInformation;
            if (protocol != null && protocol.NetworkInboundAccess)
            {
                this.inboundEnabled = true;

                bool isTmLocal = string.Compare(Environment.MachineName,
                                                protocol.NodeName,
                                                StringComparison.OrdinalIgnoreCase) == 0;

                string spnIdentity;

                string activationCoordinatorSuffix10 =
                    BindingStrings.ActivationCoordinatorSuffix(ProtocolVersion.Version10);

                string activationCoordinatorSuffix11 =
                    BindingStrings.ActivationCoordinatorSuffix(ProtocolVersion.Version11);

                if (protocol.IsClustered ||
                   (protocol.NetworkClientAccess && !isTmLocal))
                {
                    if (protocol.IsClustered)
                    {
                        // We cannot reliably perform mutual authentication against a clustered resource
                        // See MB 43523 for more details on this

                        spnIdentity = null;
                    }
                    else
                    {
                        spnIdentity = "host/" + protocol.HostName;
                    }

                    if (protocol.IsV10Enabled)
                    {
                        this.remoteActivationService10 = CreateActivationEndpointAddress(protocol,
                                                                                         activationCoordinatorSuffix10,
                                                                                         spnIdentity,
                                                                                         true);
                    }

                    if (protocol.IsV11Enabled)
                    {
                        this.remoteActivationService11 = CreateActivationEndpointAddress(protocol,
                                                                                         activationCoordinatorSuffix11,
                                                                                         spnIdentity,
                                                                                         true);
                    }
                }

                if (isTmLocal)
                {
                    spnIdentity = "host/" + protocol.NodeName;

                    // The net.pipe Activation endpoint uses the host name as a discriminant
                    // for cluster scenarios with more than one service on a node.
                    if (protocol.IsV10Enabled)
                    {
                        this.localActivationService10 = CreateActivationEndpointAddress(protocol,
                                                                                        activationCoordinatorSuffix10,
                                                                                        spnIdentity,
                                                                                        false);
                    }

                    if (protocol.IsV11Enabled)
                    {
                        this.localActivationService11 = CreateActivationEndpointAddress(protocol,
                                                                                        activationCoordinatorSuffix11,
                                                                                        spnIdentity,
                                                                                        false);
                    }
                }
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to the ProtocolInformationReader properties (HostName, HttpsPort, BasePath) are safe.")]
        EndpointAddress CreateActivationEndpointAddress(ProtocolInformationReader protocol,
                                              string suffix,
                                              string spnIdentity,
                                              bool isRemote)
        {
            string uriScheme;
            string host;
            int port;
            string path;

            if (isRemote)
            {
                uriScheme = Uri.UriSchemeHttps;
                host = protocol.HostName;
                port = protocol.HttpsPort;
                path = protocol.BasePath + "/" + suffix + BindingStrings.RemoteProxySuffix;
            }
            else
            {
                uriScheme = Uri.UriSchemeNetPipe;
                host = "localhost";
                port = -1;
                path = protocol.HostName + "/" + protocol.BasePath + "/" + suffix;
            }

            UriBuilder builder = new UriBuilder(uriScheme, host, port, path);

            if (spnIdentity != null)
            {
                EndpointIdentity identity = EndpointIdentity.CreateSpnIdentity(spnIdentity);
                return new EndpointAddress(builder.Uri, identity);
            }
            else
            {
                return new EndpointAddress(builder.Uri);
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to the ProtocolInformationReader properties and to BindingStrings.RegistrationCoordinatorSuffix(..) are safe.")]
        void InitializeForMarshal(WhereaboutsReader whereabouts)
        {
            ProtocolInformationReader protocol = whereabouts.ProtocolInformation;
            if (protocol != null && protocol.NetworkOutboundAccess)
            {
                // We can marshal outgoing transactions using a valid address
                if (protocol.IsV10Enabled)
                {
                    UriBuilder builder10 = new UriBuilder(Uri.UriSchemeHttps,
                                                          protocol.HostName,
                                                          protocol.HttpsPort,
                                                          protocol.BasePath + "/" +
                                                          BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version10));
                    this.registrationServiceAddress10 = builder10.Uri;
                }

                // when we have a WSAT1.1 coordinator 
                if (protocol.IsV11Enabled)
                {
                    UriBuilder builder11 = new UriBuilder(Uri.UriSchemeHttps,
                                                          protocol.HostName,
                                                          protocol.HttpsPort,
                                                          protocol.BasePath + "/" +
                                                          BindingStrings.RegistrationCoordinatorSuffix(ProtocolVersion.Version11));

                    this.registrationServiceAddress11 = builder11.Uri;
                }

                this.issuedTokensEnabled = protocol.IssuedTokensEnabled;
                this.maxTimeout = protocol.MaxTimeout;
            }
            else
            {
                // Generate an address that will not work
                // We do this in order to generate coordination contexts that can be propagated 
                // between processes on the same node even if WS-AT is disabled
                UriBuilder builder = new UriBuilder(Uri.UriSchemeHttps,
                                                    whereabouts.HostName,
                                                    443,
                                                    DisabledRegistrationPath);

                this.registrationServiceAddress10 = builder.Uri;
                this.registrationServiceAddress11 = builder.Uri;
                this.issuedTokensEnabled = false;
                this.maxTimeout = TimeSpan.FromMinutes(5);
            }
        }

        static object ReadValue(string key, string value)
        {
            try
            {
                using (RegistryHandle regKey = RegistryHandle.GetNativeHKLMSubkey(key, false))
                {
                    if (regKey == null)
                        return null;

                    return regKey.GetValue(value);
                }
            }
            catch (SecurityException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionManagerConfigurationException(SR.GetString(SR.WsatRegistryValueReadError, value), e));
            }
            catch (IOException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new TransactionManagerConfigurationException(SR.GetString(SR.WsatRegistryValueReadError, value), e));
            }
        }

        static int ReadInt(string key, string value, int defaultValue)
        {
            object regValue = ReadValue(key, value);
            if (regValue == null || !(regValue is Int32))
                return defaultValue;

            return (int)regValue;
        }

        static bool ReadFlag(string key, string value, bool defaultValue)
        {
            return (int)ReadInt(key, value, defaultValue ? 1 : 0) != 0;
        }
    }
}
