//-----------------------------------------------------------------------
// <copyright file="Saml2SubjectConfirmation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;

    /// <summary>
    /// Represents the SubjectConfirmation element specified in [Saml2Core, 2.4.1.1]. 
    /// </summary>
    public class Saml2SubjectConfirmation
    {
        private Saml2SubjectConfirmationData data;
        private Uri method;
        private Saml2NameIdentifier nameId;

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SubjectConfirmation"/> from a <see cref="Uri"/> indicating the
        /// method of confirmation.
        /// </summary>
        /// <param name="method">The <see cref="Uri"/> to use for initialization.</param>
        public Saml2SubjectConfirmation(Uri method)
            : this(method, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Saml2SubjectConfirmation"/> from a <see cref="Uri"/> indicating the
        /// method of confirmation and <see cref="Saml2SubjectConfirmationData"/>.
        /// </summary>
        /// <param name="method">The <see cref="Uri"/> to use for initialization.</param>
        /// <param name="data">The <see cref="Saml2SubjectConfirmationData"/> to use for initialization.</param>
        public Saml2SubjectConfirmation(Uri method, Saml2SubjectConfirmationData data)
        {
            if (null == method)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("method");
            }

            if (!method.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("method", SR.GetString(SR.ID0013));
            }

            this.method = method;
            this.data = data;
        }

        /// <summary>
        /// Gets or sets a URI reference that identifies a protocol or mechanism to be used to 
        /// confirm the subject. [Saml2Core, 2.4.1.1]
        /// </summary>
        public Uri Method
        {
            get 
            { 
                return this.method; 
            }

            set
            {
                if (null == value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (!value.IsAbsoluteUri)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.ID0013));
                }

                this.method = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2NameIdentifier"/> expected to satisfy the enclosing subject 
        /// confirmation requirements. [Saml2Core, 2.4.1.1]
        /// </summary>
        public Saml2NameIdentifier NameIdentifier
        {
            get { return this.nameId; }
            set { this.nameId = value; }
        }

        /// <summary>
        /// Gets or sets additional <see cref="Saml2SubjectConfirmationData"/> to be used by a specific confirmation
        /// method. [Saml2Core, 2.4.1.1]
        /// </summary>
        public Saml2SubjectConfirmationData SubjectConfirmationData
        {
            get { return this.data; }
            set { this.data = value; }
        }
    }
}
