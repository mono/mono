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
    /// Represents the AudienceRestriction element specified in [Saml2Core, 2.5.1.4].
    /// </summary>
    /// <remarks>
    /// If the Audiences collection is empty, an InvalidOperationException will be 
    /// thrown during serialization.
    /// </remarks>
    public class Saml2AudienceRestriction
    {
        private Collection<Uri> audiences = new Collection<Uri>();

        /// <summary>
        /// Creates an instance of Saml2AudienceRestriction.
        /// </summary>
        public Saml2AudienceRestriction()
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AudienceRestriction.
        /// </summary>
        /// <param name="audience">The audience element contained in this restriction.</param>
        public Saml2AudienceRestriction(Uri audience)
            : this(new Uri[] { audience })
        {
        }

        /// <summary>
        /// Creates an instance of Saml2AudienceRestriction.
        /// </summary>
        /// <param name="audiences">The collection of audience elements contained in this restriction.</param>
        public Saml2AudienceRestriction(IEnumerable<Uri> audiences)
        {
            if (null == audiences)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("audiences");
            }

            foreach (Uri audience in audiences)
            {
                if (null == audience)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("audiences");
                }

                this.audiences.Add(audience);
            }
        }

        /// <summary>
        /// Gets the audiences for which the assertion is addressed.
        /// </summary>
        public Collection<Uri> Audiences
        {
            get { return this.audiences; }
        }
    }
}
