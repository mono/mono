//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel;


namespace System.Security.Claims
{
    /// <summary>
    /// This class is used to specify the context of the authorization event.
    /// </summary>
    public class AuthorizationContext
    {   
        Collection<System.Security.Claims.Claim> _action = new Collection<System.Security.Claims.Claim>();
        Collection<System.Security.Claims.Claim> _resource = new Collection<System.Security.Claims.Claim>();
        ClaimsPrincipal _principal;

        /// <summary>
        /// Creates an AuthorizationContext with the specified principal, resource, and action.
        /// </summary>
        /// <param name="principal">The principal to be authorized.</param>
        /// <param name="resource">The resource to be authorized for.</param>
        /// <param name="action">The action to be performed on the resource.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="principal"/> or <paramref name="resource"/> is set to null.
        /// </exception>
        public AuthorizationContext( ClaimsPrincipal principal, string resource, string action )
        {
            if ( principal == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "principal" );
            }

            if ( string.IsNullOrEmpty( resource ) )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "resource" );
            }            

            _principal = principal;
            _resource.Add( new System.Security.Claims.Claim( ClaimTypes.Name, resource ) );
            if ( action != null )
            {
                _action.Add( new System.Security.Claims.Claim( ClaimTypes.Name, action ) );
            }
        }

        /// <summary>
        /// Creates an AuthorizationContext with the specified principal, resource, and action.
        /// </summary>
        /// <param name="principal">The principal to check authorization for</param>
        /// <param name="resource">The resource for checking authorization to</param>
        /// <param name="action">The action to be performed on the resource</param>
        /// <exception cref="ArgumentNullException">When <paramref name="principal"/> or <paramref name="resource"/> or <paramref name="action"/> is null</exception>
        public AuthorizationContext( ClaimsPrincipal principal, Collection<System.Security.Claims.Claim> resource, Collection<System.Security.Claims.Claim> action )
        {
            if ( principal == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "principal" );
            }

            if ( resource == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "resource" );
            }

            if ( action == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "action" );
            }

            _principal = principal;
            _resource = resource;
            _action = action;
        }

        /// <summary>
        /// Gets the authorization action
        /// </summary>
        public Collection<System.Security.Claims.Claim> Action
        {
            get { return _action; }
        }

        /// <summary>
        /// Gets the authorization resource
        /// </summary>
        public Collection<System.Security.Claims.Claim> Resource
        {
            get { return _resource; }
        }

        /// <summary>
        /// Gets the authorization principal
        /// </summary>
        public ClaimsPrincipal Principal
        {
            get { return _principal; }
        }
    }
}
