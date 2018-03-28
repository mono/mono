//-----------------------------------------------------------------------
// <copyright file="EndpointReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Collections.ObjectModel;
    using System.Xml;

    public class EndpointReference
    {
        Collection<XmlElement> _details = new Collection<XmlElement>();

        Uri uri;

        public Collection<XmlElement> Details
        {
            get
            {
                return _details;
            }
        }
        
        public EndpointReference(string uri)
        {
            if (uri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("uri");
            }

            Uri tempUri = new Uri(uri);

            if (!tempUri.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("uri", SR.GetString(SR.ID0013));
            }

            this.uri = tempUri;
        }

        public Uri Uri
        {
            get
            {
                return uri;
            }
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            writer.WriteStartElement(WSAddressing10Constants.Prefix, WSAddressing10Constants.Elements.EndpointReference, WSAddressing10Constants.NamespaceUri);

            writer.WriteStartElement(WSAddressing10Constants.Prefix, WSAddressing10Constants.Elements.Address, WSAddressing10Constants.NamespaceUri);
            writer.WriteString(this.Uri.AbsoluteUri);
            writer.WriteEndElement();
            foreach ( XmlElement element in _details )
            {
                element.WriteTo( writer );
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Reads an <see cref="EndpointReference"/> from xml reader.
        /// </summary>
        /// <param name="reader">The xml reader.</param>
        /// <returns>An <see cref="EndpointReference"/> instance.</returns>
        public static EndpointReference ReadFrom(XmlReader reader)
        {
            return ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        /// <summary>
        /// Reads an <see cref="EndpointReference"/> from xml dictionary reader. The addressing version is defaulted to
        /// <see cref="WSAddressing10Constants.Elements.Address"/>.
        /// </summary>
        /// <param name="reader">The xml dictionary reader.</param>
        /// <returns>An <see cref="EndpointReference"/> instance.</returns>
        public static EndpointReference ReadFrom(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            reader.ReadFullStartElement();
            reader.MoveToContent();

            if (reader.IsNamespaceUri(WSAddressing10Constants.NamespaceUri) || reader.IsNamespaceUri(WSAddressing200408Constants.NamespaceUri))
            {
                if (reader.IsStartElement(WSAddressing10Constants.Elements.Address, WSAddressing10Constants.NamespaceUri) ||
                    reader.IsStartElement(WSAddressing10Constants.Elements.Address, WSAddressing200408Constants.NamespaceUri))
                {
                    EndpointReference er = new EndpointReference(reader.ReadElementContentAsString());
                    while ( reader.IsStartElement() )
                    {
                        bool emptyElement = reader.IsEmptyElement;
                        
                        XmlReader subtreeReader = reader.ReadSubtree();
                        XmlDocument doc = new XmlDocument();
                        doc.PreserveWhitespace = true;
                        doc.Load( subtreeReader );
                        er._details.Add( doc.DocumentElement );
                        if ( !emptyElement )
                        {
                            reader.ReadEndElement();
                        }
                    }

                    reader.ReadEndElement();
                    return er;
                }
            }

            return null;
        }
    }
}
