//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security;

    class SecurityContextSecurityTokenParameters : SecurityTokenParameters
    {
        protected SecurityContextSecurityTokenParameters(SecurityContextSecurityTokenParameters other)
            : base(other)
        {
            // empty
        }

        public SecurityContextSecurityTokenParameters()
            : base()
        {
            this.InclusionMode = SecurityTokenInclusionMode.AlwaysToRecipient;
        }

        internal protected override bool SupportsClientAuthentication { get { return true; } }
        internal protected override bool SupportsServerAuthentication { get { return true; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return true; } }

        internal protected override bool HasAsymmetricKey { get { return false; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new SecurityContextSecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            return base.CreateKeyIdentifierClause<SecurityContextKeyIdentifierClause, LocalIdKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = ServiceModelSecurityTokenTypes.SecurityContext;
            requirement.KeyType = SecurityKeyType.SymmetricKey;
            requirement.RequireCryptographicToken = true;
        }
    }
}
