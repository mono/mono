//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Globalization;

    public class X509SecurityTokenParameters : SecurityTokenParameters
    {
        internal const X509KeyIdentifierClauseType defaultX509ReferenceStyle = X509KeyIdentifierClauseType.Any;

        X509KeyIdentifierClauseType x509ReferenceStyle;

        protected X509SecurityTokenParameters(X509SecurityTokenParameters other)
            : base(other)
        {
            this.x509ReferenceStyle = other.x509ReferenceStyle;
        }

        public X509SecurityTokenParameters()
            : this(X509SecurityTokenParameters.defaultX509ReferenceStyle, SecurityTokenParameters.defaultInclusionMode)
        {
            // empty
        }

        public X509SecurityTokenParameters(X509KeyIdentifierClauseType x509ReferenceStyle)
            : this(x509ReferenceStyle, SecurityTokenParameters.defaultInclusionMode)
        {
            // empty
        }

        public X509SecurityTokenParameters(X509KeyIdentifierClauseType x509ReferenceStyle, SecurityTokenInclusionMode inclusionMode)
            : this(x509ReferenceStyle, inclusionMode, SecurityTokenParameters.defaultRequireDerivedKeys)
        {
        }

        internal X509SecurityTokenParameters(X509KeyIdentifierClauseType x509ReferenceStyle, SecurityTokenInclusionMode inclusionMode,
            bool requireDerivedKeys)
            : base()
        {
            this.X509ReferenceStyle = x509ReferenceStyle;
            this.InclusionMode = inclusionMode;
            this.RequireDerivedKeys = requireDerivedKeys;
        }

        internal protected override bool HasAsymmetricKey { get { return true; } }

        public X509KeyIdentifierClauseType X509ReferenceStyle
        {
            get
            {
                return this.x509ReferenceStyle;
            }
            set
            {
                X509SecurityTokenReferenceStyleHelper.Validate(value);
                this.x509ReferenceStyle = value;
            }
        }

        internal protected override bool SupportsClientAuthentication { get { return true; } }
        internal protected override bool SupportsServerAuthentication { get { return true; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return true; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new X509SecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            SecurityKeyIdentifierClause result = null;

            switch (this.x509ReferenceStyle)
            {
                default:
                case X509KeyIdentifierClauseType.Any:
                    if (referenceStyle == SecurityTokenReferenceStyle.External)
                    {
                        X509SecurityToken x509Token = token as X509SecurityToken;
                        if (x509Token != null)
                        {
                            X509SubjectKeyIdentifierClause x509KeyIdentifierClause;
                            if (X509SubjectKeyIdentifierClause.TryCreateFrom(x509Token.Certificate, out x509KeyIdentifierClause))
                            {
                                result = x509KeyIdentifierClause;
                            }
                        }
                        else
                        {
                            X509WindowsSecurityToken windowsX509Token = token as X509WindowsSecurityToken;
                            if (windowsX509Token != null)
                            {
                                X509SubjectKeyIdentifierClause x509KeyIdentifierClause;
                                if (X509SubjectKeyIdentifierClause.TryCreateFrom(windowsX509Token.Certificate, out x509KeyIdentifierClause))
                                {
                                    result = x509KeyIdentifierClause;
                                }
                            }
                        }

                        if (result == null)
                            result = token.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause>();
                        if (result == null)
                            result = token.CreateKeyIdentifierClause<X509ThumbprintKeyIdentifierClause>();
                    }
                    else
                        result = token.CreateKeyIdentifierClause<LocalIdKeyIdentifierClause>();
                    break;
                case X509KeyIdentifierClauseType.Thumbprint:
                    result = this.CreateKeyIdentifierClause<X509ThumbprintKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
                    break; 
                case X509KeyIdentifierClauseType.SubjectKeyIdentifier:
                    result = this.CreateKeyIdentifierClause<X509SubjectKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
                    break;
                case X509KeyIdentifierClauseType.IssuerSerial:
                    result = this.CreateKeyIdentifierClause<X509IssuerSerialKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
                    break;
                case X509KeyIdentifierClauseType.RawDataKeyIdentifier:
                    result = this.CreateKeyIdentifierClause<X509RawDataKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
                    break;
            }

            return result;
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = SecurityTokenTypes.X509Certificate;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = SecurityKeyType.AsymmetricKey;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            sb.Append(String.Format(CultureInfo.InvariantCulture, "X509ReferenceStyle: {0}", this.x509ReferenceStyle.ToString()));

            return sb.ToString();
        }
    }
}
