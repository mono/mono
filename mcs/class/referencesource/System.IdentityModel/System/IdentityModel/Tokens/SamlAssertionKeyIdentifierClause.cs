//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Globalization;

    public class SamlAssertionKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        readonly string assertionId;
        readonly string valueType;
        readonly string tokenTypeUri;
        readonly string binding;
        readonly string location;
        readonly string authorityKind;

        public SamlAssertionKeyIdentifierClause(string assertionId)
            : this(assertionId, null, 0)
        {
        }

        public SamlAssertionKeyIdentifierClause(string assertionId, byte[] derivationNonce, int derivationLength)
            : this(assertionId, derivationNonce, derivationLength, null, null, null, null, null)
        {
        }

        internal SamlAssertionKeyIdentifierClause(string assertionId, byte[] derivationNonce, int derivationLength, string valueType, string tokenTypeUri, string binding, string location, string authorityKind)
            : base(null, derivationNonce, derivationLength)
        {
            if (assertionId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertionId");
            }
            this.assertionId = assertionId;
            this.valueType = valueType;
            this.tokenTypeUri = tokenTypeUri;
            this.binding = binding;
            this.location = location;
            this.authorityKind = authorityKind;
        }

        public string AssertionId
        {
            get { return this.assertionId; }
        }

        internal string TokenTypeUri
        {
            get { return this.tokenTypeUri; }
        }

        internal string ValueType
        {
            get { return this.valueType; }
        }

        internal string Binding
        {
            get { return this.binding; }
        }

        internal string Location
        {
            get { return this.location; }
        }

        internal string AuthorityKind
        {
            get { return this.authorityKind; }
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SamlAssertionKeyIdentifierClause that = keyIdentifierClause as SamlAssertionKeyIdentifierClause;

            // PreSharp Bug: Parameter 'that' to this public method must be validated: A null-dereference can occur here.
            #pragma warning suppress 56506
            return ReferenceEquals(this, that) || (that != null && that.Matches(this.assertionId));
        }

        public bool Matches(string assertionId)
        {
            return this.assertionId == assertionId;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SamlAssertionKeyIdentifierClause(AssertionId = '{0}')", this.AssertionId);
        }
    }
}
