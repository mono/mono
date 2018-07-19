//-----------------------------------------------------------------------
// <copyright file="Saml2AudienceRestriction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the AttributeStatement element specified in [Saml2Core, 2.7.3].
    /// </summary>
    public class Saml2AttributeStatement : Saml2Statement
    {
        private Collection<Saml2Attribute> attributes = new Collection<Saml2Attribute>();

        /// <summary>
        /// Creates an instance of Saml2AttributeStatement.
        /// </summary>
        public Saml2AttributeStatement()
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AttributeStatement.
        /// </summary>
        /// <param name="attribute">The <see cref="Saml2Attribute"/> contained in this statement.</param>
        public Saml2AttributeStatement(Saml2Attribute attribute)
            : this(new Saml2Attribute[] { attribute })
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AttributeStatement.
        /// </summary>
        /// <param name="attributes">The collection of <see cref="Saml2Attribute"/> elements contained in this statement.</param>
        public Saml2AttributeStatement(IEnumerable<Saml2Attribute> attributes)
        {
            if (attributes == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributes");
            }

            foreach (Saml2Attribute attribute in attributes)
            {
                if (attribute == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributes");
                }

                this.attributes.Add(attribute);
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="Saml2Attribute"/> of this statement. [Saml2Core, 2.7.3]
        /// </summary>
        public Collection<Saml2Attribute> Attributes
        {
            get { return this.attributes; }
        }
    }
}
