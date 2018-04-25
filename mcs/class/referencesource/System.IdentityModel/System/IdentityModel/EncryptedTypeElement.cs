//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Xml;

    /// <summary>
    /// This class implements a deserialization for: EncryptedType as defined in section 3.1 of http://www.w3.org/TR/2002/REC-xmlenc-core-2002120
    /// </summary>
    internal abstract class EncryptedTypeElement
    {
        KeyInfo _keyInfo;
        EncryptionMethodElement _encryptionMethod;
        CipherDataElement _cipherData;
        List<string> _properties;
        SecurityTokenSerializer _keyInfoSerializer;
        string _id;
        string _type;
        string _mimeType;
        string _encoding;

        public EncryptedTypeElement(SecurityTokenSerializer keyInfoSerializer)
        {
            _cipherData = new CipherDataElement();
            _encryptionMethod = new EncryptionMethodElement();
            _keyInfo = new KeyInfo(keyInfoSerializer);
            _properties = new List<string>();
            _keyInfoSerializer = keyInfoSerializer;
        }

        public string Algorithm
        {
            get { return (EncryptionMethod != null) ? EncryptionMethod.Algorithm : null; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                EncryptionMethod.Algorithm = value;
            }
        }

        public string Id
        {
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("value");
                }

                _id = value;
            }
        }

        public EncryptionMethodElement EncryptionMethod
        {
            get { return _encryptionMethod; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _encryptionMethod = value;
            }
        }

        public CipherDataElement CipherData
        {
            get { return _cipherData; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _cipherData = value;
            }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get { return _keyInfo.KeyIdentifier; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                _keyInfo.KeyIdentifier = value;
            }

        }

        public abstract void ReadExtensions(XmlDictionaryReader reader);

        public SecurityTokenSerializer TokenSerializer
        {
            get { return _keyInfoSerializer; }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("value");
                }

                _type = value;
            }
        }


        /// <summary>
        /// Reads an "EncryptedType" xmlfragment
        /// </summary>
        /// <remarks>Assumes that the reader is positioned on an "EncryptedData" or "EncryptedKey" element.
        /// Both of these elements extend EncryptedType</remarks>
        public virtual void ReadXml(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            reader.MoveToContent();

            _id = reader.GetAttribute(XmlEncryptionConstants.Attributes.Id, null);
            _type = reader.GetAttribute(XmlEncryptionConstants.Attributes.Type, null);
            _mimeType = reader.GetAttribute(XmlEncryptionConstants.Attributes.MimeType, null);
            _encoding = reader.GetAttribute(XmlEncryptionConstants.Attributes.Encoding, null);

            reader.ReadStartElement();
            reader.MoveToContent();

            // <EncryptedMethod>? 0 - 1
            if (reader.IsStartElement(XmlEncryptionConstants.Elements.EncryptionMethod, XmlEncryptionConstants.Namespace))
            {
                _encryptionMethod.ReadXml(reader);
            }

            // <KeyInfo>? 0 - 1
            reader.MoveToContent();
            if (reader.IsStartElement(XD.XmlSignatureDictionary.KeyInfo.Value, XD.XmlSignatureDictionary.Namespace.Value))
            {
                _keyInfo = new KeyInfo(_keyInfoSerializer);

                // if there is a keyInfo, we need to reset the default which is 
                // contains a single EmptyKeyInfoClause
                if (_keyInfoSerializer.CanReadKeyIdentifier(reader))
                {
                    _keyInfo.KeyIdentifier = _keyInfoSerializer.ReadKeyIdentifier(reader);
                }
                else
                {
                    _keyInfo.ReadXml(reader);
                }
            }

            // <CipherData> 1
            reader.MoveToContent();
            _cipherData.ReadXml(reader);

            ReadExtensions(reader);

            // should be on EndElement for the extended type.
            reader.MoveToContent();
            reader.ReadEndElement();
        }
    }
}
