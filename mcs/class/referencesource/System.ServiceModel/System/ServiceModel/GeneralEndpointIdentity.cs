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

    internal class GeneralEndpointIdentity : EndpointIdentity
    {
        public GeneralEndpointIdentity(Claim identityClaim)
        {
            base.Initialize(identityClaim);
        }
    }
}
