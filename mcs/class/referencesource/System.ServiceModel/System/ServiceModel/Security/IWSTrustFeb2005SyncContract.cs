//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Protocols.WSTrust;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Defines the ServiceContract interface for a Trust Feb 2005 protocol
    /// </summary>
    [ServiceContract( Name = WSTrustServiceContractConstants.Contracts.IWSTrustFeb2005Sync, Namespace = WSTrustServiceContractConstants.Namespace )]
    public interface IWSTrustFeb2005SyncContract
    {
        /// <summary>
        /// Definiton of a RST/Cancel method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005Cancel, Action = WSTrustFeb2005Constants.Actions.Cancel, ReplyAction = WSTrustFeb2005Constants.Actions.CancelResponse )]
        Message ProcessTrustFeb2005Cancel( Message message );

        /// <summary>
        /// Definiton of a RST/Issue method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005Issue, Action = WSTrustFeb2005Constants.Actions.Issue, ReplyAction = WSTrustFeb2005Constants.Actions.IssueResponse )]
        Message ProcessTrustFeb2005Issue( Message message );

        /// <summary>
        /// Definiton of a RST/Renew method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005Renew, Action = WSTrustFeb2005Constants.Actions.Renew, ReplyAction = WSTrustFeb2005Constants.Actions.RenewResponse )]
        Message ProcessTrustFeb2005Renew( Message message );

        /// <summary>
        /// Definiton of a RST/Validate method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005Validate, Action = WSTrustFeb2005Constants.Actions.Validate, ReplyAction = WSTrustFeb2005Constants.Actions.ValidateResponse )]
        Message ProcessTrustFeb2005Validate( Message message );

        /// <summary>
        /// Definiton of a RSTR/Cancel method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005CancelResponse, Action = WSTrustFeb2005Constants.Actions.CancelResponse, ReplyAction = WSTrustFeb2005Constants.Actions.CancelResponse )]
        Message ProcessTrustFeb2005CancelResponse( Message message );

        /// <summary>
        /// Definiton of a RSTR/Issue method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005IssueResponse, Action = WSTrustFeb2005Constants.Actions.IssueResponse, ReplyAction = WSTrustFeb2005Constants.Actions.IssueResponse )]
        Message ProcessTrustFeb2005IssueResponse( Message message );

        /// <summary>
        /// Definiton of a RSTR/Renew method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005RenewResponse, Action = WSTrustFeb2005Constants.Actions.RenewResponse, ReplyAction = WSTrustFeb2005Constants.Actions.RenewResponse )]
        Message ProcessTrustFeb2005RenewResponse( Message message );

        /// <summary>
        /// Definiton of a RSTR/Validate method for WS-Trut Feb 2005
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.TrustFeb2005ValidateResponse, Action = WSTrustFeb2005Constants.Actions.ValidateResponse, ReplyAction = WSTrustFeb2005Constants.Actions.ValidateResponse )]
        Message ProcessTrustFeb2005ValidateResponse( Message message );
    }
}
