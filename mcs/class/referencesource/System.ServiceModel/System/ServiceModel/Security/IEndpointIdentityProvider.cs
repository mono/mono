//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;
    using System.ServiceModel;

    public interface IEndpointIdentityProvider
    {
        EndpointIdentity GetIdentityOfSelf(SecurityTokenRequirement tokenRequirement);
    }
}
