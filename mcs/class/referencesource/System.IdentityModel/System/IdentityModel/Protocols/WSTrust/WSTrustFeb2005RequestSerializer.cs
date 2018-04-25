//-----------------------------------------------------------------------
// <copyright file="WSTrustFeb2005RequestSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Xml;

    /// <summary>
    /// Class for deserializing a WS-Trust Feb 2005 RequestSecurityToken from an XmlReader
    /// </summary>
    public class WSTrustFeb2005RequestSerializer : WSTrustRequestSerializer
    {
        /// <summary>
        /// Deserializes the RST from the XmlReader to a RequestSecurityToken object.
        /// </summary>
        /// <param name="reader">XML reader over the RST</param>
        /// <param name="context">Current Serialization context.</param>
        /// <returns>RequestSecurityToken object if the deserialization was successful</returns>
        /// <exception cref="ArgumentNullException">The reader or context parameter is null</exception>
        /// <exception cref="WSTrustSerializationException">There was an error parsing the RST</exception>
        public override RequestSecurityToken ReadXml(XmlReader reader, WSTrustSerializationContext context)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            return WSTrustSerializationHelper.CreateRequest(reader, context, this, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Override of the base class that reads a child element inside the RST
        /// </summary>
        /// <param name="reader">Reader pointing at an element to read inside the RST.</param>
        /// <param name="rst">The RequestSecurityToken element that is being populated from the reader.</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either reader or rst or context parameter is null.</exception>
        /// <exception cref="WSTrustSerializationException">Unable to deserialize the current parameter.</exception>
        public override void ReadXmlElement(XmlReader reader, RequestSecurityToken rst, WSTrustSerializationContext context)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (rst == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.ReadRSTXml(reader, rst, context, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Writes out the supported elements on the request object. Override this method if someone 
        /// has sub-class the RequestSecurityToken class and added more property to it.
        /// </summary>
        /// <param name="rst">The request instance</param>
        /// <param name="writer">The writer to write to</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either rst or writer or context parameter is null.</exception>
        public override void WriteKnownRequestElement(RequestSecurityToken rst, XmlWriter writer, WSTrustSerializationContext context)
        {
            if (rst == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.WriteKnownRequestElement(rst, writer, context, this, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Serializes the given RequestSecurityToken into the XmlWriter
        /// </summary>
        /// <param name="request">RequestSecurityToken object to be serialized</param>
        /// <param name="writer">XML writer to serialize into</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">The request or writer or context parameter is null.</exception>
        public override void WriteXml(RequestSecurityToken request, XmlWriter writer, WSTrustSerializationContext context)
        {
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.WriteRequest(request, writer, context, this, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Override of the Base class method that writes a specific RST parameter to the outgoing stream.
        /// </summary>
        /// <param name="writer">Writer to which the RST is serialized. </param>
        /// <param name="elementName">The Local name of the element to be written.</param>
        /// <param name="elementValue">The value of the element.</param>
        /// <param name="rst">The entire RST object that is being serialized.</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either writer or rst or context is null.</exception>
        /// <exception cref="ArgumentException">elementName is null or an empty string.</exception>
        public override void WriteXmlElement(XmlWriter writer, string elementName, object elementValue, RequestSecurityToken rst, WSTrustSerializationContext context)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (string.IsNullOrEmpty(elementName))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("elementName");
            }

            if (rst == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.WriteRSTXml(writer, elementName, elementValue, context, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Checks if the given reader is positioned at a RequestSecurityToken element with namespace
        /// 'http://schemas.xmlsoap.org/ws/2005/02/trust'
        /// </summary>
        /// <param name="reader">The reader to read from</param>
        /// <returns>
        /// 'True' if the reader is positioned at a RequestSecurityToken element with namespace
        /// 'http://schemas.xmlsoap.org/ws/2005/02/trust'.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input argument is null.</exception>
        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(WSTrustFeb2005Constants.ElementNames.RequestSecurityToken, WSTrustFeb2005Constants.NamespaceURI);
        }
    }
}
