//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Xml;
    using System.Xml.Serialization;

    public class DnsEndpointIdentity : EndpointIdentity
    {
        public DnsEndpointIdentity(string dnsName)
        {
            if (dnsName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dnsName");

            base.Initialize(Claim.CreateDnsClaim(dnsName));
        }

        public DnsEndpointIdentity(Claim identity)
        {
            if (identity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");

            // PreSharp Bug: Parameter 'identity.ResourceType' to this public method must be validated: A null-dereference can occur here.
#pragma warning suppress 56506 // Claim.ClaimType will never return null
            if (!identity.ClaimType.Equals(ClaimTypes.Dns))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.UnrecognizedClaimTypeForIdentity, identity.ClaimType, ClaimTypes.Dns));

            base.Initialize(identity);
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            writer.WriteElementString(XD.AddressingDictionary.Dns, XD.AddressingDictionary.IdentityExtensionNamespace, (string)this.IdentityClaim.Resource);
        }
    }
}
