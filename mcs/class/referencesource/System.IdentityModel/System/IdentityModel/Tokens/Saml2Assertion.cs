//-----------------------------------------------------------------------
// <copyright file="Saml2Assertion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.Xml;

    /// <summary>
    /// Represents the Assertion element specified in [Saml2Core, 2.3.3].
    /// </summary>
    public class Saml2Assertion
    {
        private Saml2Advice advice;
        private Saml2Conditions conditions;
        private EncryptingCredentials encryptingCredentials;
        private Collection<EncryptedKeyIdentifierClause> externalEncryptedKeys = new Collection<EncryptedKeyIdentifierClause>();
        private Saml2Id id = new Saml2Id();
        private DateTime issueInstant = DateTime.UtcNow;
        private Saml2NameIdentifier issuer;
        private SigningCredentials signingCredentials;
        private XmlTokenStream sourceData;
        private Collection<Saml2Statement> statements = new Collection<Saml2Statement>();
        private Saml2Subject subject;
        private string version = "2.0";

        /// <summary>
        /// Creates an instance of a Saml2Assertion.
        /// </summary>
        /// <param name="issuer">Issuer of the assertion.</param>
        public Saml2Assertion(Saml2NameIdentifier issuer)
        {
            if (issuer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuer");
            }

            this.issuer = issuer;
        }

        /// <summary>
        /// Gets or sets additional information related to the assertion that assists processing in certain
        /// situations but which may be ignored by applications that do not understand the 
        /// advice or do not wish to make use of it. [Saml2Core, 2.3.3]
        /// </summary>
        public Saml2Advice Advice
        {
            get { return this.advice; }
            set { this.advice = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this assertion was deserialized from XML source
        /// and can re-emit the XML data unchanged.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The default implementation preserves the source data when read using
        /// Saml2AssertionSerializer.ReadAssertion and is willing to re-emit the
        /// original data as long as the Id has not changed from the time that 
        /// assertion was read.
        /// </para>
        /// <para>
        /// Note that it is vitally important that SAML assertions with different
        /// data have different IDs. If implementing a scheme whereby an assertion
        /// "template" is loaded and certain bits of data are filled in, the Id 
        /// must be changed.
        /// </para>
        /// </remarks>
        /// <returns>'True' if this instance can write the source data.</returns>
        public virtual bool CanWriteSourceData
        {
            get { return null != this.sourceData; }
        }

        /// <summary>
        /// Gets or sets conditions that must be evaluated when assessing the validity of and/or
        /// when using the assertion. [Saml2Core 2.3.3]
        /// </summary>
        public Saml2Conditions Conditions
        {
            get { return this.conditions; }
            set { this.conditions = value; }
        }

        /// <summary>
        /// Gets or sets the credentials used for encrypting the assertion. The key
        /// identifier in the encrypting credentials will be used for the 
        /// embedded EncryptedKey in the EncryptedData element.
        /// </summary>
        public EncryptingCredentials EncryptingCredentials
        {
            get { return this.encryptingCredentials; }
            set { this.encryptingCredentials = value; }
        }

        /// <summary>
        /// Gets additional encrypted keys which will be specified external to the 
        /// EncryptedData element, as children of the EncryptedAssertion element.
        /// </summary>
        public Collection<EncryptedKeyIdentifierClause> ExternalEncryptedKeys
        {
            get { return this.externalEncryptedKeys; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2Id"/> identifier for this assertion. [Saml2Core, 2.3.3]
        /// </summary>
        public Saml2Id Id
        {
            get 
            { 
                return this.id; 
            }

            set
            {
                if (null == value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.id = value;
                this.sourceData = null;
            }
        }

        /// <summary>
        /// Gets or sets the time instant of issue in UTC. [Saml2Core, 2.3.3]
        /// </summary>
        public DateTime IssueInstant
        {
            get { return this.issueInstant; }
            set { this.issueInstant = DateTimeUtil.ToUniversalTime(value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2NameIdentifier"/> as the authority that is making the claim(s) in the assertion. [Saml2Core, 2.3.3]
        /// </summary>
        public Saml2NameIdentifier Issuer
        {
            get 
            { 
                return this.issuer; 
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.issuer = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SigningCredentials"/> used by the issuer to protect the integrity of the assertion.
        /// </summary>
        public SigningCredentials SigningCredentials
        {
            get { return this.signingCredentials; }
            set { this.signingCredentials = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Saml2Subject"/> of the statement(s) in the assertion. [Saml2Core, 2.3.3]
        /// </summary>
        public Saml2Subject Subject
        {
            get { return this.subject; }
            set { this.subject = value; }
        }

        /// <summary>
        /// Gets the <see cref="Saml2Statement"/>(s) regarding the subject.
        /// </summary>
        public Collection<Saml2Statement> Statements
        {
            get { return this.statements; }
        }

        /// <summary>
        /// Gets the version of this assertion. [Saml2Core, 2.3.3]
        /// </summary>
        /// <remarks>
        /// In this version of the Windows Identity Foundation, only version "2.0" is supported.
        /// </remarks>
        public string Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// Writes the source data, if available.
        /// </summary>
        /// <exception cref="InvalidOperationException">When no source data is available</exception>
        /// <param name="writer">A <see cref="XmlWriter"/> for writting the data.</param>
        public virtual void WriteSourceData(XmlWriter writer)
        {
            if (!this.CanWriteSourceData)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ID4140)));
            }

            // This call will properly just reuse the existing writer if it already qualifies
            XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            this.sourceData.SetElementExclusion(null, null);
            this.sourceData.GetWriter().WriteTo(dictionaryWriter, new DictionaryManager());
        }

        /// <summary>
        /// Captures the XML source data from an EnvelopedSignatureReader. 
        /// </summary>
        /// <remarks>
        /// The EnvelopedSignatureReader that was used to read the data for this
        /// assertion should be passed to this method after the &lt;/Assertion>
        /// element has been read. This method will preserve the raw XML data
        /// that was read, including the signature, so that it may be re-emitted
        /// without changes and without the need to re-sign the data. See 
        /// CanWriteSourceData and WriteSourceData.
        /// </remarks>
        /// <param name="reader"><see cref="EnvelopedSignatureReader"/> that contains the data for the assertion.</param>
        internal virtual void CaptureSourceData(EnvelopedSignatureReader reader)
        {
            if (null == reader)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            this.sourceData = reader.XmlTokens;
        }
    }
}
