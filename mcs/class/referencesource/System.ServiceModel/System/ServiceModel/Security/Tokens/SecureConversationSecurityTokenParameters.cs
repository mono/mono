//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Channels;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    
    using System.Text;
    using System.Globalization;

    public class SecureConversationSecurityTokenParameters : SecurityTokenParameters
    {
        internal const bool defaultRequireCancellation = true;
        internal const bool defaultCanRenewSession = true;

        SecurityBindingElement bootstrapSecurityBindingElement;
        ChannelProtectionRequirements bootstrapProtectionRequirements;
        bool requireCancellation;
        bool canRenewSession = defaultCanRenewSession;
        BindingContext issuerBindingContext;

        protected SecureConversationSecurityTokenParameters(SecureConversationSecurityTokenParameters other)
            : base(other)
        {
            this.requireCancellation = other.requireCancellation;
            this.canRenewSession = other.canRenewSession;
            if (other.bootstrapSecurityBindingElement != null)
                this.bootstrapSecurityBindingElement = (SecurityBindingElement)other.bootstrapSecurityBindingElement.Clone();
            if (other.bootstrapProtectionRequirements != null)
                this.bootstrapProtectionRequirements = new ChannelProtectionRequirements(other.bootstrapProtectionRequirements);
            if (other.issuerBindingContext != null)
                this.issuerBindingContext = other.issuerBindingContext.Clone();
        }

        public SecureConversationSecurityTokenParameters()
            : this(null, defaultRequireCancellation, null)
        {
            // empty
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement)
            : this(bootstrapSecurityBindingElement, defaultRequireCancellation, null)
        {
            // empty
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation)
            : this(bootstrapSecurityBindingElement, requireCancellation, true)
        {
            // empty
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation, bool canRenewSession)
            : this(bootstrapSecurityBindingElement, requireCancellation, canRenewSession, null)
        {
            // empty
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation, ChannelProtectionRequirements bootstrapProtectionRequirements)
            : this(bootstrapSecurityBindingElement, requireCancellation, defaultCanRenewSession, null)
        {
            // empty
        }

        public SecureConversationSecurityTokenParameters(SecurityBindingElement bootstrapSecurityBindingElement, bool requireCancellation, bool canRenewSession, ChannelProtectionRequirements bootstrapProtectionRequirements)
            : base()
        {
            this.bootstrapSecurityBindingElement = bootstrapSecurityBindingElement;
            this.canRenewSession = canRenewSession;
            if (bootstrapProtectionRequirements != null)
                this.bootstrapProtectionRequirements = new ChannelProtectionRequirements(bootstrapProtectionRequirements);
            else
            {
                this.bootstrapProtectionRequirements = new ChannelProtectionRequirements();
                this.bootstrapProtectionRequirements.IncomingEncryptionParts.AddParts(new MessagePartSpecification(true));
                this.bootstrapProtectionRequirements.IncomingSignatureParts.AddParts(new MessagePartSpecification(true));
                this.bootstrapProtectionRequirements.OutgoingEncryptionParts.AddParts(new MessagePartSpecification(true));
                this.bootstrapProtectionRequirements.OutgoingSignatureParts.AddParts(new MessagePartSpecification(true));
            }
            this.requireCancellation = requireCancellation;
        }

        internal protected override bool HasAsymmetricKey { get { return false; } }

        public SecurityBindingElement BootstrapSecurityBindingElement
        {
            get
            {
                return this.bootstrapSecurityBindingElement;
            }
            set
            {
                this.bootstrapSecurityBindingElement = value;
            }
        }

        public ChannelProtectionRequirements BootstrapProtectionRequirements
        {
            get
            {
                return this.bootstrapProtectionRequirements;
            }
        }

        internal BindingContext IssuerBindingContext
        {
            get
            {
                return this.issuerBindingContext;
            }
            set
            {
                if (value != null)
                {
                    value = value.Clone();
                }
                this.issuerBindingContext = value;
            }
        }

        ISecurityCapabilities BootstrapSecurityCapabilities
        {
            get
            {
                return this.bootstrapSecurityBindingElement.GetIndividualProperty<ISecurityCapabilities>();
            }
        }

        public bool RequireCancellation
        {
            get
            {
                return this.requireCancellation;
            }
            set
            {
                this.requireCancellation = value;
            }
        }

        public bool CanRenewSession
        {
            get
            {
                return this.canRenewSession;
            }
            set
            {
                this.canRenewSession = value;
            }
        }

        internal protected override bool SupportsClientAuthentication
        {
            get
            {
                return this.BootstrapSecurityCapabilities == null ? false : this.BootstrapSecurityCapabilities.SupportsClientAuthentication;
            }
        }

        internal protected override bool SupportsServerAuthentication
        {
            get
            {
                return this.BootstrapSecurityCapabilities == null ? false : this.BootstrapSecurityCapabilities.SupportsServerAuthentication;
            }
        }

        internal protected override bool SupportsClientWindowsIdentity
        {
            get
            {
                return this.BootstrapSecurityCapabilities == null ? false : this.BootstrapSecurityCapabilities.SupportsClientWindowsIdentity;
            }
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SecureConversationSecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (token is GenericXmlSecurityToken)
                return base.CreateGenericXmlTokenKeyIdentifierClause(token, referenceStyle);
            else
                return this.CreateKeyIdentifierClause<SecurityContextKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = ServiceModelSecurityTokenTypes.SecureConversation;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.RequireCryptographicToken = true;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportSecurityContextCancellationProperty] = this.RequireCancellation;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SecureConversationSecurityBindingElementProperty] = this.BootstrapSecurityBindingElement;
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty] = this.IssuerBindingContext.Clone();
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty] = this.Clone();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "RequireCancellation: {0}", this.requireCancellation.ToString()));
            if (this.bootstrapSecurityBindingElement == null)
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "BootstrapSecurityBindingElement: null"));
            }
            else
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "BootstrapSecurityBindingElement:"));
                sb.AppendLine("  " + this.BootstrapSecurityBindingElement.ToString().Trim().Replace("\n", "\n  "));
            }

            return sb.ToString().Trim();
        }
    }
}
