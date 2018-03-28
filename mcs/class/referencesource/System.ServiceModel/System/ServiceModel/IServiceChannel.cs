//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net.Security;
    using System.ServiceModel.Security.Tokens;

    public interface IServiceChannel : IContextChannel
    {
        Uri ListenUri { get; }
    }
}
