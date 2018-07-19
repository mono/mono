//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
namespace System.IdentityModel.Tokens
{
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using SysClaim = System.IdentityModel.Claims.Claim;

    /// <summary>
    /// This class serves as a dummy AuthorizationPolicy on an issued token so that when
    /// WCF renews a token it can match the identity of the issuer with the renewer. This is 
    /// required as in the IDFX layer we throw the WCF generated AuthorizationPolicy ( UnconditionalPolicy )
    /// </summary>
    internal class SctAuthorizationPolicy : IAuthorizationPolicy
    {
        ClaimSet _issuer;
        string _id = UniqueId.CreateUniqueId();

        internal SctAuthorizationPolicy( SysClaim claim )
        {
            _issuer = new DefaultClaimSet( claim );
        }

        #region IAuthorizationPolicy Members

        bool IAuthorizationPolicy.Evaluate( EvaluationContext evaluationContext, ref object state )
        {
            if ( evaluationContext == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "evaluationContext" );
            }
            evaluationContext.AddClaimSet( this, _issuer );
            return true;
        }

        ClaimSet IAuthorizationPolicy.Issuer
        {
            get
            {
                return _issuer;
            }
        }

        #endregion

        #region IAuthorizationComponent Members

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
