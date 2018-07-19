//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Protocols.WSTrust;
    using System.Security.Claims;
    using RSTR = System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse;
    using System.IdentityModel;

    /// <summary>
    /// Defines the inputs and outputs to the <see cref="WSTrustServiceContract.DispatchRequest"/> method.
    /// </summary>
    public class DispatchContext
    {
        ClaimsPrincipal principal;
        string requestAction;
        WSTrustMessage requestMessage;
        string responseAction;
        RSTR responseMessage;
        SecurityTokenService securityTokenService;
        string trustNamespace;

        /// <summary>
        /// The identity of the requestor.
        /// </summary>
        public ClaimsPrincipal Principal
        {
            get { return principal; }
            set { principal = value; }
        }

        /// <summary>
        /// The WS-Addressing action of the request message.
        /// </summary>
        public string RequestAction
        {
            get { return requestAction; }
            set { requestAction = value; }
        }

        /// <summary>
        /// The request message.
        /// </summary>
        public WSTrustMessage RequestMessage
        {
            get { return requestMessage; }
            set { requestMessage = value; }
        }

        /// <summary>
        /// The desired WS-Addressing action of the response message.
        /// </summary>
        public string ResponseAction
        {
            get { return responseAction; }
            set { responseAction = value; }
        }

        /// <summary>
        /// The response message.
        /// </summary>
        public RSTR ResponseMessage
        {
            get { return responseMessage; }
            set { responseMessage = value; }
        }

        /// <summary>
        /// The <see cref="SecurityTokenService"/> object which should process <see cref="RequestMessage"/>.
        /// </summary>
        public SecurityTokenService SecurityTokenService
        {
            get { return securityTokenService; }
            set { securityTokenService = value; }
        }

        /// <summary>
        /// The WS-Trust namespace uri defining the schema for the request and response messages.
        /// </summary>
        public string TrustNamespace
        {
            get { return trustNamespace; }
            set { trustNamespace = value; }
        }
    }    
}
