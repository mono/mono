//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Protocols.WSTrust;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Defines the ServiceContract Asynchronous interface for a Trust Feb 2005 protocol.
    /// </summary>
    [ServiceContract( Name = WSTrustServiceContractConstants.Contracts.IWSTrustFeb2005Async, Namespace = WSTrustServiceContractConstants.Namespace )]
    public interface IWSTrustFeb2005AsyncContract
    {
        /// <summary>
        /// Definiton of Async RST/Cancel method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005CancelAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.Cancel, ReplyAction = WSTrustFeb2005Constants.Actions.CancelResponse )]
        IAsyncResult BeginTrustFeb2005Cancel( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Cancel method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginCancel call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005Cancel( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RST/Issue method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005IssueAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.Issue, ReplyAction = WSTrustFeb2005Constants.Actions.IssueResponse )]
        IAsyncResult BeginTrustFeb2005Issue( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Issue method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginIssue call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005Issue( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RST/Renew method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005RenewAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.Renew, ReplyAction = WSTrustFeb2005Constants.Actions.RenewResponse )]
        IAsyncResult BeginTrustFeb2005Renew( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Renew method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginRenew call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005Renew( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RST/Validate method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005ValidateAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.Validate, ReplyAction = WSTrustFeb2005Constants.Actions.ValidateResponse )]
        IAsyncResult BeginTrustFeb2005Validate( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Validate method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginValidate call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005Validate( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RSTR/Cancel method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005CancelResponseAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.CancelResponse, ReplyAction = WSTrustFeb2005Constants.Actions.CancelResponse )]
        IAsyncResult BeginTrustFeb2005CancelResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Cancel method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginCancelResponse call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005CancelResponse( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RSTR/Issue method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005IssueResponseAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.IssueResponse, ReplyAction = WSTrustFeb2005Constants.Actions.IssueResponse )]
        IAsyncResult BeginTrustFeb2005IssueResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Issue method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginIssueResponse call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005IssueResponse( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RSTR/Renew method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005RenewResponseAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.RenewResponse, ReplyAction = WSTrustFeb2005Constants.Actions.RenewResponse )]
        IAsyncResult BeginTrustFeb2005RenewResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Renew method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginRenewResponse call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005RenewResponse( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RSTR/Validate method for WS-Trust Feb 2005
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005ValidateResponseAsync, AsyncPattern = true, Action = WSTrustFeb2005Constants.Actions.ValidateResponse, ReplyAction = WSTrustFeb2005Constants.Actions.ValidateResponse )]
        IAsyncResult BeginTrustFeb2005ValidateResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Validate method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginValidateResponse call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrustFeb2005ValidateResponse( IAsyncResult ar );
    }
}
