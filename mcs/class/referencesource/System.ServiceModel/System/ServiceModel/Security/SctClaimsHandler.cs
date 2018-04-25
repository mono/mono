//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;

using SysClaim = System.IdentityModel.Claims.Claim;
using SystemAuthorizationContext = System.IdentityModel.Policy.AuthorizationContext;

namespace System.ServiceModel.Security
{
    internal class SctClaimsHandler
    {
        SecurityTokenHandlerCollection _securityTokenHandlerCollection;
        string _endpointId;

        /// <summary>
        /// Creates an instance of <see cref="SctClaimsHandler"/>
        /// </summary>
        public SctClaimsHandler(
            SecurityTokenHandlerCollection securityTokenHandlerCollection,
            string endpointId)
        {
            if ( securityTokenHandlerCollection == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "securityTokenHandlerCollection" );
            }

            if ( endpointId == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNullOrEmptyString( "endpointId" );
            }

            _securityTokenHandlerCollection = securityTokenHandlerCollection;
            _endpointId = endpointId;
        }

        /// <summary>
        /// Gets the Endpoint Id to which all <see cref="SecurityContextSecurityToken"/> should be scoped.
        /// </summary>
        public string EndpointId
        {
            get { return _endpointId; }
        }

        /// <summary>
        /// Gets the <see cref="SecurityTokenHandlerCollection" /> used to validate the SCT.
        /// </summary>
        public SecurityTokenHandlerCollection SecurityTokenHandlerCollection
        {
            get { return _securityTokenHandlerCollection; }
        }

        /// <summary>
        /// The the purposes of this method are:
        /// 1. To enable layers above to get to the bootstrap tokens
        /// 2. To ensure an ClaimsPrincipal is inside the SCT authorization policies.  This is needed so that
        ///    a CustomPrincipal will be created and can be set.  This is required as we set the principal permission mode to custom 
        /// 3. To set the IAuthorizationPolicy collection on the SCT to be one of IDFx's Authpolicy.        
        /// This allows SCT cookie and SCT cached to be treated the same, futher up the stack.  
        /// 
        /// This method is call AFTER the final SCT has been created and the bootstrap tokens are around.  Itis not called during the SP/TLS nego bootstrap.
        /// </summary>
        /// <param name="sct"></param>
        internal void SetPrincipalBootstrapTokensAndBindIdfxAuthPolicy( SecurityContextSecurityToken sct )
        {
            if ( sct == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "sct" );
            }

            List<IAuthorizationPolicy> iaps = new List<IAuthorizationPolicy>();

            //
            // The SecurityContextToken is cached first before the OnTokenIssued is called. So in the Session SCT
            // case the AuthorizationPolicies will have already been updated. So check the sct.AuthorizationPolicies
            // policy to see if the first is a AuthorizationPolicy. 
            //
            if ( ( sct.AuthorizationPolicies != null ) &&
                ( sct.AuthorizationPolicies.Count > 0 ) &&
                ( ContainsEndpointAuthPolicy(sct.AuthorizationPolicies) ) )
            {
                // We have already seen this sct and have fixed up the AuthorizationPolicy
                // collection. Just return.
                return;
            }

            //
            // Nego SCT just has a cookie, there are no IAuthorizationPolicy. In this case,
            // we want to add the EndpointAuthorizationPolicy alone to the SCT.
            //
            if ( ( sct.AuthorizationPolicies != null ) &&
                ( sct.AuthorizationPolicies.Count > 0 ) )
            {
                //
                // Create a principal with known policies.
                //
                AuthorizationPolicy sctAp = IdentityModelServiceAuthorizationManager.TransformAuthorizationPolicies( sct.AuthorizationPolicies,
                                                                                                                     _securityTokenHandlerCollection,
                                                                                                                     false );
                // Replace the WCF authorization policies with our IDFx policies.  
                // The principal is needed later on to set the custom principal by WCF runtime.
                iaps.Add( sctAp );

                //
                // Convert the claim from WCF unconditional policy to an SctAuthorizationPolicy. The SctAuthorizationPolicy simply
                // captures the primary identity claim from the WCF unconditional policy which IdFX will eventually throw away. 
                // If we don't capture that claim, then in a token renewal scenario WCF will fail due to identities being different 
                // for the issuedToken and the renewedToken.
                //
                SysClaim claim = GetPrimaryIdentityClaim( SystemAuthorizationContext.CreateDefaultAuthorizationContext( sct.AuthorizationPolicies ) );

                SctAuthorizationPolicy sctAuthPolicy = new SctAuthorizationPolicy( claim );
                iaps.Add( sctAuthPolicy );
            }

            iaps.Add( new EndpointAuthorizationPolicy( _endpointId ) );
            sct.AuthorizationPolicies = iaps.AsReadOnly();
        }

        bool ContainsEndpointAuthPolicy( ReadOnlyCollection<IAuthorizationPolicy> policies )
        {
            if ( policies == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "policies" );
            }

            for ( int i = 0; i < policies.Count; ++i )
            {
                if ( policies[i] is EndpointAuthorizationPolicy )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the primary identity claim to create the SCTAuthorizationPolicy
        /// </summary>
        /// <param name="authContext">The authorization context</param>
        /// <returns>The primary identity claim from the authorization context.</returns>
        SysClaim GetPrimaryIdentityClaim( SystemAuthorizationContext authContext )
        {
            if ( authContext != null )
            {
                for ( int i = 0; i < authContext.ClaimSets.Count; ++i )
                {
                    System.IdentityModel.Claims.ClaimSet claimSet = authContext.ClaimSets[i];
                    foreach ( System.IdentityModel.Claims.Claim claim in claimSet.FindClaims( null, System.IdentityModel.Claims.Rights.Identity ) )
                    {
                        return claim;
                    }
                }
            }
            return null;
        }

        public void OnTokenIssued( SecurityToken issuedToken, EndpointAddress tokenRequestor )
        {
            SetPrincipalBootstrapTokensAndBindIdfxAuthPolicy( issuedToken as SecurityContextSecurityToken );
        }

        public void OnTokenRenewed( SecurityToken issuedToken, SecurityToken oldToken )
        {
            SetPrincipalBootstrapTokensAndBindIdfxAuthPolicy( issuedToken as SecurityContextSecurityToken );
        }
    }
}
