//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class CipherDataElement
    {
        byte[] _iv;
        byte[] _cipherText;

        public byte[] CipherValue
        {
            get
            {
                if ( _iv != null )
                {
                    byte[] buffer = new byte[_iv.Length + _cipherText.Length];
                    Buffer.BlockCopy( _iv, 0, buffer, 0, _iv.Length );
                    Buffer.BlockCopy( _cipherText, 0, buffer, _iv.Length, _cipherText.Length );
                    _iv = null;
                }

                return _cipherText;
            }
            set
            {
                _cipherText = value;
            }
        }

        public void ReadXml( XmlDictionaryReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            reader.MoveToContent();
            if ( !reader.IsStartElement( XmlEncryptionConstants.Elements.CipherData, XmlEncryptionConstants.Namespace ) )
            {
                throw DiagnosticUtility.ThrowHelperXml( reader, SR.GetString( SR.ID4188 ) );
            }

            reader.ReadStartElement( XmlEncryptionConstants.Elements.CipherData, XmlEncryptionConstants.Namespace );
            reader.ReadStartElement( XmlEncryptionConstants.Elements.CipherValue, XmlEncryptionConstants.Namespace );

            _cipherText = reader.ReadContentAsBase64();
            _iv         = null;

            // <CipherValue>
            reader.MoveToContent();           
            reader.ReadEndElement();

            
            // <CipherData>
            reader.MoveToContent();
            reader.ReadEndElement(); 
        }

        public void SetCipherValueFragments( byte[] iv, byte[] cipherText )
        {
            _iv         = iv;
            _cipherText = cipherText;
        }

        public void WriteXml( XmlWriter writer )
        {
            writer.WriteStartElement( XmlEncryptionConstants.Prefix, XmlEncryptionConstants.Elements.CipherData, XmlEncryptionConstants.Namespace );
            writer.WriteStartElement( XmlEncryptionConstants.Prefix, XmlEncryptionConstants.Elements.CipherValue, XmlEncryptionConstants.Namespace );

            if ( _iv != null )
                writer.WriteBase64( _iv, 0, _iv.Length );

            writer.WriteBase64( _cipherText, 0, _cipherText.Length );

            writer.WriteEndElement(); // CipherValue
            writer.WriteEndElement(); // CipherData
        }
    }
}
