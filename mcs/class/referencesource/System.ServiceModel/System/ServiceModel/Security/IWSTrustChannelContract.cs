//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.IdentityModel.Protocols.WSTrust;

    /// <summary>
    /// A service contract that defines the methods which wrap the Message-oriented 
    /// operation contracts exposed by <see cref="IWSTrustContract" />.
    /// </summary>
    [ServiceContract]
    [ComVisible(false)]
    public interface IWSTrustChannelContract : IWSTrustContract
    {
        /// <summary>
        /// Sends a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <returns>The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</returns>
        System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse Cancel(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request);

        /// <summary>
        /// Asynchronously sends a WS-Trust Cancel message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        IAsyncResult BeginCancel(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, AsyncCallback callback, object state);

        /// <summary>
        /// Completes the asynchronous send operation initiated by <see cref="BeginCancel" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="response">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        void EndCancel(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response);

        /// <summary>
        /// Sends a WS-Trust Issue message to an endpoint STS
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>
        /// <returns>A <see cref="SecurityToken" /> that represents the token issued by the STS.</returns>
        SecurityToken Issue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request);

        /// <summary>
        /// Sends a WS-Trust Issue message to an endpoint STS
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>
        /// <param name="response">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> that represents the response from 
        /// the STS.</param>
        /// <returns>A <see cref="SecurityToken" /> that represents the token issued by the STS.</returns>
        SecurityToken Issue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response);

        /// <summary>
        /// Asynchronously sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        IAsyncResult BeginIssue(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, AsyncCallback callback, object asyncState);

        /// <summary>
        /// Completes the asynchronous send operation initiated by <see cref="BeginIssue" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="response">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        /// <returns>A <see cref="SecurityToken" /> that represents the token issued by the STS.</returns>
        SecurityToken EndIssue(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response);

        /// <summary>
        /// Sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <returns>The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</returns>
        System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse Renew(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request);

        /// <summary>
        /// Asynchronously sends a WS-Trust Renew message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        IAsyncResult BeginRenew(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, AsyncCallback callback, object state);

        /// <summary>
        /// Completes the asynchronous send operation initiated by <see cref="BeginRenew" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="response">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        void EndRenew(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response);

        /// <summary>
        /// Sends a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <returns>The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</returns>
        System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse Validate(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request);

        /// <summary>
        /// Asynchronously sends a WS-Trust Validate message to an endpoint.
        /// </summary>
        /// <param name="request">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityToken" /> that represents the request to the STS.</param>        
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous send 
        /// request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> object that represents the asynchronous send, which could still 
        /// be pending. </returns>
        IAsyncResult BeginValidate(System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, AsyncCallback callback, object state);

        /// <summary>
        /// Completes the asynchronous send operation initiated by <see cref="BeginValidate" />.
        /// </summary>
        /// <param name="result">A reference to the outstanding asynchronous send request.</param>
        /// <param name="response">The <see cref="System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse" /> representing the STS response.</param>
        void EndValidate(IAsyncResult result, out System.IdentityModel.Protocols.WSTrust.RequestSecurityTokenResponse response);
    }
}
