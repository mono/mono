//-----------------------------------------------------------------------
// <copyright file="Saml2SubjectConfirmation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// A security token backed by a SAML2 assertion.
    /// </summary>
    public class Saml2SecurityToken : SecurityToken
    {
        private Saml2Assertion assertion;
        private ReadOnlyCollection<SecurityKey> keys;
        private SecurityToken issuerToken;

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SecurityToken"/> from a <see cref="Saml2Assertion"/>.
        /// </summary>
        /// <param name="assertion">A <see cref="Saml2Assertion"/> to initialize from.</param>
        public Saml2SecurityToken(Saml2Assertion assertion)
            : this(assertion, EmptyReadOnlyCollection<SecurityKey>.Instance, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SecurityToken"/> from a <see cref="Saml2Assertion"/>.
        /// </summary>
        /// <param name="assertion">A <see cref="Saml2Assertion"/> to initialize from.</param>
        /// <param name="keys">A collection of <see cref="SecurityKey"/> to include in the token.</param>
        /// <param name="issuerToken">A <see cref="SecurityToken"/> representing the issuer.</param>
        public Saml2SecurityToken(Saml2Assertion assertion, ReadOnlyCollection<SecurityKey> keys, SecurityToken issuerToken)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            if (null == keys)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keys");
            }

            this.assertion = assertion;
            this.keys = keys;
            this.issuerToken = issuerToken;
        }

        /// <summary>
        /// Gets the <see cref="Saml2Assertion"/> for this token.
        /// </summary>
        public Saml2Assertion Assertion
        {
            get { return this.assertion; }
        }

        /// <summary>
        /// Gets the SecurityToken id.
        /// </summary>
        public override string Id
        {
            get { return this.assertion.Id.Value; }
        }

        /// <summary>
        /// Gets the <see cref="SecurityToken"/> of the issuer.
        /// </summary>
        public SecurityToken IssuerToken
        {
            get { return this.issuerToken; }
        }

        /// <summary>
        /// Gets the collection of <see cref="SecurityKey"/> contained in this token.
        /// </summary>
        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return this.keys; }
        }

        /// <summary>
        /// Gets the time the token is valid from.
        /// </summary>
        public override DateTime ValidFrom
        {
            get
            {
                if (null != this.assertion.Conditions && null != this.assertion.Conditions.NotBefore)
                {
                    return this.assertion.Conditions.NotBefore.Value;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Gets the time the token is valid to.
        /// </summary>
        public override DateTime ValidTo
        {
            get
            {
                if (null != this.assertion.Conditions && null != this.assertion.Conditions.NotOnOrAfter)
                {
                    return this.assertion.Conditions.NotOnOrAfter.Value;
                }
                else
                {
                    return DateTime.MaxValue;
                }
            }
        }

        /// <summary>
        /// Determines if this token matches the keyIdentifierClause.
        /// </summary>
        /// <param name="keyIdentifierClause"><see cref="SecurityKeyIdentifierClause"/> to match.</param>
        /// <returns>True if the keyIdentifierClause is matched. False otherwise.</returns>
        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return Saml2AssertionKeyIdentifierClause.Matches(this.Id, keyIdentifierClause)
                || base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        /// <summary>
        /// Determines is this token can create a <see cref="SecurityKeyIdentifierClause"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="SecurityKeyIdentifierClause"/> to check if creation is possible.</typeparam>
        /// <returns>'True' if this token can create a <see cref="SecurityKeyIdentifierClause"/> of type T. 'False' otherwise.</returns>
        public override bool CanCreateKeyIdentifierClause<T>()
        {
            return (typeof(T) == typeof(Saml2AssertionKeyIdentifierClause))
                || base.CanCreateKeyIdentifierClause<T>();
        }

        /// <summary>
        /// Creates a <see cref="SecurityKeyIdentifierClause"/> that represents this token. 
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="SecurityKeyIdentifierClause"/> to create.</typeparam>
        /// <returns>A <see cref="SecurityKeyIdentifierClause"/> for this token.</returns>
        public override T CreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(Saml2AssertionKeyIdentifierClause))
            {
                return new Saml2AssertionKeyIdentifierClause(this.assertion.Id.Value) as T;
            }
            else if (typeof(T) == typeof(SamlAssertionKeyIdentifierClause))
            {
                return new WrappedSaml2AssertionKeyIdentifierClause(new Saml2AssertionKeyIdentifierClause(this.assertion.Id.Value)) as T;
            }
            else
            {
                return base.CreateKeyIdentifierClause<T>();
            }
        }
    }
}
