//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ComponentModel;

    public sealed class WSFederationHttpSecurity
    {
        internal const WSFederationHttpSecurityMode DefaultMode = WSFederationHttpSecurityMode.Message;

        WSFederationHttpSecurityMode mode;
        FederatedMessageSecurityOverHttp messageSecurity;

        public WSFederationHttpSecurity()
            : this(DefaultMode, new FederatedMessageSecurityOverHttp())
        {
        }

        WSFederationHttpSecurity(WSFederationHttpSecurityMode mode, FederatedMessageSecurityOverHttp messageSecurity)
        {
            Fx.Assert(WSFederationHttpSecurityModeHelper.IsDefined(mode), string.Format("Invalid WSFederationHttpSecurityMode value: {0}", mode.ToString()));

            this.mode = mode;
            this.messageSecurity = messageSecurity == null ? new FederatedMessageSecurityOverHttp() : messageSecurity;
        }

        public WSFederationHttpSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!WSFederationHttpSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public FederatedMessageSecurityOverHttp Message
        {
            get { return this.messageSecurity; }
            set { this.messageSecurity = value; }
        }

        internal SecurityBindingElement CreateMessageSecurity(bool isReliableSessionEnabled, MessageSecurityVersion version)
        {
            if (this.mode == WSFederationHttpSecurityMode.Message || this.mode == WSFederationHttpSecurityMode.TransportWithMessageCredential)
            {
                return this.messageSecurity.CreateSecurityBindingElement(this.Mode == WSFederationHttpSecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, version);
            }
            else
            {
                return null;
            }
        }

        internal static bool TryCreate(SecurityBindingElement sbe,
                                       WSFederationHttpSecurityMode mode,
                                       HttpTransportSecurity transportSecurity,
                                       bool isReliableSessionEnabled,
                                       MessageSecurityVersion version,
                                       out WSFederationHttpSecurity security)
        {
            security = null;
            FederatedMessageSecurityOverHttp messageSecurity = null;
            if (sbe == null)
            {
                mode = WSFederationHttpSecurityMode.None;
            }
            else
            {
                mode &= WSFederationHttpSecurityMode.Message | WSFederationHttpSecurityMode.TransportWithMessageCredential;
                Fx.Assert(WSFederationHttpSecurityModeHelper.IsDefined(mode), string.Format("Invalid WSFederationHttpSecurityMode value: {0}", mode.ToString()));

                if (!FederatedMessageSecurityOverHttp.TryCreate(sbe, mode == WSFederationHttpSecurityMode.TransportWithMessageCredential, isReliableSessionEnabled, version, out messageSecurity))
                    return false;
            }
            security = new WSFederationHttpSecurity(mode, messageSecurity);
            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return this.ShouldSerializeMode()
                || this.ShouldSerializeMessage();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMode()
        {
            return this.Mode != DefaultMode;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessage()
        {
            return this.Message.InternalShouldSerialize();
        }
    }
}
