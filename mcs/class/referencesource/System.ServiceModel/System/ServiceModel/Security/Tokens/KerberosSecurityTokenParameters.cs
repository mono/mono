//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.ServiceModel.Security;
    using System.ServiceModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    public class KerberosSecurityTokenParameters : SecurityTokenParameters
    {
        protected KerberosSecurityTokenParameters(KerberosSecurityTokenParameters other)
            : base(other)
        {
            // empty
        }

        public KerberosSecurityTokenParameters()
            : base()
        {
            this.InclusionMode = SecurityTokenInclusionMode.Once;
        }

        internal protected override bool HasAsymmetricKey { get { return false; } }
        internal protected override bool SupportsClientAuthentication { get { return true; } }
        internal protected override bool SupportsServerAuthentication { get { return true; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return true; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new KerberosSecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            return base.CreateKeyIdentifierClause<KerberosTicketHashKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = SecurityTokenTypes.Kerberos;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.RequireCryptographicToken = true;
        }
    }
}
