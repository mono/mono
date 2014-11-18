//-----------------------------------------------------------------------
// <copyright file="Saml2Action.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;

    /// <summary>
    /// Represents the Action element specified in [Saml2Core, 2.7.4.2].
    /// </summary>
    public class Saml2Action
    {
        private Uri actionNamespace;
        private string value;

        /// <summary>
        /// Constructs an instance of Saml2Action class.
        /// </summary>
        /// <param name="value">Value represented by this class.</param>
        /// <param name="actionNamespace">Namespace in which the action is interpreted.</param>
        public Saml2Action(string value, Uri actionNamespace)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            // ==
            // There is a discrepency between the schema and the text of the 
            // specification as to whether the Namespace attribute is optional
            // or required. The schema specifies required.
            // ==
            // Per the SAML 2.0 errata the schema takes precedence over the text, 
            // and the namespace attribute is required. This is errata item E36.
            // ==
            // SAML 2.0 errata at the time of this implementation:
            // http://docs.oasis-open.org/security/saml/v2.0/sstc-saml-approved-errata-2.0-cd-02.pdf
            // ==
            if (null == actionNamespace)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actionNamespace");
            }

            if (!actionNamespace.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("actionNamespace", SR.GetString(SR.ID0013));
            }

            this.actionNamespace = actionNamespace;
            this.value = value;
        }

        /// <summary>
        /// Gets or sets a URI reference representing the namespace in which the name of the
        /// specified action is to be interpreted. [Saml2Core, 2.7.4.2]
        /// </summary>
        public Uri Namespace
        {
            get
            { 
                return this.actionNamespace; 
            }

            set
            {
                // See note in constructor about why this is required.
                if (null == value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (!value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }

                this.actionNamespace = value;
            }
        }

        /// <summary>
        /// Gets or sets the label for an action sought to be performed on the 
        /// specified resource. [Saml2Core, 2.7.4.2]
        /// </summary>
        public string Value
        {
            get
            { 
                return this.value; 
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.value = value;
            }
        }
    }
}
