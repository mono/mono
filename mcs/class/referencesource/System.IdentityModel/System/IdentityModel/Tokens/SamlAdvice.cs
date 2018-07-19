//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Threading;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;

    public class SamlAdvice
    {
        readonly ImmutableCollection<string> assertionIdReferences = new ImmutableCollection<string>();
        readonly ImmutableCollection<SamlAssertion> assertions = new ImmutableCollection<SamlAssertion>();
        bool isReadOnly = false;

        public SamlAdvice()
            : this(null, null)
        {
        }

        public SamlAdvice(IEnumerable<string> references)
            : this(references, null)
        {
        }

        public SamlAdvice(IEnumerable<SamlAssertion> assertions)
            : this(null, assertions)
        {
        }

        public SamlAdvice(IEnumerable<string> references, IEnumerable<SamlAssertion> assertions)
        {
            if (references != null)
            {
                foreach (string idReference in references)
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
                this.assertionIdReferences.MakeReadOnly();

                foreach (SamlAssertion assertion in this.assertions)
                {
                    assertion.MakeReadOnly();
                }

                this.assertions.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            // SAML Advice is an optional element and all its child elements are optional 
            // too. So we may have an empty saml:Advice element in the saml token.
            if (reader.IsEmptyElement)
            {
                // Just issue a read for the empty element.
                reader.MoveToContent();
                reader.Read();
                return;
            }

            reader.MoveToContent();
            reader.Read();
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.AssertionIdReference, dictionary.Namespace))
                {
                    reader.MoveToContent();
                    this.assertionIdReferences.Add(reader.ReadString());
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }
                else if (reader.IsStartElement(dictionary.Assertion, dictionary.Namespace))
                {
                    SamlAssertion assertion = new SamlAssertion();
                    assertion.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                    this.assertions.Add(assertion);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.Advice.Value)));
                }
            }

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.Advice, dictionary.Namespace);

            for (int i = 0; i < this.assertionIdReferences.Count; i++)
            {
                writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AssertionIdReference, dictionary.Namespace);
                writer.WriteString(assertionIdReferences[i]);
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

