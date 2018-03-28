//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Xml;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    class BufferedGenericXmlSecurityToken : GenericXmlSecurityToken
    {
        XmlBuffer tokenXmlBuffer;

        public BufferedGenericXmlSecurityToken(
            XmlElement tokenXml,
            SecurityToken proofToken,
            DateTime effectiveTime,
            DateTime expirationTime,
            SecurityKeyIdentifierClause internalTokenReference,
            SecurityKeyIdentifierClause externalTokenReference,
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies,
            XmlBuffer tokenXmlBuffer
            )
            : base(tokenXml, proofToken, effectiveTime, expirationTime, internalTokenReference, externalTokenReference, authorizationPolicies)
        {
            this.tokenXmlBuffer = tokenXmlBuffer;
        }

        public XmlBuffer TokenXmlBuffer
        {
            get { return this.tokenXmlBuffer; }
        }
    }
}
