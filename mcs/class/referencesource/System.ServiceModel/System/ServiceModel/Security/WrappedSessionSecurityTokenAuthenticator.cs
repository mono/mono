//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Policy;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;
using SysClaim = System.IdentityModel.Claims.Claim;
using SystemAuthorizationContext = System.IdentityModel.Policy.AuthorizationContext;

using System.Security.Claims;


namespace System.ServiceModel.Security
{

    /// <summary>
    /// Wraps a SessionSecurityTokenHandler. Delegates the token authentication call to
    /// this wrapped tokenAuthenticator. Wraps the returned ClaimsIdentities into
    /// an IAuthorizationPolicy. This class is wired into WCF and actually receives 
    /// SecurityContextSecurityTokens which are then wrapped into SessionSecurityTokens for
    /// validation.
    /// </summary>
    internal class WrappedSessionSecurityTokenAuthenticator : SecurityTokenAuthenticator, IIssuanceSecurityTokenAuthenticator, ICommunicationObject
    {
        SessionSecurityTokenHandler _sessionTokenHandler;
        IIssuanceSecurityTokenAuthenticator _issuanceSecurityTokenAuthenticator;
        ICommunicationObject _communicationObject;

        SctClaimsHandler _sctClaimsHandler;
        ExceptionMapper _exceptionMapper;

        /// <summary>
        /// Initializes an instance of <see cref="WrappedRsaSecurityTokenAuthenticator"/>
        /// </summary>
        /// <param name="sessionTokenHandler">The sessionTokenHandler to wrap</param>
        /// <param name="wcfSessionAuthenticator">The wcf SessionTokenAuthenticator.</param>
        /// <param name="sctClaimsHandler">Handler that converts WCF generated IAuthorizationPolicy to <see cref="AuthorizationPolicy"/></param>
        /// <param name="exceptionMapper">Converts token validation exception to SOAP faults.</param>
        public WrappedSessionSecurityTokenAuthenticator( SessionSecurityTokenHandler sessionTokenHandler,
                                                         SecurityTokenAuthenticator wcfSessionAuthenticator,
                                                         SctClaimsHandler sctClaimsHandler,
                                                         ExceptionMapper exceptionMapper )
            : base()
        {
            if ( sessionTokenHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "sessionTokenHandler" );
            }

            if ( wcfSessionAuthenticator == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "wcfSessionAuthenticator" );
            }

