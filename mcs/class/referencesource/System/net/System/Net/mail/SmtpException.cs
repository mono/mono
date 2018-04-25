//-----------------------------------------------------------------------------
// <copyright file="SmtpException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class SmtpException : Exception, ISerializable
    {
        SmtpStatusCode statusCode = SmtpStatusCode.GeneralFailure;
        
        static string GetMessageForStatus(SmtpStatusCode statusCode, string serverResponse)
        {
            return GetMessageForStatus(statusCode)+" "+SR.GetString(SR.MailServerResponse,serverResponse);
        }
        
        static string GetMessageForStatus(SmtpStatusCode statusCode)
        {
            switch (statusCode)
            {
                default :
                case SmtpStatusCode.CommandUnrecognized:
                    return SR.GetString(SR.SmtpCommandUnrecognized);
                case SmtpStatusCode.SyntaxError:
                    return SR.GetString(SR.SmtpSyntaxError);
                case SmtpStatusCode.CommandNotImplemented:
                    return SR.GetString(SR.SmtpCommandNotImplemented);
                case SmtpStatusCode.BadCommandSequence:
                    return SR.GetString(SR.SmtpBadCommandSequence);
                case SmtpStatusCode.CommandParameterNotImplemented:
                    return SR.GetString(SR.SmtpCommandParameterNotImplemented);
                case SmtpStatusCode.SystemStatus:
                    return SR.GetString(SR.SmtpSystemStatus);
                case SmtpStatusCode.HelpMessage:
                    return SR.GetString(SR.SmtpHelpMessage);
                case SmtpStatusCode.ServiceReady:
                    return SR.GetString(SR.SmtpServiceReady);
                case SmtpStatusCode.ServiceClosingTransmissionChannel:
                    return SR.GetString(SR.SmtpServiceClosingTransmissionChannel);
                case SmtpStatusCode.ServiceNotAvailable:
                    return SR.GetString(SR.SmtpServiceNotAvailable);
                case SmtpStatusCode.Ok:
                    return SR.GetString(SR.SmtpOK);
                case SmtpStatusCode.UserNotLocalWillForward:
                    return SR.GetString(SR.SmtpUserNotLocalWillForward);
                case SmtpStatusCode.MailboxBusy:
                    return SR.GetString(SR.SmtpMailboxBusy);
                case SmtpStatusCode.MailboxUnavailable:
                    return SR.GetString(SR.SmtpMailboxUnavailable);
                case SmtpStatusCode.LocalErrorInProcessing:
                    return SR.GetString(SR.SmtpLocalErrorInProcessing);
                case SmtpStatusCode.UserNotLocalTryAlternatePath:
                    return SR.GetString(SR.SmtpUserNotLocalTryAlternatePath);
                case SmtpStatusCode.InsufficientStorage:
                    return SR.GetString(SR.SmtpInsufficientStorage);
                case SmtpStatusCode.ExceededStorageAllocation:
                    return SR.GetString(SR.SmtpExceededStorageAllocation);
                case SmtpStatusCode.MailboxNameNotAllowed:
                    return SR.GetString(SR.SmtpMailboxNameNotAllowed);
                case SmtpStatusCode.StartMailInput:
                    return SR.GetString(SR.SmtpStartMailInput);
                case SmtpStatusCode.TransactionFailed:
                    return SR.GetString(SR.SmtpTransactionFailed);
                case SmtpStatusCode.ClientNotPermitted:
                    return SR.GetString(SR.SmtpClientNotPermitted);
                case SmtpStatusCode.MustIssueStartTlsFirst:
                    return SR.GetString(SR.SmtpMustIssueStartTlsFirst);

            }
        }

        public SmtpException(SmtpStatusCode statusCode) : base(GetMessageForStatus(statusCode))
        {
            this.statusCode = statusCode;
        }

        public SmtpException(SmtpStatusCode statusCode, string message) : base(message)
        {
            this.statusCode = statusCode;
        }

        public SmtpException() : this(SmtpStatusCode.GeneralFailure)
        {
        }

        public SmtpException(string message) : base(message)
        {
        }
        
        public SmtpException(string message, Exception innerException) : base(message, innerException) 
        {
        }

        protected SmtpException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base (serializationInfo, streamingContext) 
        {
            statusCode = (SmtpStatusCode)serializationInfo.GetInt32("Status");
        }


        internal SmtpException(SmtpStatusCode statusCode, string serverMessage, bool serverResponse) : base(GetMessageForStatus(statusCode,serverMessage))
        {
            this.statusCode = statusCode;
        }

        internal SmtpException(string message, string serverResponse) : base(message+" "+SR.GetString(SR.MailServerResponse,serverResponse))
        {
        }


        /// <internalonly/>

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext) {
            GetObjectData(serializationInfo, streamingContext);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)] 		
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext){
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("Status", (int)statusCode, typeof(int));
        }

        public SmtpStatusCode StatusCode
        {
            get {
                return this.statusCode;
            }
            set {
                statusCode = value;
            }
        }
    }

}
