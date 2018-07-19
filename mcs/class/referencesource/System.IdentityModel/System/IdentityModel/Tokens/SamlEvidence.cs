//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;
    using System.IdentityModel.Selectors;

    public class SamlEvidence
    {
        readonly ImmutableCollection<string> assertionIdReferences = new ImmutableCollection<string>();
        readonly ImmutableCollection<SamlAssertion> assertions = new ImmutableCollection<SamlAssertion>();
        bool isReadOnly = false;

        public SamlEvidence(IEnumerable<string> assertionIdReferences)
            : this(assertionIdReferences, null)
        {
        }

        public SamlEvidence(IEnumerable<SamlAssertion> assertions)
            : this(null, assertions)
        {
        }

        public SamlEvidence(IEnumerable<string> assertionIdReferences, IEnumerable<SamlAssertion> assertions)
        {
            if (assertionIdReferences == null && assertions == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEvidenceShouldHaveOneAssertion));

            if (assertionIdReferences != null)
            {
                foreach (string idReference in assertionIdReferences)
                {
                    if (string.IsNullOrEmpty(idReference))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.AssertionIdReference.Value));

                    this.assertionIdReferences.Add(idReference);
                }
            }

            if (assertions != null)
            {
                foreach (SamlAssertion assertion in assertions)
                {
                    if (assertion == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.Assertion.Value));

                    this.assertions.Add(assertion);
                }
            }
        }

        public SamlEvidence()
        {
        }

        public IList<string> AssertionIdReferences
        {
            get { return this.assertionIdReferences; }
        }

        public IList<SamlAssertion> Assertions
        {
            get { return this.assertions; }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                foreach (SamlAssertion assertion in this.assertions)
                {
                    assertion.MakeReadOnly();
                }

                this.assertionIdReferences.MakeReadOnly();
                this.assertions.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        void CheckObjectValidity()
        {
            if ((this.assertions.Count == 0) && (this.assertionIdReferences.Count == 0))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLEvidenceShouldHaveOneAssertion)));
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            reader.MoveToContent();
            reader.Read();
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.AssertionIdReference, dictionary.Namespace))
                {
                    reader.MoveToContent();
                    this.assertionIdReferences.Add(reader.ReadString());
                    reader.ReadEndElement();
                }
                else if (reader.IsStartElement(dictionary.Assertion, dictionary.Namespace))
                {
                    SamlAssertion assertion = new SamlAssertion();
                    assertion.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                    this.assertions.Add(assertion);
                }
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.Evidence.Value)));
            }

            if ((this.assertionIdReferences.Count == 0) && (this.assertions.Count == 0))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLEvidenceShouldHaveOneAssertionOnRead)));

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.Evidence.Value, dictionary.Namespace.Value);

            for (int i = 0; i < this.assertionIdReferences.Count; i++)
            {
                writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AssertionIdReference, dictionary.Namespace);
                writer.WriteString(this.assertionIdReferences[i]);
                writer.WriteEndElement();
            }

            for (int i = 0; i < this.assertions.Count; i++)
            {
                this.assertions[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }

            writer.WriteEndElement();
        }
    }
}


