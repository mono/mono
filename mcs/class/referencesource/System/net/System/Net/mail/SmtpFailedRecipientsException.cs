using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Net.Mail
{
    /// <summary>
    /// Summary description for SmtpFailedRecipientsException.
    /// </summary>
    [Serializable]
    public class SmtpFailedRecipientsException : SmtpFailedRecipientException, ISerializable
    {
        SmtpFailedRecipientException[] innerExceptions;


        // FxCop
        public SmtpFailedRecipientsException()
        {
            innerExceptions = new SmtpFailedRecipientException[0];
        }

        public SmtpFailedRecipientsException(string message) : base(message)
        {
            innerExceptions = new SmtpFailedRecipientException[0];
        }

        public SmtpFailedRecipientsException(string message, Exception innerException) : base(message, innerException)
        {
            SmtpFailedRecipientException smtpException = innerException as SmtpFailedRecipientException;
            this.innerExceptions = smtpException == null ? new SmtpFailedRecipientException[0] : new SmtpFailedRecipientException[] { smtpException };
        }

        protected SmtpFailedRecipientsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            innerExceptions = (SmtpFailedRecipientException[]) info.GetValue("innerExceptions", typeof(SmtpFailedRecipientException[]));
        }


        public SmtpFailedRecipientsException(string message, SmtpFailedRecipientException[] innerExceptions) :
            base(message, innerExceptions != null && innerExceptions.Length > 0 ? innerExceptions[0].FailedRecipient : null,
            innerExceptions != null && innerExceptions.Length > 0 ? innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }

            this.innerExceptions = innerExceptions == null ? new SmtpFailedRecipientException[0] : innerExceptions;
        }

        internal SmtpFailedRecipientsException(ArrayList innerExceptions, bool allFailed) :
            base(allFailed ? SR.GetString(SR.SmtpAllRecipientsFailed) : SR.GetString(SR.SmtpRecipientFailed),
            innerExceptions != null && innerExceptions.Count > 0 ? ((SmtpFailedRecipientException) innerExceptions[0]).FailedRecipient : null,
            innerExceptions != null && innerExceptions.Count > 0 ? (SmtpFailedRecipientException) innerExceptions[0] : null)
        {
            if (innerExceptions == null)
            {
                throw new ArgumentNullException("innerExceptions");
            }

            this.innerExceptions = new SmtpFailedRecipientException[innerExceptions.Count];
            int i = 0;
            foreach(SmtpFailedRecipientException e in innerExceptions) {
                this.innerExceptions[i++]=e;
            }
        }

        public SmtpFailedRecipientException[] InnerExceptions
        { 
            get
            {
                return innerExceptions;
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
            serializationInfo.AddValue("innerExceptions", innerExceptions, typeof(SmtpFailedRecipientException[]));
        }
    }
}
