//-----------------------------------------------------------------------
// <copyright file="Saml2Evidence.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Evidence element specified in [Saml2Core, 2.7.4.3].
    /// </summary>
    /// <remarks>
    /// Contains one or more assertions or assertion references that the SAML
    /// authority relied on in issuing the authorization decision. 
    /// [Saml2Core, 2.7.4.3]
    /// </remarks>
    public class Saml2Evidence
    {
        private Collection<Saml2Id> assertionIdReferences = new Collection<Saml2Id>();
        private Collection<Saml2Assertion> assertions = new Collection<Saml2Assertion>();
        private AbsoluteUriCollection assertionUriReferences = new AbsoluteUriCollection();

        /// <summary>
        /// Initializes a new instance of <see cref="Saml2Evidence"/> class.
        /// </summary>
        public Saml2Evidence()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Saml2Evidence"/> class from a <see cref="Saml2Assertion"/>.
        /// </summary>
        /// <param name="assertion"><see cref="Saml2Assertion"/> containing the evidence.</param>
        public Saml2Evidence(Saml2Assertion assertion)
        {
            if (null == assertion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertion");
            }

            this.assertions.Add(assertion);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Saml2Evidence"/> class from a <see cref="Saml2Id"/>.
        /// </summary>
        /// <param name="idReference"><see cref="Saml2Id"/> containing the evidence.</param>
        public Saml2Evidence(Saml2Id idReference)
        {
            if (null == idReference)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("idReference");
            }

            this.assertionIdReferences.Add(idReference);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Saml2Evidence"/> class from a <see cref="Uri"/>.
        /// </summary>
        /// <param name="uriReference"><see cref="Uri"/> containing the evidence.</param>
        public Saml2Evidence(Uri uriReference)
        {
            if (null == uriReference)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uriReference");
            }

            this.assertionUriReferences.Add(uriReference);
        }

        /// <summary>
        /// Gets a collection of <see cref="Saml2Id"/> for use by the <see cref="Saml2Evidence"/>.
        /// </summary>
        public Collection<Saml2Id> AssertionIdReferences
        {
            get { return this.assertionIdReferences; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Saml2Assertion"/>  for use by the <see cref="Saml2Evidence"/>.
        /// </summary>
        public Collection<Saml2Assertion> Assertions
        {
            get { return this.assertions; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Uri"/>  for use by the <see cref="Saml2Evidence"/>.
        /// </summary>
        public Collection<Uri> AssertionUriReferences
        {
            get { return this.assertionUriReferences; }
        }
    }
}
