//-----------------------------------------------------------------------
// <copyright file="Saml2ProxyRestriction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the ProxyRestriction element specified in [Saml2Core, 2.5.1.6].
    /// </summary>
    public class Saml2ProxyRestriction
    {
        private Collection<Uri> audiences = new AbsoluteUriCollection();
        private int? count;

        /// <summary>
        /// Initializes an instance of <see cref="Saml2ProxyRestriction"/>.
        /// </summary>
        public Saml2ProxyRestriction()
        {
        }

        /// <summary>
        /// Gets the set of audiences to whom the asserting party permits
        /// new assertions to be issued on the basis of this assertion.
        /// </summary>
        public Collection<Uri> Audiences
        {
            get { return this.audiences; }
        }

        /// <summary>
        /// Gets or sets the maximum number of indirections that the asserting party
        /// permits to exist between this assertion and an assertion which has 
        /// ultimately been issued on the basis of it.
        /// </summary>
        public int? Count
        {
            get
            { 
                return this.count; 
            }

            set
            {
                if (null != value)
                {
                    if (value.Value < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ID0002)));
                    }
                }

                this.count = value;
            }
        }
    }
}
