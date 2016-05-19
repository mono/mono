//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Protocols.WSTrust;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Defines the ServiceContract interface for a Trust 1.3 protocol.
    /// </summary>
    [ServiceContract( Name = WSTrustServiceContractConstants.Contracts.IWSTrust13Sync, Namespace = WSTrustServiceContractConstants.Namespace )]
    public interface IWSTrust13SyncContract
    {
        /// <summary>
        /// Definiton of a RST/Cancel method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13Cancel, Action = WSTrust13Constants.Actions.Cancel, ReplyAction = WSTrust13Constants.Actions.CancelFinalResponse )]
        Message ProcessTrust13Cancel( Message message );

        /// <summary>
        /// Definiton of a RST/Issue method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13Issue, Action = WSTrust13Constants.Actions.Issue, ReplyAction = WSTrust13Constants.Actions.IssueFinalResponse )]
        Message ProcessTrust13Issue( Message message );

        /// <summary>
        /// Definiton of a RST/Renew method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13Renew, Action = WSTrust13Constants.Actions.Renew, ReplyAction = WSTrust13Constants.Actions.RenewFinalResponse )]
        Message ProcessTrust13Renew( Message message );

        /// <summary>
        /// Definiton of a RST/Validate method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13Validate, Action = WSTrust13Constants.Actions.Validate, ReplyAction = WSTrust13Constants.Actions.ValidateFinalResponse )]
        Message ProcessTrust13Validate( Message message );

        /// <summary>
        /// Definiton of a RSTR/Cancel method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13CancelResponse, Action = WSTrust13Constants.Actions.CancelResponse, ReplyAction = "*" )]
        Message ProcessTrust13CancelResponse( Message message );

        /// <summary>
        /// Definiton of a RSTR/Issue method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13IssueResponse, Action = WSTrust13Constants.Actions.IssueResponse, ReplyAction = "*" )]
        Message ProcessTrust13IssueResponse( Message message );

        /// <summary>
        /// Definiton of a RSTR/Renew method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13RenewResponse, Action = WSTrust13Constants.Actions.RenewResponse, ReplyAction = "*" )]
        Message ProcessTrust13RenewResponse( Message message );

        /// <summary>
        /// Definiton of a RSTR/Validate method for WS-Trust 1.3
        /// </summary>
        /// <param name="message">The Request Message that contains a RST.</param>
        /// <returns>Response message containing the RSTR.</returns>
        //
        // NOTE:
        //      ReplyAction = "*" has a side effect of not generating this operation, port, or messages in the 
        //      WCF-generated WSDL. This is desired.
        //
        [OperationContract( Name = WSTrustServiceContractConstants.Operations.Trust13ValidateResponse, Action = WSTrust13Constants.Actions.ValidateResponse, ReplyAction = "*" )]
        Message ProcessTrust13ValidateResponse( Message message );
    }
}
