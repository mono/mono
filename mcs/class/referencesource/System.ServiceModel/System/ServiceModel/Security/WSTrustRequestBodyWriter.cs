//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;
    using System.Diagnostics;
    using System.IdentityModel.Protocols.WSTrust;

    /// <summary>
    /// Defines a Body Writer that writes out a RequestSecurityToken into an XmlDictionaryWriter.
    /// </summary>
    public class WSTrustRequestBodyWriter : BodyWriter
    {
        WSTrustSerializationContext _serializationContext;
        System.IdentityModel.Protocols.WSTrust.RequestSecurityToken _requestSecurityToken;
        WSTrustRequestSerializer _serializer;

        /// <summary>
        /// Constructor for the WSTrustRequestBodyWriter.
        /// </summary>
        /// <param name="requestSecurityToken">The RequestSecurityToken object to be serialized in the outgoing Message.</param>
        /// <param name="serializer">Serializer is responsible for writting the requestSecurityToken into a XmlDictionaryWritter.</param>
        /// <param name="serializationContext">Context for the serialization.</param>
        /// <exception cref="ArgumentNullException">The 'requestSecurityToken' is null.</exception>
        /// <exception cref="ArgumentNullException">The 'serializer' is null.</exception>
        /// <exception cref="ArgumentNullException">The 'serializationContext' is null.</exception>
        public WSTrustRequestBodyWriter(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken requestSecurityToken, WSTrustRequestSerializer serializer, WSTrustSerializationContext serializationContext)
            : base(true)
        {
            if (requestSecurityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSecurityToken");
            }

            if (serializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }

            if (serializationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializationContext");
            }

            _requestSecurityToken = requestSecurityToken;
            _serializer = serializer;
            _serializationContext = serializationContext;
        }

        /// <summary>
        /// Override of the base class method. Serializes the requestSecurityToken to the outgoing stream.
        /// </summary>
        /// <param name="writer">Writer into which the requestSecurityToken should be written.</param>
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            _serializer.WriteXml(_requestSecurityToken, writer, _serializationContext);
        }
    }

}
