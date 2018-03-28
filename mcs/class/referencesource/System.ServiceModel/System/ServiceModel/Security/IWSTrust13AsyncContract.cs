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
    /// Defines the ServiceContract Asynchronous interface for a Trust 1.3 protocol.
    /// </summary>
    [ServiceContract( Name = WSTrustServiceContractConstants.Contracts.IWSTrust13Async, Namespace = WSTrustServiceContractConstants.Namespace )]
    public interface IWSTrust13AsyncContract
    {
        /// <summary>
        /// Definiton of Async RST/Cancel method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13CancelAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.Cancel, ReplyAction = WSTrust13Constants.Actions.CancelFinalResponse )]
        IAsyncResult BeginTrust13Cancel( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Cancel method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginCancel call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13Cancel( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RST/Issue method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13IssueAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.Issue, ReplyAction = WSTrust13Constants.Actions.IssueFinalResponse )]
        IAsyncResult BeginTrust13Issue( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Issue method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginIssue call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13Issue( IAsyncResult ar );


        /// <summary>
        /// Definiton of Async RST/Renew method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13RenewAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.Renew, ReplyAction = WSTrust13Constants.Actions.RenewFinalResponse )]
        IAsyncResult BeginTrust13Renew( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Renew method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginRenew call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13Renew( IAsyncResult ar );


        /// <summary>
        /// Definiton of Async RST/Validate method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13ValidateAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.Validate, ReplyAction = WSTrust13Constants.Actions.ValidateFinalResponse )]
        IAsyncResult BeginTrust13Validate( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RST/Validate method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginValidate call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13Validate( IAsyncResult ar );

        /// <summary>
        /// Definiton of Async RSTR/Cancel method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13CancelResponseAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.CancelResponse, ReplyAction = "*" )]
        IAsyncResult BeginTrust13CancelResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Cancel method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginCancel call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13CancelResponse( IAsyncResult ar );


        /// <summary>
        /// Definiton of Async RSTR/Issue method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13IssueResponseAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.IssueResponse, ReplyAction = "*" )]
        IAsyncResult BeginTrust13IssueResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Issue method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginIssue call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13IssueResponse( IAsyncResult ar );


        /// <summary>
        /// Definiton of Async RSTR/Renew method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13RenewResponseAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.RenewResponse, ReplyAction = "*" )]
        IAsyncResult BeginTrust13RenewResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Renew method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginRenew call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13RenewResponse( IAsyncResult ar );


        /// <summary>
        /// Definiton of Async RSTR/Validate method for WS-Trust 1.3
        /// </summary>
        /// <param name="request">Request Message containing the RST.</param>
        /// <param name="callback">AsyncCallback context.</param>
        /// <param name="state">Asyn state.</param>
        /// <returns>IAsyncResult result instance.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13ValidateResponseAsync, AsyncPattern = true, Action = WSTrust13Constants.Actions.ValidateResponse, ReplyAction = "*" )]
        IAsyncResult BeginTrust13ValidateResponse( Message request, AsyncCallback callback, object state );

        /// <summary>
        /// Completes the Async RSTR/Validate method.
        /// </summary>
        /// <param name="ar">The IAsyncResult result instance returned by the BeginValidate call.</param>
        /// <returns>Message object that contains the RSTR.</returns>
        Message EndTrust13ValidateResponse( IAsyncResult ar );
    }
}
