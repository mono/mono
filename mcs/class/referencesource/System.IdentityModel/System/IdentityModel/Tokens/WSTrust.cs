//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using KeyIdentifierClauseEntry = System.IdentityModel.Selectors.SecurityTokenSerializer.KeyIdentifierClauseEntry;

    internal class WSTrust : SecurityTokenSerializer.SerializerEntries
    {
        KeyInfoSerializer securityTokenSerializer;
        TrustDictionary serializerDictionary;

        public WSTrust(KeyInfoSerializer securityTokenSerializer, TrustDictionary serializerDictionary)
        {
            this.securityTokenSerializer = securityTokenSerializer;
            this.serializerDictionary = serializerDictionary;
        }

        public TrustDictionary SerializerDictionary
        {
            get
            {
                return this.serializerDictionary;
            }
        }

        public override void PopulateTokenEntries(IList<SecurityTokenSerializer.TokenEntry> tokenEntryList)
        {
            tokenEntryList.Add(new BinarySecretTokenEntry(this));
        }

        public override void PopulateKeyIdentifierClauseEntries(IList<SecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries)
        {
            keyIdentifierClauseEntries.Add(new BinarySecretClauseEntry(this));
            keyIdentifierClauseEntries.Add(new GenericXmlSecurityKeyIdentifierClauseEntry(this));
        }

        class BinarySecretTokenEntry : SecurityTokenSerializer.TokenEntry
        {
            WSTrust parent;

            public BinarySecretTokenEntry(WSTrust parent)
            {
                this.parent = parent;
            }

            protected override XmlDictionaryString LocalName { get { return parent.SerializerDictionary.BinarySecret; } }
            protected override XmlDictionaryString NamespaceUri { get { return parent.SerializerDictionary.Namespace; } }
            protected override Type[] GetTokenTypesCore() { return new Type[] { typeof(BinarySecretSecurityToken) }; }
            public override string TokenTypeUri { get { return null; } }
            protected override string ValueTypeUri { get { return null; } }

        }

        internal class BinarySecretClauseEntry : KeyIdentifierClauseEntry
        {
            WSTrust parent;
            TrustDictionary otherDictionary = null;

            public BinarySecretClauseEntry(WSTrust parent)
            {
                this.parent = parent;

                this.otherDictionary = null;

                if (parent.SerializerDictionary is TrustDec2005Dictionary)
                {
                    this.otherDictionary = parent.securityTokenSerializer.DictionaryManager.TrustFeb2005Dictionary;
                }

                if (parent.SerializerDictionary is TrustFeb2005Dictionary)
                {
                    this.otherDictionary = parent.securityTokenSerializer.DictionaryManager.TrustDec2005Dictionary;
                }

                // always set it, so we don't have to worry about null
                if (this.otherDictionary == null)
                    this.otherDictionary = this.parent.SerializerDictionary;
            }

            protected override XmlDictionaryString LocalName
            {
                get { return this.parent.SerializerDictionary.BinarySecret; }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get { return this.parent.SerializerDictionary.Namespace; }
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                byte[] secret = reader.ReadElementContentAsBase64();
                return new BinarySecretKeyIdentifierClause(secret, false);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return keyIdentifierClause is BinarySecretKeyIdentifierClause;
            }

            public override bool CanReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                return (reader.IsStartElement(this.LocalName, this.NamespaceUri) || reader.IsStartElement(this.LocalName, this.otherDictionary.Namespace));
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                BinarySecretKeyIdentifierClause skic = keyIdentifierClause as BinarySecretKeyIdentifierClause;
                byte[] secret = skic.GetKeyBytes();
                writer.WriteStartElement(this.parent.SerializerDictionary.Prefix.Value, this.parent.SerializerDictionary.BinarySecret, this.parent.SerializerDictionary.Namespace);
                writer.WriteBase64(secret, 0, secret.Length);
                writer.WriteEndElement();
            }
        }

        internal class GenericXmlSecurityKeyIdentifierClauseEntry : KeyIdentifierClauseEntry
        {
            private WSTrust parent;

            public GenericXmlSecurityKeyIdentifierClauseEntry(WSTrust parent)
            {
                this.parent = parent;
            }

            protected override XmlDictionaryString LocalName
            {
                get { return null; }
            }

            protected override XmlDictionaryString NamespaceUri
            {
                get { return null; }
            }

            public override bool CanReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                return false;
            }

            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                return null;
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return keyIdentifierClause is GenericXmlSecurityKeyIdentifierClause;
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                GenericXmlSecurityKeyIdentifierClause genericXmlSecurityKeyIdentifierClause = keyIdentifierClause as GenericXmlSecurityKeyIdentifierClause;
                genericXmlSecurityKeyIdentifierClause.ReferenceXml.WriteTo(writer);
            }
        }

        protected static bool CheckElement(XmlElement element, string name, string ns, out string value)
        {
            value = null;
            if (element.LocalName != name || element.NamespaceURI != ns)
                return false;
            if (element.FirstChild is XmlText)
            {
                value = ((XmlText)element.FirstChild).Value;
                return true;
            }
            return false;
        }
    }
}
