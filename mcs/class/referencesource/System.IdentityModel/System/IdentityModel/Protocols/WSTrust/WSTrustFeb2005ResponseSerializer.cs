//-----------------------------------------------------------------------
// <copyright file="WSTrustFeb2005ResponseSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Xml;

    /// <summary>
    /// Class for serializing a WS-Trust Feb 2005 RequestSecurityTokenResponse to an XmlWriter
    /// </summary>
    public class WSTrustFeb2005ResponseSerializer : WSTrustResponseSerializer
    {
        /// <summary>
        /// Deserializes the RSTR from the XmlReader to a RequestSecurityTokenResponse object.
        /// </summary>
        /// <param name="reader">XML reader over the RSTR</param>
        /// <param name="context">Current Serialization context.</param>
        /// <returns>RequestSecurityTokenResponse object if the deserialization was successful</returns>
        /// <exception cref="ArgumentNullException">The given reader or context parameter is null</exception>
        /// <exception cref="WSTrustSerializationException">There was an error parsing the RSTR</exception>
        public override RequestSecurityTokenResponse ReadXml(XmlReader reader, WSTrustSerializationContext context)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            return WSTrustSerializationHelper.CreateResponse(reader, context, this, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Override of the base class that Reads a specific child element inside the RSTR.
        /// </summary>
        /// <param name="reader">Reader pointing at an element to read inside the RSTR.</param>
        /// <param name="rstr">The RequestSecurityTokenResponse element that is being populated from the reader.</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either reader or rstr or context parameter is null.</exception>
        /// <exception cref="WSTrustSerializationException">Unable to deserialize the current parameter.</exception>
        public override void ReadXmlElement(XmlReader reader, RequestSecurityTokenResponse rstr, WSTrustSerializationContext context)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            if (rstr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.ReadRSTRXml(reader, rstr, context, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Writes out the supported elements on the response object. 
        /// </summary>
        /// <param name="rstr">The response instance</param>
        /// <param name="writer">The writer to write to</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either rstr or writer or context parameter is null.</exception>
        public override void WriteKnownResponseElement(RequestSecurityTokenResponse rstr, XmlWriter writer, WSTrustSerializationContext context)
        {
            if (rstr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.WriteKnownResponseElement(rstr, writer, context, this, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Serializes the given RequestSecurityTokenResponse into the XmlWriter
        /// </summary>
        /// <param name="response">RequestSecurityTokenRespone object to be serialized</param>
        /// <param name="writer">XML writer to serialize into</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">The response or writer or context parameter is null.</exception>
        public override void WriteXml(RequestSecurityTokenResponse response, XmlWriter writer, WSTrustSerializationContext context)
        {
            if (response == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("response");
            }

            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.WriteResponse(response, writer, context, this, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Override of the Base class method that writes a specific RSTR parameter to the outgoing stream.
        /// </summary>
        /// <param name="writer">Writer to which the RSTR is serialized</param>
        /// <param name="elementName">The Local name of the element to be written.</param>
        /// <param name="elementValue">The value of the element.</param>
        /// <param name="rstr">The entire RSTR object that is being serialized.</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either writer or rstr or context is null.</exception>
        /// <exception cref="ArgumentException">elementName is null or an empty string.</exception>
        public override void WriteXmlElement(XmlWriter writer, string elementName, object elementValue, RequestSecurityTokenResponse rstr, WSTrustSerializationContext context)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }

            if (string.IsNullOrEmpty(elementName))
            {
                throw DiagnosticUtility.ThrowHelperArgumentNullOrEmptyString("elementName");
            }

            if (rstr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            WSTrustSerializationHelper.WriteRSTRXml(writer, elementName, elementValue, context, WSTrustConstantsAdapter.TrustFeb2005);
        }

        /// <summary>
        /// Checks if the given reader is positioned at a RequestSecurityTokenResponse element with namespace
        /// 'http://schemas.xmlsoap.org/ws/2005/02/trust'
        /// </summary>
        /// <param name="reader">The reader to read from</param>
        /// <returns>
        /// 'True' if the reader is positioned at a RequestSecurityTokenResponse element with namespace
        /// 'http://schemas.xmlsoap.org/ws/2005/02/trust'.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input argument is null.</exception>
        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(WSTrustFeb2005Constants.ElementNames.RequestSecurityTokenResponse, WSTrustFeb2005Constants.NamespaceURI);
        }
    }
}
