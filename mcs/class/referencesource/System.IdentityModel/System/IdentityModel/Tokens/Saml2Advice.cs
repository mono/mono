//-----------------------------------------------------------------------
// <copyright file="Saml2Advice.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Advice element specified in [Saml2Core, 2.6.1].
    /// </summary>
    /// <remarks>
    /// This information MAY be ignored by applications without affecting either
    /// the semantics or the validity of the assertion. [Saml2Core, 2.6.1]
    /// </remarks>
    public class Saml2Advice
    {
        private Collection<Saml2Id> assertionIdReferences = new Collection<Saml2Id>();
        private Collection<Saml2Assertion> assertions = new Collection<Saml2Assertion>();
        private AbsoluteUriCollection assertionUriReferences = new AbsoluteUriCollection();

        /// <summary>
        /// Creates an instance of Saml2Advice.
        /// </summary>
        public Saml2Advice()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="Saml2Id"/> representating the assertions in the <see cref="Saml2Advice"/>.
        /// </summary>
        public Collection<Saml2Id> AssertionIdReferences
        {
            get { return this.assertionIdReferences; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Saml2Assertion"/> representating the assertions in the <see cref="Saml2Advice"/>.
        /// </summary>
        public Collection<Saml2Assertion> Assertions
        {
            get { return this.assertions; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Uri"/> representing the assertions in the <see cref="Saml2Advice"/>.
        /// </summary>
        public Collection<Uri> AssertionUriReferences
        {
            get { return this.assertionUriReferences; }
        }
    }
}
