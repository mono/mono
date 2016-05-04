//-----------------------------------------------------------------------
// <copyright file="WSTrustResponseSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Xml;

    /// <summary>
    /// Base class for support of versions of WS-Trust request messages.
    /// </summary>
    public abstract class WSTrustResponseSerializer
    {
        /// <summary>
        /// When overriden in the derived class deserializes the RSTR from the XmlReader to a RequestSecurityTokenResponse object.
        /// </summary>
        /// <param name="reader">XML reader over the RSTR</param>
        /// <param name="context">Current Serialization context.</param>
        /// <returns>RequestSecurityTokenResponse object if teh deserialization was successful</returns>
        public abstract RequestSecurityTokenResponse ReadXml(XmlReader reader, WSTrustSerializationContext context);

        /// <summary>
        /// When overridden in the derived class reads a child element inside RSTR.
        /// </summary>
        /// <param name="reader">Reader pointing at an element to read inside the RSTR.</param>
        /// <param name="requestSecurityTokenResponse">The RequestSecurityTokenResponse element that is being populated from the reader.</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void ReadXmlElement(XmlReader reader, RequestSecurityTokenResponse requestSecurityTokenResponse, WSTrustSerializationContext context);

        /// <summary>
        /// When overridden in the derived class writes out the supported elements on the response object. 
        /// </summary>
        /// <param name="requestSecurityTokenResponse">The response instance</param>
        /// <param name="writer">The writer to write to</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void WriteKnownResponseElement(RequestSecurityTokenResponse requestSecurityTokenResponse, XmlWriter writer, WSTrustSerializationContext context);

        /// <summary>
        /// When overriden in the derived class serializes the given RequestSecurityTokenResponse into the XmlWriter
        /// </summary>
        /// <param name="response">RequestSecurityTokenRespone object to be serializes</param>
        /// <param name="writer">XML writer to serialize into</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void WriteXml(RequestSecurityTokenResponse response, XmlWriter writer, WSTrustSerializationContext context);

        /// <summary>
        /// When overridden in the derived class writes a specific RSTR parameter to the outgoing stream.
        /// </summary>
        /// <param name="writer">Writer to which the RSTR is serialized</param>
        /// <param name="elementName">The Local name of the element to be written.</param>
        /// <param name="elementValue">The value of the element.</param>
        /// <param name="requestSecurityTokenResponse">The entire RSTR object that is being serialized.</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void WriteXmlElement(XmlWriter writer, string elementName, object elementValue, RequestSecurityTokenResponse requestSecurityTokenResponse, WSTrustSerializationContext context);

        /// <summary>
        /// Creates an instance of the RequestSecurityTokenResponse object that this class can Serialize or Deserialize.
        /// </summary>
        /// <returns>Instance of RequestSecurityTokenResponse object</returns>
        public virtual RequestSecurityTokenResponse CreateInstance()
        {
            return new RequestSecurityTokenResponse();
        }

        /// <summary>
        /// Validates the RequestSecurityTokenResponse object that has been deserialized.
        /// </summary>
        /// <param name="requestSecurityTokenResponse">The RequestSecurityTokenResponse object to Validate.</param>
        /// <exception cref="InvalidOperationException">An Response for an IssueRequest does not contain the RequestedSecurityToken.</exception>
        public virtual void Validate(RequestSecurityTokenResponse requestSecurityTokenResponse)
        {
            if (requestSecurityTokenResponse == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstr");
            }
        }

        /// <summary>
        /// When implemented in the derived class checks if the given reader is positioned at a RequestSecurityTokenResponse element.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>'True' if the reader is positioned at an RSTR element that this serializer can read.</returns>
        public abstract bool CanRead(XmlReader reader);
    }
}
