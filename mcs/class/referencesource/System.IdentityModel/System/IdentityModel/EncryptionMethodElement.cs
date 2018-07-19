//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IdentityModel.Diagnostics;
    using System.Xml;
    
    internal class EncryptionMethodElement
    {
        private string _algorithm;
        private string _parameters;

        public string Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value; }
        }

        public string Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public void ReadXml( XmlDictionaryReader reader )
        {
            if ( reader == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "reader" );
            }

            reader.MoveToContent();
            if ( !reader.IsStartElement( XmlEncryptionConstants.Elements.EncryptionMethod, XmlEncryptionConstants.Namespace ) )
            {
                return;
            }

            _algorithm = reader.GetAttribute( XmlEncryptionConstants.Attributes.Algorithm, null );

            if ( !reader.IsEmptyElement )
            {
                //
                // Trace unread missing element
                //

                string xml = reader.ReadOuterXml();
                if ( DiagnosticUtility.ShouldTraceWarning )
                {
                    TraceUtility.TraceString( System.Diagnostics.TraceEventType.Warning, SR.GetString( SR.ID8024, reader.Name, reader.NamespaceURI, xml ) );
                }
            }
            else
            {
                //
                // Read to the next element
                //
                reader.Read();
            }
        }

        public void WriteXml( XmlWriter writer )
        {
            writer.WriteStartElement( XmlEncryptionConstants.Prefix, XmlEncryptionConstants.Elements.EncryptionMethod, XmlEncryptionConstants.Namespace );

            writer.WriteAttributeString( XmlEncryptionConstants.Attributes.Algorithm, null, _algorithm );

            // <EncryptionMethod>

            writer.WriteEndElement(); 
        }

    }
}
