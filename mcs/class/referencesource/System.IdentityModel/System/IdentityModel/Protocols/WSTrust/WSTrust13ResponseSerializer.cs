//-----------------------------------------------------------------------
// <copyright file="WSTrust13ResponseSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Xml;

    /// <summary>
    /// Class for serializing a WS-Trust 1.3 RequestSecurityTokenResponse to an XmlWriter
    /// </summary>
    public class WSTrust13ResponseSerializer : WSTrustResponseSerializer
    {
        /// <summary>
        /// Deserializes an RSTR and returns a RequestSecurityTokenRespone object.
        /// </summary>
        /// <param name="reader">Reader over the RSTR.</param>
        /// <param name="context">Current Serialization context.</param>
        /// <returns>RequestSecurityTokenResponse object if deserialization was successful.</returns>
        /// <exception cref="ArgumentNullException">The given reader or context parameter is null</exception>
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

            bool isFinal = false;
            if (reader.IsStartElement(WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection, WSTrust13Constants.NamespaceURI))
            {
                reader.ReadStartElement(WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection, WSTrust13Constants.NamespaceURI);
                isFinal = true;
            }

            RequestSecurityTokenResponse rstr = WSTrustSerializationHelper.CreateResponse(reader, context, this, WSTrustConstantsAdapter.Trust13);
            rstr.IsFinal = isFinal;

            if (isFinal)
            {
                reader.ReadEndElement();
            }

            return rstr;
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

            if (reader.IsStartElement(WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI))
            {
                rstr.KeyWrapAlgorithm = reader.ReadElementContentAsString();
                return;
            }

            WSTrustSerializationHelper.ReadRSTRXml(reader, rstr, context, WSTrustConstantsAdapter.Trust13);
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

            // Write out the exisiting ones
            WSTrustSerializationHelper.WriteKnownResponseElement(rstr, writer, context, this, WSTrustConstantsAdapter.Trust13);

            // Specific to WS-Trust 13
            if (!string.IsNullOrEmpty(rstr.KeyWrapAlgorithm))
            {
                this.WriteXmlElement(writer, WSTrust13Constants.ElementNames.KeyWrapAlgorithm, rstr.KeyWrapAlgorithm, rstr, context);
            }
        }

        /// <summary>
        /// Serializes a RequestSecurityTokenResponse object to the given XmlWriter
        /// stream.
        /// </summary>
        /// <param name="response">RequestSecurityTokenResponse object that needs to be serialized to the writer.</param>
        /// <param name="writer">XmlWriter into which the object will be serialized</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">The given response or writer or context parameter is null</exception>
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

            if (response.IsFinal)
            {
                writer.WriteStartElement(WSTrust13Constants.Prefix, WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection, WSTrust13Constants.NamespaceURI);
            }

            WSTrustSerializationHelper.WriteResponse(response, writer, context, this, WSTrustConstantsAdapter.Trust13);

            if (response.IsFinal)
            {
                writer.WriteEndElement();
            }
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

            if (StringComparer.Ordinal.Equals(elementName, WSTrust13Constants.ElementNames.KeyWrapAlgorithm))
            {
                writer.WriteElementString(WSTrust13Constants.Prefix, WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI, (string)elementValue);
                return;
            }

            WSTrustSerializationHelper.WriteRSTRXml(writer, elementName, elementValue, context, WSTrustConstantsAdapter.Trust13);
        }

        /// <summary>
        /// Checks if the given reader is positioned at a RequestSecurityTokenResponse or 
        /// RequestSecurityTokenResponseCollection element with namespace 'http://docs.oasis-open.org/ws-sx/ws-trust/200512'
        /// </summary>
        /// <param name="reader">The reader to read from</param>
        /// <returns>
        /// 'True' if the reader is positioned at a RequestSecurityTokenResponse or RequestSecurityTokenResponseCollection 
        /// element with namespace 'http://docs.oasis-open.org/ws-sx/ws-trust/200512'.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input argument is null.</exception>
        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(WSTrust13Constants.ElementNames.RequestSecurityTokenResponseCollection, WSTrust13Constants.NamespaceURI)
                || reader.IsStartElement(WSTrust13Constants.ElementNames.RequestSecurityTokenResponse, WSTrust13Constants.NamespaceURI);
        }
    }
}
