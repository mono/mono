//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Globalization;

    public class SslSecurityTokenParameters : SecurityTokenParameters
    {
        internal const bool defaultRequireClientCertificate = false;
        internal const bool defaultRequireCancellation = false;

        bool requireCancellation = defaultRequireCancellation;
        bool requireClientCertificate;
        BindingContext issuerBindingContext;

        protected SslSecurityTokenParameters(SslSecurityTokenParameters other)
            : base(other)
        {
            this.requireClientCertificate = other.requireClientCertificate;
            this.requireCancellation = other.requireCancellation;
            if (other.issuerBindingContext != null)
            {
                this.issuerBindingContext = other.issuerBindingContext.Clone();
            }
        }

        public SslSecurityTokenParameters()
            : this(defaultRequireClientCertificate)
        {
            // empty
        }

        public SslSecurityTokenParameters(bool requireClientCertificate)
            : this(requireClientCertificate, defaultRequireCancellation)
        {
            // empty
        }

        public SslSecurityTokenParameters(bool requireClientCertificate, bool requireCancellation)
            : base()
        {
            this.requireClientCertificate = requireClientCertificate;
            this.requireCancellation = requireCancellation;
        }

        internal protected override bool HasAsymmetricKey { get { return false; } }

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

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
            set
            {
                this.requireClientCertificate = value;
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

        internal protected override bool SupportsClientAuthentication { get { return this.requireClientCertificate; } }
        internal protected override bool SupportsServerAuthentication { get { return true; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return this.requireClientCertificate; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SslSecurityTokenParameters(this);
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
            requirement.TokenType = (this.RequireClientCertificate) ? ServiceModelSecurityTokenTypes.MutualSslnego : ServiceModelSecurityTokenTypes.AnonymousSslnego;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.Properties[ServiceModelSecurityTokenRequirement.SupportSecurityContextCancellationProperty] = this.RequireCancellation;
            if (this.IssuerBindingContext != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerBindingContextProperty] = this.IssuerBindingContext.Clone();
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty] = this.Clone();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "RequireCancellation: {0}", this.RequireCancellation.ToString()));
            sb.Append(String.Format(CultureInfo.InvariantCulture, "RequireClientCertificate: {0}", this.RequireClientCertificate.ToString()));

            return sb.ToString();
        }
    }
}
