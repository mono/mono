//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    class SamlAssertionDirectKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        string samlUri;

        public SamlAssertionDirectKeyIdentifierClause(string samlUri, byte[] derivationNonce, int derivationLength)
            : base(null, derivationNonce, derivationLength)
        {
            if (string.IsNullOrEmpty(samlUri))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.SamlUriCannotBeNullOrEmpty));
            }
            this.samlUri = samlUri;
        }

        public string SamlUri
        {
            get { return this.samlUri; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SamlAssertionDirectKeyIdentifierClause that = keyIdentifierClause as SamlAssertionDirectKeyIdentifierClause;

            // PreSharp 
#pragma warning suppress 56506
            return (ReferenceEquals(this, that) || (that != null && that.SamlUri == this.SamlUri));
        }
    }
}
