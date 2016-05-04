//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class MsmqPoisonMessageException : PoisonMessageException
    {
        long messageLookupId = 0;

        public MsmqPoisonMessageException() { }
        public MsmqPoisonMessageException(string message) : base(message) { }
        public MsmqPoisonMessageException(string message, Exception innerException) : base(message, innerException) { }
        public MsmqPoisonMessageException(long messageLookupId) : this(messageLookupId, null) { }
        public MsmqPoisonMessageException(long messageLookupId, Exception innerException)
            : base(SR.GetString(SR.MsmqPoisonMessage), innerException)
        {
            this.messageLookupId = messageLookupId;
        }

        public long MessageLookupId
        {
            get { return this.messageLookupId; }
        }

        protected MsmqPoisonMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.messageLookupId = (long)info.GetValue("messageLookupId", typeof(long));
        }

#pragma warning disable 688 // This is a Level1 assembly: a Level2 [SecurityCrital] on public members are turned into [SecuritySafeCritical] + LinkDemand
        [Fx.Tag.SecurityNote(Critical = "Overrides the base.GetObjectData which is critical, as well as calling this method.",
            Safe = "Replicates the LinkDemand.")]
        [SecurityCritical]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("messageLookupId", this.messageLookupId);
        }
#pragma warning restore 688
    }
}
