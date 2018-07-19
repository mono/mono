//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security.Tokens;

    interface IInitiatorSecuritySessionProtocol
    {
        bool ReturnCorrelationState { get; set; }
        SecurityToken GetOutgoingSessionToken();
        void SetIdentityCheckAuthenticator(SecurityTokenAuthenticator tokenAuthenticator);
        void SetOutgoingSessionToken(SecurityToken token);
        List<SecurityToken> GetIncomingSessionTokens();
        void SetIncomingSessionTokens(List<SecurityToken> tokens);
    }
}
