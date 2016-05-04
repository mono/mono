//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel;

    class SessionSymmetricTransportSecurityProtocolFactory : TransportSecurityProtocolFactory
    {
        SecurityTokenParameters securityTokenParameters;
        SessionDerivedKeySecurityTokenParameters derivedKeyTokenParameters;

        public SessionSymmetricTransportSecurityProtocolFactory()
            : base()
        {
        }

        public override bool SupportsReplayDetection
        {
            get
            {
                return true;
            }
        }

        public SecurityTokenParameters SecurityTokenParameters
        {
            get
            {
                return this.securityTokenParameters;
            }
            set
            {
                ThrowIfImmutable();
                this.securityTokenParameters = value;
            }
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            if (this.ActAsInitiator)
            {
                return new InitiatorSessionSymmetricTransportSecurityProtocol(this, target, via);
            }
            else
            {
                return new AcceptorSessionSymmetricTransportSecurityProtocol(this);
            }
        }

        public override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
            if (this.SecurityTokenParameters == null)
            {
                OnPropertySettingsError("SecurityTokenParameters", true);
            }
            if (this.SecurityTokenParameters.RequireDerivedKeys)
            {
                this.ExpectKeyDerivation = true;
                this.derivedKeyTokenParameters = new SessionDerivedKeySecurityTokenParameters(this.ActAsInitiator);
            }
        }

        internal SecurityTokenParameters GetTokenParameters()
        {
            if (this.derivedKeyTokenParameters != null)
            {
                return this.derivedKeyTokenParameters;
            }
            else
            {
                return this.securityTokenParameters;
            }
        }
    }
}
