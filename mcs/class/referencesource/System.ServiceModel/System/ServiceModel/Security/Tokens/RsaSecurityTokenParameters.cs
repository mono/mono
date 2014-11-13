//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="UserNameSecurityTokenParameters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace System.ServiceModel.Security.Tokens
{
    using System.ServiceModel.Security;
    using System.ServiceModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;

    public class RsaSecurityTokenParameters : SecurityTokenParameters
    {
        protected RsaSecurityTokenParameters(RsaSecurityTokenParameters other)
            : base(other)
        {
            this.InclusionMode = SecurityTokenInclusionMode.Never;
        }

        public RsaSecurityTokenParameters()
            : base()
        {
            this.InclusionMode = SecurityTokenInclusionMode.Never;
        }

        internal protected override bool HasAsymmetricKey { get { return true; } }

        internal protected override bool SupportsClientAuthentication { get { return true; } }
        internal protected override bool SupportsServerAuthentication { get { return true; } }
        internal protected override bool SupportsClientWindowsIdentity { get { return false; } }

        protected override SecurityTokenParameters CloneCore()
        {
            return new RsaSecurityTokenParameters(this);
        }

        internal protected override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            return this.CreateKeyIdentifierClause<RsaKeyIdentifierClause, RsaKeyIdentifierClause>(token, referenceStyle);
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = SecurityTokenTypes.Rsa;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = SecurityKeyType.AsymmetricKey;
        }
    }
}
