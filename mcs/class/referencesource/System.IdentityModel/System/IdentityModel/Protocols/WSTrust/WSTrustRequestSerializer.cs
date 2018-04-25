//-----------------------------------------------------------------------
// <copyright file="WSTrustRequestSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Xml;

    /// <summary>
    /// Base class for support of versions of WS-Trust request messages.
    /// </summary>
    public abstract class WSTrustRequestSerializer
    {
        /// <summary>
        /// When overriden in the derived class deserializes the RST from the XmlReader to a RequestSecurityToken object.
        /// </summary>
        /// <param name="reader">XML reader over the RST</param>
        /// <param name="context">Current Serialization context.</param>
        /// <returns>RequestSecurityToken object if the deserialization was successful</returns>
        public abstract RequestSecurityToken ReadXml(XmlReader reader, WSTrustSerializationContext context);

        /// <summary>
        /// When overridden in the derived class reads a child element inside RST.
        /// </summary>
        /// <param name="reader">Reader pointing at an element to read inside the RST.</param>
        /// <param name="requestSecurityToken">The RequestSecurityToken element that is being populated from the reader.</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void ReadXmlElement(XmlReader reader, RequestSecurityToken requestSecurityToken, WSTrustSerializationContext context);

        /// <summary>
        /// When overriden in the derived classs writes out the supported elements on the request object. 
        /// </summary>
        /// <param name="requestSecurityToken">The request instance</param>
        /// <param name="writer">The writer to write to</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void WriteKnownRequestElement(RequestSecurityToken requestSecurityToken, XmlWriter writer, WSTrustSerializationContext context);

        /// <summary>
        /// When overriden in the derived class serializes the given RequestSecurityToken into the XmlWriter
        /// </summary>
        /// <param name="request">RequestSecurityToken object to be serialized</param>
        /// <param name="writer">XML writer to serialize into</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void WriteXml(RequestSecurityToken request, XmlWriter writer, WSTrustSerializationContext context);

        /// <summary>
        /// When overridden in the derived class writes a child element inside the RST.
        /// </summary>
        /// <param name="writer">Writer to which the RST is serialized. </param>
        /// <param name="elementName">The Local name of the element to be written.</param>
        /// <param name="elementValue">The value of the element.</param>
        /// <param name="requestSecurityToken">The entire RST object that is being serialized.</param>
        /// <param name="context">Current Serialization context.</param>
        public abstract void WriteXmlElement(XmlWriter writer, string elementName, object elementValue, RequestSecurityToken requestSecurityToken, WSTrustSerializationContext context);

        /// <summary>
        /// Creates an instance of the RequestSecurityToken object that this class can Serialize or Deserialize.
        /// </summary>
        /// <returns>Instance of RequestSecurityToken object</returns>
        public virtual RequestSecurityToken CreateRequestSecurityToken()
        {
            return new RequestSecurityToken();
        }

        /// <summary>
        /// Validates the RequestSecurityToken object that has been deserialized.
        /// </summary>
        /// <param name="requestSecurityToken">The RequestSecurityToken object to Validate.</param>
        /// <exception cref="InvalidOperationException">An Issue Request for an Asymmetric Key did not specify UseKey.</exception>
        public virtual void Validate(RequestSecurityToken requestSecurityToken)
        {
            if (requestSecurityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rst");
            }

            // Validate the RequestSecurityToken required parameters.
            if ((StringComparer.Ordinal.Equals(requestSecurityToken.RequestType, RequestTypes.Issue) || requestSecurityToken.RequestType == null) &&
                 StringComparer.Ordinal.Equals(requestSecurityToken.KeyType, KeyTypes.Asymmetric) &&
                 ((requestSecurityToken.UseKey == null) || (requestSecurityToken.UseKey.SecurityKeyIdentifier == null && requestSecurityToken.UseKey.Token == null)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID3091)));
            }
        }

        /// <summary>
        /// When implemented in the derived class checks if the given reader is positioned at a RequestSecurityToken element.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <returns>'True' if the reader is positioned at an RST element that this serializer can read.</returns>
        public abstract bool CanRead(XmlReader reader);

        /// <summary>
        /// When overriden in the derived classs reads a custom element. 
        /// </summary>
        /// <param name="reader">The reader on the current element.</param>
        /// <param name="context">Current Serialization context.</param>
        protected virtual void ReadCustomElement(XmlReader reader, WSTrustSerializationContext context)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ID2072, reader.LocalName)));
        }
    }
}
