//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Protocols.WSTrust;
    using System.ServiceModel.Channels;
    using System.Xml;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;

    /// <summary>
    /// Defines a Body Writer that writes out the RSTR to a outgoing message.
    /// </summary>
    public class WSTrustResponseBodyWriter : BodyWriter
    {
        WSTrustResponseSerializer _serializer;
        RSTR _rstr;
        WSTrustSerializationContext _context;

        /// <summary>
        /// Initializes an instance of <see cref="WSTrustResponseBodyWriter"/>
        /// </summary>
        /// <param name="requestSecurityTokenResponse">The Response object that can write the body contents.</param>
        /// <param name="serializer">Serializer to use for serializing the RSTR.</param>
        /// <param name="context">The <see cref="WSTrustSerializationContext"/> of this request.</param>
        /// <exception cref="ArgumentNullException">serializer parameter is null.</exception>
        public WSTrustResponseBodyWriter(RSTR requestSecurityTokenResponse, WSTrustResponseSerializer serializer, WSTrustSerializationContext context)
            : base( true )
        {
            if ( serializer == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "serializer" );
            }

            if ( requestSecurityTokenResponse == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestSecurityTokenResponse");
            }

            if ( context == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "context" );
            }

            _serializer = serializer;
            _rstr = requestSecurityTokenResponse;
            _context = context;
        }

        /// <summary>
        /// Override of the base class method. Serializes the RSTR to the outgoing stream.
        /// </summary>
        /// <param name="writer">Writer to which the RSTR should be written.</param>
        protected override void OnWriteBodyContents( XmlDictionaryWriter writer )
        {
            _serializer.WriteXml( _rstr, writer, _context );
        }
    }
}
