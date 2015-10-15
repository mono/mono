//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    class RelAssertionDirectKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        string assertionId;

        public RelAssertionDirectKeyIdentifierClause(string assertionId, byte[] derivationNonce, int derivationLength)
            : base(null, derivationNonce, derivationLength)
        {
            if (string.IsNullOrEmpty(assertionId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.AssertionIdCannotBeNullOrEmpty));
            }
            this.assertionId = assertionId;
        }

        public string AssertionId
        {
            get { return this.assertionId; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            RelAssertionDirectKeyIdentifierClause that = keyIdentifierClause as RelAssertionDirectKeyIdentifierClause;

            // PreSharp 
#pragma warning suppress 56506
            return (ReferenceEquals(this, that) || (that != null && that.AssertionId == this.AssertionId));
        }
    }
}
