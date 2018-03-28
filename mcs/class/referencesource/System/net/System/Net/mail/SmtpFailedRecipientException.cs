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
    public class SmtpFailedRecipientException : SmtpException, ISerializable
    {
        private string failedRecipient;
        internal bool fatal;


        public SmtpFailedRecipientException() : base() { }

        public SmtpFailedRecipientException(string message) : base(message) { }

        public SmtpFailedRecipientException(string message, Exception innerException) : base(message, innerException) { }

        protected SmtpFailedRecipientException(SerializationInfo info, StreamingContext context) : base (info, context)
        {
            failedRecipient = info.GetString("failedRecipient");
        }


        public SmtpFailedRecipientException(SmtpStatusCode statusCode, string failedRecipient) : base(statusCode)
        {
            this.failedRecipient = failedRecipient;
        }

        public SmtpFailedRecipientException(SmtpStatusCode statusCode, string failedRecipient, string serverResponse) : base(statusCode, serverResponse, true)
        {
            this.failedRecipient = failedRecipient;
        }

        public SmtpFailedRecipientException(string message, string failedRecipient, Exception innerException) : base(message, innerException)
        {
            this.failedRecipient = failedRecipient;
        }

        public string FailedRecipient
        {
            get
            {
                return failedRecipient;
            }
        }


        //
        // ISerializable
        //

        /// <internalonly/>

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)] 		
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            GetObjectData(serializationInfo, streamingContext);
        }

        //
        // FxCop: provide some way for derived classes to access GetObjectData even if the derived class
        // explicitly re-inherits ISerializable.
        //
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)] 		
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("failedRecipient", failedRecipient, typeof(string));
        }
    }
}
