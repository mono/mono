//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.Xml;
    using KeyIdentifierClauseEntry = System.IdentityModel.Selectors.SecurityTokenSerializer.KeyIdentifierClauseEntry;

    class XmlEncApr2001 : SecurityTokenSerializer.SerializerEntries
    {
        KeyInfoSerializer securityTokenSerializer;

        public XmlEncApr2001(KeyInfoSerializer securityTokenSerializer)
        {
            this.securityTokenSerializer = securityTokenSerializer;
        }
        
        public override void PopulateKeyIdentifierClauseEntries(IList<KeyIdentifierClauseEntry> keyIdentifierClauseEntries)
        {
            keyIdentifierClauseEntries.Add(new EncryptedKeyClauseEntry(this.securityTokenSerializer));
        }

        internal class EncryptedKeyClauseEntry : SecurityTokenSerializer.KeyIdentifierClauseEntry
        {
            KeyInfoSerializer securityTokenSerializer;

            public EncryptedKeyClauseEntry(KeyInfoSerializer securityTokenSerializer)
            {
                this.securityTokenSerializer = securityTokenSerializer;
            }
                        
            protected override XmlDictionaryString LocalName
            { 
                get
                {
                    return XD.XmlEncryptionDictionary.EncryptedKey;
                }
            }
            
            protected override XmlDictionaryString NamespaceUri
            { 
                get
                {
                    return XD.XmlEncryptionDictionary.Namespace;
                }
            }
            
            public override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                string encryptionMethod = null;
                string carriedKeyName = null;
                SecurityKeyIdentifier encryptingKeyIdentifier = null;
                byte[] encryptedKey = null;

                reader.ReadStartElement(XD.XmlEncryptionDictionary.EncryptedKey, NamespaceUri);
                
                if (reader.IsStartElement(XD.XmlEncryptionDictionary.EncryptionMethod, NamespaceUri))
                {
                    encryptionMethod = reader.GetAttribute(XD.XmlEncryptionDictionary.AlgorithmAttribute, null);
                    bool isEmptyElement = reader.IsEmptyElement;
                    reader.ReadStartElement();
                    if (!isEmptyElement)
                    {
                        while (reader.IsStartElement())
                        {
                            reader.Skip();
                        }
                        reader.ReadEndElement();
                    }
                }

                if (this.securityTokenSerializer.CanReadKeyIdentifier(reader))
                {
                    encryptingKeyIdentifier = this.securityTokenSerializer.ReadKeyIdentifier(reader);
                }
                
                reader.ReadStartElement(XD.XmlEncryptionDictionary.CipherData, NamespaceUri);
                reader.ReadStartElement(XD.XmlEncryptionDictionary.CipherValue, NamespaceUri);
                encryptedKey = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                reader.ReadEndElement();

                if (reader.IsStartElement(XD.XmlEncryptionDictionary.CarriedKeyName, NamespaceUri))
                {
                    reader.ReadStartElement();
                    carriedKeyName = reader.ReadString();                    
                    reader.ReadEndElement();
                }
                
                reader.ReadEndElement();

                return new EncryptedKeyIdentifierClause(encryptedKey, encryptionMethod, encryptingKeyIdentifier, carriedKeyName);
            }

            public override bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause)
            {
                return keyIdentifierClause is EncryptedKeyIdentifierClause;
            }

            public override void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
            {
                EncryptedKeyIdentifierClause encryptedKeyClause = keyIdentifierClause as EncryptedKeyIdentifierClause;

                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.EncryptedKey, NamespaceUri);

                if (encryptedKeyClause.EncryptionMethod != null)
                {
                    writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.EncryptionMethod, NamespaceUri);
                    writer.WriteAttributeString(XD.XmlEncryptionDictionary.AlgorithmAttribute, null, encryptedKeyClause.EncryptionMethod);
                    if (encryptedKeyClause.EncryptionMethod == XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap.Value)
                    {
                        writer.WriteStartElement(XmlSignatureStrings.Prefix, XD.XmlSignatureDictionary.DigestMethod, XD.XmlSignatureDictionary.Namespace);
                        writer.WriteAttributeString(XD.XmlSignatureDictionary.Algorithm, null, SecurityAlgorithms.Sha1Digest);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (encryptedKeyClause.EncryptingKeyIdentifier != null)
                {
                    this.securityTokenSerializer.WriteKeyIdentifier(writer, encryptedKeyClause.EncryptingKeyIdentifier);
                }

                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.CipherData, NamespaceUri);
                writer.WriteStartElement(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.CipherValue, NamespaceUri);
                byte[] encryptedKey = encryptedKeyClause.GetEncryptedKey();
                writer.WriteBase64(encryptedKey, 0, encryptedKey.Length);
                writer.WriteEndElement();
                writer.WriteEndElement();

                if (encryptedKeyClause.CarriedKeyName != null)
                {
                    writer.WriteElementString(XD.XmlEncryptionDictionary.Prefix.Value, XD.XmlEncryptionDictionary.CarriedKeyName, NamespaceUri, encryptedKeyClause.CarriedKeyName);
                }

                writer.WriteEndElement();
            }
        }
    }
}
