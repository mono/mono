//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security.Tokens;

    class SessionSymmetricMessageSecurityProtocolFactory : MessageSecurityProtocolFactory
    {
        SecurityTokenParameters securityTokenParameters;
        SessionDerivedKeySecurityTokenParameters derivedKeyTokenParameters;

        public SessionSymmetricMessageSecurityProtocolFactory()
            : base()
        {
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

        public override EndpointIdentity GetIdentityOfSelf()
        {
            if (this.SecurityTokenManager is IEndpointIdentityProvider)
            {
                SecurityTokenRequirement requirement = CreateRecipientSecurityTokenRequirement();
                this.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
                return ((IEndpointIdentityProvider)this.SecurityTokenManager).GetIdentityOfSelf(requirement);
            }
            else
            {
                return base.GetIdentityOfSelf();
            }
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            if (this.ActAsInitiator)
            {
                return new InitiatorSessionSymmetricMessageSecurityProtocol(this, target, via);
            }
            else
            {
                return new AcceptorSessionSymmetricMessageSecurityProtocol(this, null);
            }
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.SecurityTokenParameters == null)
            {
                OnPropertySettingsError("SecurityTokenParameters", true);
            }
            if (this.SecurityTokenParameters.RequireDerivedKeys)
            {
                this.ExpectKeyDerivation = true;
                this.derivedKeyTokenParameters = new SessionDerivedKeySecurityTokenParameters(this.ActAsInitiator);
            }
            base.OnOpen(timeout);
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

    internal class SessionDerivedKeySecurityTokenParameters : SecurityTokenParameters
    {
        bool actAsInitiator;

        protected SessionDerivedKeySecurityTokenParameters(SessionDerivedKeySecurityTokenParameters other)
            : base(other)
        {
            this.actAsInitiator = other.actAsInitiator;
        }

        public SessionDerivedKeySecurityTokenParameters(bool actAsInitiator)
            : base()
        {
            this.actAsInitiator = actAsInitiator;
            this.InclusionMode = actAsInitiator ? SecurityTokenInclusionMode.AlwaysToRecipient : SecurityTokenInclusionMode.AlwaysToInitiator;
            base.RequireDerivedKeys = false;
        }

        internal protected override bool SupportsClientAuthentication { get { return false; } }
        internal protected override bool SupportsServerAuthentication { get { return false; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return false; } }

        internal protected override bool HasAsymmetricKey { get { return false; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SessionDerivedKeySecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (referenceStyle == SecurityTokenReferenceStyle.Internal)
            {
                return token.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();
            }
            else
            {
                return null;
            }
        }

        internal protected override bool MatchesKeyIdentifierClause(SecurityToken token, SecurityKeyIdentifierClause keyIdentifierClause, SecurityTokenReferenceStyle referenceStyle)
        {
            if (referenceStyle == SecurityTokenReferenceStyle.Internal)
            {
                LocalIdKeyIdentifierClause localClause = keyIdentifierClause as LocalIdKeyIdentifierClause;
                if (localClause == null)
                {
                    return false;
                }
                else
                {
                    return (localClause.LocalId == token.Id);
                }
            }
            else
            {
                return false;
            }
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }
    }
}
