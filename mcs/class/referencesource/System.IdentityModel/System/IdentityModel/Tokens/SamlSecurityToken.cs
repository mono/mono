//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.CodeDom;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Threading;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Policy;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.IO;

    public class SamlSecurityToken : SecurityToken
    {
        SamlAssertion assertion;

        protected SamlSecurityToken()
        {
        }

        public SamlSecurityToken(SamlAssertion assertion)
        {
            Initialize(assertion);
        }

        protected void Initialize(SamlAssertion assertion)
        {
            if (assertion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");

            this.assertion = assertion;
            this.assertion.MakeReadOnly();
        }

        public override string Id
        {
            get { return this.assertion.AssertionId; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return this.assertion.SecurityKeys;
            }
        }

        public SamlAssertion Assertion
        {
            get { return this.assertion; }
        }

        public override DateTime ValidFrom
        {
            get
            {
                if (this.assertion.Conditions != null)
                {
                    return this.assertion.Conditions.NotBefore;
                }

                return SecurityUtils.MinUtcDateTime;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                if (this.assertion.Conditions != null)
                {
                    return this.assertion.Conditions.NotOnOrAfter;
                }

                return SecurityUtils.MaxUtcDateTime;
            }
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(SamlAssertionKeyIdentifierClause))
                return true;

            return false;
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(SamlAssertionKeyIdentifierClause))
                return new SamlAssertionKeyIdentifierClause(this.Id) as T;

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnableToCreateTokenReference)));
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            SamlAssertionKeyIdentifierClause samlKeyIdentifierClause = keyIdentifierClause as SamlAssertionKeyIdentifierClause;
            if (samlKeyIdentifierClause != null)
                return samlKeyIdentifierClause.Matches(this.Id);

            return false;
        }
    }

}
