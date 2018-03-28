//-----------------------------------------------------------------------
// <copyright file="WrappedSaml2AssertionKeyIdentifierClause.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// This class wraps a Saml2AssertionKeyIdentifierClause and delegates to the wrapped clause.
    /// It derives off the SamlAssertionKeyIdentifierClause to get around a specific bug in WCF
    /// where the WCF runtime will call the Saml2SecurityToken to create a SamlAssertionKeyIdentifierClause.
    /// </summary>
    internal class WrappedSaml2AssertionKeyIdentifierClause : SamlAssertionKeyIdentifierClause
    {
        private Saml2AssertionKeyIdentifierClause clause;

        /// <summary>
        /// Creates an instance of <see cref="WrappedSaml2AssertionKeyIdentifierClause"/>
        /// </summary>
        /// <param name="clause">A <see cref="Saml2AssertionKeyIdentifierClause"/> to be wrapped.</param>
        public WrappedSaml2AssertionKeyIdentifierClause(Saml2AssertionKeyIdentifierClause clause)
            : base(clause.Id)
        {
            this.clause = clause;
        }

        /// <summary>
        /// Gets a boolean that states if the clause can create a Key.
        /// This returns false by default.
        /// </summary>
        public override bool CanCreateKey
        {
            get
            {
                return this.clause.CanCreateKey;
            }
        }

        /// <summary>
        /// Gets the wrapped <see cref="Saml2AssertionKeyIdentifierClause" />.
        /// </summary>
        public Saml2AssertionKeyIdentifierClause WrappedClause
        {
            get { return this.clause; }
        }

        /// <summary>
        /// Creates a key from this clause type. 
        /// </summary>
        /// <returns>A <see cref="SecurityKey"/></returns>
        public override SecurityKey CreateKey()
        {
            return this.clause.CreateKey();
        }

        /// <summary>
        /// Returns a value that indicates whether the key identifier for this instance
        /// is equivalent to the specified key identifier clause.
        /// </summary>
        /// <param name="keyIdentifierClause">A <see cref="SecurityKeyIdentifierClause"/> to compare to.</param>
        /// <returns>'True' if keyIdentifierClause matches this.  'False' otherwise.</returns>
        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            return this.clause.Matches(keyIdentifierClause);
        }
    }
}
