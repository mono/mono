//-----------------------------------------------------------------------
// <copyright file="Saml2Subject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents the Subject element specified in [Saml2Core, 2.4.1].
    /// </summary>
    /// <remarks>
    /// If the NameId is null and the SubjectConfirmations collection is empty,
    /// an InvalidOperationException will be thrown during serialization.
    /// </remarks>
    public class Saml2Subject
    {
        private Saml2NameIdentifier nameId;
        private Collection<Saml2SubjectConfirmation> subjectConfirmations = new Collection<Saml2SubjectConfirmation>();

        /// <summary>
        /// Initialize an instance of <see cref="Saml2Subject"/>.
        /// </summary>
        public Saml2Subject()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="Saml2Subject"/> from a <see cref="Saml2NameIdentifier"/>.
        /// </summary>
        /// <param name="nameId">The <see cref="Saml2NameIdentifier"/> to use for initialization.</param>
        public Saml2Subject(Saml2NameIdentifier nameId)
        {
            this.nameId = nameId;
        }

        /// <summary>
        /// Initializes an instance of <see cref="Saml2Subject"/> from a <see cref="Saml2SubjectConfirmation"/>.
        /// </summary>
        /// <param name="subjectConfirmation">The <see cref="Saml2SubjectConfirmation"/> to use for initialization.</param>
        public Saml2Subject(Saml2SubjectConfirmation subjectConfirmation)
        {
            if (null == subjectConfirmation)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectConfirmation");
            }

            this.subjectConfirmations.Add(subjectConfirmation);
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2NameIdentifier"/>. [Saml2Core, 2.4.1]
        /// </summary>
        public Saml2NameIdentifier NameId
        {
            get { return this.nameId; }
            set { this.nameId = value; }
        }

        /// <summary>
        /// Gets a collection of <see cref="Saml2SubjectConfirmation"/> which can be used to validate and confirm the <see cref="Saml2Subject"/>. [Saml2Core, 2.4.1]
        /// </summary>
        /// <remarks>
        /// If more than one subject confirmation is provied, then satisfying any one of 
        /// them is sufficient to confirm the subject for the purpose of applying the 
        /// assertion.
        /// </remarks>
        public Collection<Saml2SubjectConfirmation> SubjectConfirmations
        {
            get { return this.subjectConfirmations; }
        }
    }
}
