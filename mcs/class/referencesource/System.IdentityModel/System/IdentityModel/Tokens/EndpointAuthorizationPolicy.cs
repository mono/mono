//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
namespace System.IdentityModel.Tokens
{
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;

    /// <summary>
    /// Implementation of IAuthorizationPolicy that contains endpoint specific 
    /// AuthorizationPolicy.
    /// </summary>
    internal class EndpointAuthorizationPolicy : IAuthorizationPolicy
    {
        string _endpointId;
        string _id = UniqueId.CreateUniqueId();

        /// <summary>
        /// Creates an instance of <see cref="EndpointAuthorizationPolicy"/>
        /// </summary>
        /// <param name="endpointId">Identifier of the Endpoint to which the token should be restricted.</param>
        public EndpointAuthorizationPolicy( string endpointId )
        {
            if ( endpointId == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "endpointId" );
            }

            _endpointId = endpointId;
        }

        /// <summary>
        /// Gets the EndpointId for the AuthorizationPolicy
        /// </summary>
        public string EndpointId
        {
            get { return _endpointId; }
        }

        #region IAuthorizationPolicy Members

        /// <summary>
        /// Check if the claims in the EvaluationContext.
        /// </summary>
        /// <param name="evaluationContext">The current evaluationContext</param>
        /// <param name="state">Any custom state.</param>
        /// <returns>Returns true by default.</returns>
        bool IAuthorizationPolicy.Evaluate( EvaluationContext evaluationContext, ref object state )
        {
            return true;
        }

        /// <summary>
        /// Gets the Issuer ClaimSet. Returns null by default.
        /// </summary>
        ClaimSet IAuthorizationPolicy.Issuer
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IAuthorizationComponent Members

        /// <summary>
        /// Gets the Id.
        /// </summary>
        string IAuthorizationComponent.Id
        {
            get
            {
                return _id;
            }
        }
        #endregion
    }

}
