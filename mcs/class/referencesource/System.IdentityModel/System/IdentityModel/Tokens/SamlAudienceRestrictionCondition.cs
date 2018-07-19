//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class SamlAudienceRestrictionCondition : SamlCondition
    {
        readonly ImmutableCollection<Uri> audiences = new ImmutableCollection<Uri>();
        bool isReadOnly = false;

        public SamlAudienceRestrictionCondition(IEnumerable<Uri> audiences)
        {
            if (audiences == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("audiences"));

            foreach (Uri audience in audiences)
            {
                if (audience == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.Audience.Value));

                this.audiences.Add(audience);
            }


            CheckObjectValidity();
        }

        public SamlAudienceRestrictionCondition()
        {
        }

        public IList<Uri> Audiences
        {
            get { return this.audiences; }
        }

        public override bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.audiences.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        void CheckObjectValidity()
        {
            if (this.audiences.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAudienceRestrictionShouldHaveOneAudience)));
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
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
                if (reader.IsStartElement(dictionary.Audience, dictionary.Namespace))
                {
                    reader.MoveToContent();
                    string audience = reader.ReadString();
                    if (string.IsNullOrEmpty(audience))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAudienceRestrictionInvalidAudienceValueOnRead)));

                    this.audiences.Add(new Uri(audience));
                    reader.MoveToContent();
                    reader.ReadEndElement();
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.AudienceRestrictionCondition.Value)));
                }
            }

            if (this.audiences.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAudienceRestrictionShouldHaveOneAudienceOnRead)));

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AudienceRestrictionCondition, dictionary.Namespace);

            for (int i = 0; i < this.audiences.Count; i++)
            {
                writer.WriteElementString(dictionary.Audience, dictionary.Namespace, this.audiences[i].AbsoluteUri);
            }

            writer.WriteEndElement();
        }
    }
}
