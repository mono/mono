//-----------------------------------------------------------------------
// <copyright file="WSTrust13RequestSerializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Protocols.WSTrust
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Xml;

    /// <summary>
    /// Class for deserializing a WSTrust 1.3 RequestSecurityToken from an XmlReader
    /// </summary>
    public class WSTrust13RequestSerializer : WSTrustRequestSerializer
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

            return WSTrustSerializationHelper.CreateRequest(reader, context, this, WSTrustConstantsAdapter.Trust13);
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

            // special case SecondaryParameters, they cannot be embeded as per WS-Trust 1.3
            if (reader.IsStartElement(WSTrust13Constants.ElementNames.SecondaryParameters, WSTrust13Constants.NamespaceURI))
            {
                rst.SecondaryParameters = this.ReadSecondaryParameters(reader, context);
                return;
            }

            if (reader.IsStartElement(WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI))
            {
                rst.KeyWrapAlgorithm = reader.ReadElementContentAsString();
                if (!UriUtil.CanCreateValidUri(rst.KeyWrapAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI, rst.KeyWrapAlgorithm)));
                }

                return;
            }

            if (reader.IsStartElement(WSTrust13Constants.ElementNames.ValidateTarget, WSTrust13Constants.NamespaceURI))
            {
                if (!reader.IsEmptyElement)
                {
                    rst.ValidateTarget = new SecurityTokenElement(WSTrustSerializationHelper.ReadInnerXml(reader), context.SecurityTokenHandlers);
                }

                if (rst.ValidateTarget == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3221)));
                }

                return;
            }

            WSTrustSerializationHelper.ReadRSTXml(reader, rst, context, WSTrustConstantsAdapter.Trust13);
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

            // Write out the exisiting ones
            WSTrustSerializationHelper.WriteKnownRequestElement(rst, writer, context, this, WSTrustConstantsAdapter.Trust13);

            // Specific to WS-Trust 13
            if (!string.IsNullOrEmpty(rst.KeyWrapAlgorithm))
            {
                if (!UriUtil.CanCreateValidUri(rst.KeyWrapAlgorithm, UriKind.Absolute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WSTrustSerializationException(SR.GetString(SR.ID3135, WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI, rst.KeyWrapAlgorithm)));
                }

                this.WriteXmlElement(writer, WSTrust13Constants.ElementNames.KeyWrapAlgorithm, rst.KeyWrapAlgorithm, rst, context);
            }

            if (rst.SecondaryParameters != null)
            {
                this.WriteXmlElement(writer, WSTrust13Constants.ElementNames.SecondaryParameters, rst.SecondaryParameters, rst, context);
            }

            if (rst.ValidateTarget != null)
            {
                this.WriteXmlElement(writer, WSTrust13Constants.ElementNames.ValidateTarget, rst.ValidateTarget, rst, context);
            }
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

            WSTrustSerializationHelper.WriteRequest(request, writer, context, this, WSTrustConstantsAdapter.Trust13);
        }

        /// <summary>
        /// Override of the Base class method that writes a specific RST parameter to the outgoing stream.
        /// </summary>
        /// <param name="writer">Writer to which the </param>
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

            // Write out the WSTrust13 specific elements
            if (StringComparer.Ordinal.Equals(elementName, WSTrust13Constants.ElementNames.SecondaryParameters))
            {
                RequestSecurityToken secondaryParameters = elementValue as RequestSecurityToken;

                if (secondaryParameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2064, WSTrust13Constants.ElementNames.SecondaryParameters)));
                }

                // WS-Trust 13 spec does not allow this
                if (secondaryParameters.SecondaryParameters != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID2055)));
                }

                writer.WriteStartElement(WSTrust13Constants.Prefix, WSTrust13Constants.ElementNames.SecondaryParameters, WSTrust13Constants.NamespaceURI);

                // write out the known elements inside the rst.SecondaryParameters
                this.WriteKnownRequestElement(secondaryParameters, writer, context);

                // Write the custom elements here from the rst.SecondaryParameters.Elements bag
                foreach (KeyValuePair<string, object> messageParam in secondaryParameters.Properties)
                {
                    this.WriteXmlElement(writer, messageParam.Key, messageParam.Value, rst, context);
                }

                // close out the SecondaryParameters element
                writer.WriteEndElement();
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, WSTrust13Constants.ElementNames.KeyWrapAlgorithm))
            {
                writer.WriteElementString(WSTrust13Constants.Prefix, WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI, (string)elementValue);
                return;
            }

            if (StringComparer.Ordinal.Equals(elementName, WSTrust13Constants.ElementNames.ValidateTarget))
            {
                SecurityTokenElement tokenElement = elementValue as SecurityTokenElement;

                if (tokenElement == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("elementValue", SR.GetString(SR.ID3222, WSTrust13Constants.ElementNames.ValidateTarget, WSTrust13Constants.NamespaceURI, typeof(SecurityTokenElement), elementValue));
                }

                writer.WriteStartElement(WSTrust13Constants.Prefix, WSTrust13Constants.ElementNames.ValidateTarget, WSTrust13Constants.NamespaceURI);
                if (tokenElement.SecurityTokenXml != null)
                {
                    tokenElement.SecurityTokenXml.WriteTo(writer);
                }
                else
                {
                    context.SecurityTokenHandlers.WriteToken(writer, tokenElement.GetSecurityToken());
                }

                writer.WriteEndElement();
                return;
            }

            WSTrustSerializationHelper.WriteRSTXml(writer, elementName, elementValue, context, WSTrustConstantsAdapter.Trust13);
        }

        /// <summary>
        /// Checks if the given reader is positioned at a RequestSecurityToken element with namespace
        /// 'http://docs.oasis-open.org/ws-sx/ws-trust/200512'
        /// </summary>
        /// <param name="reader">The reader to read from</param>
        /// <returns>
        /// 'True' if the reader is positioned at a RequestSecurityToken element with namespace
        /// 'http://docs.oasis-open.org/ws-sx/ws-trust/200512'.
        /// </returns>
        /// <exception cref="ArgumentNullException">The input argument is null.</exception>
        public override bool CanRead(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }

            return reader.IsStartElement(WSTrust13Constants.ElementNames.RequestSecurityToken, WSTrust13Constants.NamespaceURI);
        }

        /// <summary>
        /// Special case for reading SecondaryParameters inside a WS-Trust 1.3 RST.  The specification states that a SecondaryParameters element
        /// cannot be inside a SecondaryParameters element.  Override this method to provide custom processing.
        /// </summary>
        /// <param name="reader">Reader pointing at the SecondaryParameters element inside the RST.</param>
        /// <param name="context">Current Serialization context.</param>
        /// <exception cref="ArgumentNullException">Either reader or context parameter is null.</exception>
        /// <exception cref="WSTrustSerializationException">An inner 'SecondaryParameter' element was found while processing the outer 'SecondaryParameter'.</exception>
        /// <returns>RequestSecurityToken that contains the SecondaryParameters found in the RST</returns>
        protected virtual RequestSecurityToken ReadSecondaryParameters(
                                                  XmlReader reader,
                                                  WSTrustSerializationContext context)
        {
            RequestSecurityToken secondaryParameters = CreateRequestSecurityToken();

            if (reader.IsEmptyElement)
            {
                reader.Read();
                reader.MoveToContent();
                return secondaryParameters;
            }

            reader.ReadStartElement();
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(
                    WSTrust13Constants.ElementNames.KeyWrapAlgorithm, WSTrust13Constants.NamespaceURI))
                {
                    secondaryParameters.KeyWrapAlgorithm = reader.ReadElementContentAsString();
                    if (!UriUtil.CanCreateValidUri(secondaryParameters.KeyWrapAlgorithm, UriKind.Absolute))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new WSTrustSerializationException(
                                SR.GetString(
                                    SR.ID3135,
                                    WSTrust13Constants.ElementNames.KeyWrapAlgorithm,
                                    WSTrust13Constants.NamespaceURI,
                                    secondaryParameters.KeyWrapAlgorithm)));
                    }
                }
                else if (reader.IsStartElement(
                    WSTrust13Constants.ElementNames.SecondaryParameters, WSTrust13Constants.NamespaceURI))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new WSTrustSerializationException(SR.GetString(SR.ID3130)));
                }
                else
                {
                    WSTrustSerializationHelper.ReadRSTXml(
                        reader,
                        secondaryParameters,
                        context,
                        WSTrustConstantsAdapter.GetConstantsAdapter(reader.NamespaceURI) ?? WSTrustConstantsAdapter.TrustFeb2005);
                }
            }

            reader.ReadEndElement();

            return secondaryParameters;
        }
    }
}