            if ( sctClaimsHandler == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "sctClaimsHandler" );
            }

            if ( exceptionMapper == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull( "exceptionMapper" );
            }

            _issuanceSecurityTokenAuthenticator = wcfSessionAuthenticator as IIssuanceSecurityTokenAuthenticator;
            if ( _issuanceSecurityTokenAuthenticator == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4244 ) );
            }

            _communicationObject = wcfSessionAuthenticator as ICommunicationObject;
            if ( _communicationObject == null )
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation( SR.GetString( SR.ID4245 ) );
            }

            _sessionTokenHandler = sessionTokenHandler;
            _sctClaimsHandler = sctClaimsHandler;

            _exceptionMapper = exceptionMapper;
        }

        /// <summary>
        /// Validates the token using the wrapped token handler and generates IAuthorizationPolicy
        /// wrapping the returned ClaimsIdentities.
        /// </summary>
        /// <param name="token">Token to be validated. This is always a SecurityContextSecurityToken.</param>
        /// <returns>Read-only collection of IAuthorizationPolicy</returns>
        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore( SecurityToken token )
        {
            SecurityContextSecurityToken sct = token as SecurityContextSecurityToken;
            SessionSecurityToken sessionToken = SecurityContextSecurityTokenHelper.ConvertSctToSessionToken( sct );
            IEnumerable<ClaimsIdentity> identities = null;

            try
            {
                identities = _sessionTokenHandler.ValidateToken(sessionToken, _sctClaimsHandler.EndpointId);
            }
            catch (Exception ex)
            {
                if (!_exceptionMapper.HandleSecurityTokenProcessingException(ex))
                {
                    throw;
                }
            }

            return new List<IAuthorizationPolicy>(new AuthorizationPolicy[] { new AuthorizationPolicy(identities) }).AsReadOnly();
        }

        protected override bool CanValidateTokenCore( SecurityToken token )
        {
            return ( token is SecurityContextSecurityToken );
        }

        #region IIssuanceSecurityTokenAuthenticator Members

        public IssuedSecurityTokenHandler IssuedSecurityTokenHandler
        {
            get
            {
                return _issuanceSecurityTokenAuthenticator.IssuedSecurityTokenHandler;
            }
            set
            {
                _issuanceSecurityTokenAuthenticator.IssuedSecurityTokenHandler = value;
            }
        }

        public RenewedSecurityTokenHandler RenewedSecurityTokenHandler
        {
            get
            {
                return _issuanceSecurityTokenAuthenticator.RenewedSecurityTokenHandler;
            }
            set
            {
                _issuanceSecurityTokenAuthenticator.RenewedSecurityTokenHandler = value;
            }
        }

        #endregion


        #region ICommunicationObject Members

        // all these methods are passthroughs

        public void Abort()
        {
            _communicationObject.Abort();
        }

        public System.IAsyncResult BeginClose( System.TimeSpan timeout, System.AsyncCallback callback, object state )
        {
            return _communicationObject.BeginClose( timeout, callback, state );
        }

        public System.IAsyncResult BeginClose( System.AsyncCallback callback, object state )
        {
            return _communicationObject.BeginClose( callback, state );
        }

        public System.IAsyncResult BeginOpen( System.TimeSpan timeout, System.AsyncCallback callback, object state )
        {
            return _communicationObject.BeginOpen( timeout, callback, state );
        }

        public System.IAsyncResult BeginOpen( System.AsyncCallback callback, object state )
        {
            return _communicationObject.BeginOpen( callback, state );
        }

        public void Close( System.TimeSpan timeout )
        {
            _communicationObject.Close( timeout );
        }

        public void Close()
        {
            _communicationObject.Close();
        }

        public event System.EventHandler Closed
        {
            add { _communicationObject.Closed += value; }
            remove { _communicationObject.Closed -= value; }
        }

        public event System.EventHandler Closing
        {
            add { _communicationObject.Closing += value; }
            remove { _communicationObject.Closing -= value; }
        }

        public void EndClose( System.IAsyncResult result )
        {
            _communicationObject.EndClose( result );
        }

        public void EndOpen( System.IAsyncResult result )
        {
            _communicationObject.EndOpen( result );
        }

        public event System.EventHandler Faulted
        {
            add { _communicationObject.Faulted += value; }
            remove { _communicationObject.Faulted -= value; }
        }

        public void Open( System.TimeSpan timeout )
        {
            _communicationObject.Open( timeout );
        }

        public void Open()
        {
            _communicationObject.Open();
        }

        public event System.EventHandler Opened
        {
            add { _communicationObject.Opened += value; }
            remove { _communicationObject.Opened -= value; }
        }

        public event System.EventHandler Opening
        {
            add { _communicationObject.Opening += value; }
            remove { _communicationObject.Opening -= value; }
        }

        public CommunicationState State
        {

            get { return _communicationObject.State; }
        }

        #endregion
    }

    /// <summary>
    /// Defines a SecurityStateEncoder whose Encode and Decode operations are 
    /// a no-op. This class is used to null WCF SecurityContextToken creation
    /// code to skip any encryption and decryption cost. When SessionSecurityTokenHandler
    /// is being used we will use our own EncryptionTransform and ignore the WCF 
    /// generated cookie.
    /// </summary>
    internal class NoOpSecurityStateEncoder : SecurityStateEncoder
    {
        protected internal override byte[] EncodeSecurityState( byte[] data )
        {
            return data;
        }

        protected internal override byte[] DecodeSecurityState( byte[] data )
        {
            return data;
        }
    }
}
