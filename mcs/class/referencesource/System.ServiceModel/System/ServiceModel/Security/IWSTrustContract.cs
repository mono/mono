//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Defines the IWSTrustContract for sending WS-Trust messages to an STS.
    /// </summary>
    [ServiceContract]
    public interface IWSTrustContract
    {
        /// <summary>
        /// Method for Cancel binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract(Name = "Cancel", Action = "*", ReplyAction = "*")]
        Message Cancel(Message message);

        /// <summary>
        /// Begin Async Method for Cancel binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous send request from other requests.</param>
        /// <returns>An IAsyncResult object that represents the asynchronous send, which could still be pending. </returns>
        [OperationContract(AsyncPattern = true, Name = "Cancel", Action = "*", ReplyAction = "*")]
        IAsyncResult BeginCancel(Message message, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End Async Method for Cancel binding for WS-Trust
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous send request.</param>
        /// <returns>Response message containing the RSTR.</returns>
        Message EndCancel(IAsyncResult asyncResult);

        /// <summary>
        /// Method for Issue binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract(Name = "Issue", Action = "*", ReplyAction = "*")]
        Message Issue(Message message);

        /// <summary>
        /// Begin Async Method for Issue binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous send request from other requests.</param>
        /// <returns>An IAsyncResult object that represents the asynchronous send, which could still be pending. </returns>
        [OperationContract(AsyncPattern = true, Name = "Issue", Action = "*", ReplyAction = "*")]
        IAsyncResult BeginIssue(Message message, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End Async Method for Issue binding for WS-Trust
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous send request.</param>
        /// <returns>Response message containing the RSTR.</returns>
        Message EndIssue(IAsyncResult asyncResult);

        /// <summary>
        /// Method for Renew binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract(Name = "Renew", Action = "*", ReplyAction = "*")]
        Message Renew(Message message);

        /// <summary>
        /// Begin Async Method for Renew binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous send request from other requests.</param>
        /// <returns>An IAsyncResult object that represents the asynchronous send, which could still be pending. </returns>
        [OperationContract(AsyncPattern = true, Name = "Renew", Action = "*", ReplyAction = "*")]
        IAsyncResult BeginRenew(Message message, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End Async Method for Renew binding for WS-Trust
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous send request.</param>
        /// <returns>Response message containing the RSTR.</returns>
        Message EndRenew(IAsyncResult asyncResult);

        /// <summary>
        /// Method for Validate binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract(Name = "Validate", Action = "*", ReplyAction = "*")]
        Message Validate(Message message);

        /// <summary>
        /// Begin Async Method for Validate binding for WS-Trust
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the send is complete.</param>
        /// <param name="asyncState">A user-provided object that distinguishes this particular asynchronous send request from other requests.</param>
        /// <returns>An IAsyncResult object that represents the asynchronous send, which could still be pending. </returns>
        [OperationContract(AsyncPattern = true, Name = "Validate", Action = "*", ReplyAction = "*")]
        IAsyncResult BeginValidate(Message message, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End Async Method for Validate binding for WS-Trust
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous send request.</param>
        /// <returns>Response message containing the RSTR.</returns>
        Message EndValidate(IAsyncResult asyncResult);

    }
}
