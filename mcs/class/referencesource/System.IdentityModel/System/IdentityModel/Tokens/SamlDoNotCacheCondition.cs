//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Xml;
    using System.IdentityModel.Selectors;

    public class SamlDoNotCacheCondition : SamlCondition
    {
        bool isReadOnly = false;

        public SamlDoNotCacheCondition()
        {
        }

        public override bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public override void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            if (!reader.IsStartElement(dictionary.DoNotCacheCondition, dictionary.Namespace))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.DoNotCacheCondition.Value)));

            // saml:DoNotCacheCondition is a empty element. So just issue a read for
            // the empty element.
            if (reader.IsEmptyElement)
            {
                reader.MoveToContent();
                reader.Read();
                return;
            }

            reader.MoveToContent();
            reader.Read();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.DoNotCacheCondition, dictionary.Namespace);
            writer.WriteEndElement();
        }
    }
}
