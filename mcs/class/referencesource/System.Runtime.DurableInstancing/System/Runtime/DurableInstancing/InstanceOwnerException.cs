//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Runtime.DurableInstancing
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceOwnerException : InstancePersistenceException
    {
        const string InstanceOwnerIdName = "instancePersistenceInstanceOwnerId";

        public InstanceOwnerException()
            : base(SRCore.InstanceOwnerDefault)
        {
        }

        public InstanceOwnerException(string message)
            : base(message)
        {
        }

        public InstanceOwnerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceOwnerException(XName commandName, Guid instanceOwnerId)
            : this(commandName, instanceOwnerId, null)
        {
        }

        public InstanceOwnerException(XName commandName, Guid instanceOwnerId, Exception innerException)
            : this(commandName, instanceOwnerId, ToMessage(instanceOwnerId), innerException)
        {
        }

        public InstanceOwnerException(XName commandName, Guid instanceOwnerId, string message, Exception innerException)
            : base(commandName, message, innerException)
        {
            InstanceOwnerId = instanceOwnerId;
        }

        [SecurityCritical]
        protected InstanceOwnerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            InstanceOwnerId = (Guid)info.GetValue(InstanceOwnerIdName, typeof(Guid));
        }

        public Guid InstanceOwnerId { get; private set; }

        [Fx.Tag.SecurityNote(Critical = "Overrides critical inherited method")]
        [SecurityCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureGetObjectDataOverrides,
            Justification = "Method is SecurityCritical")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(InstanceOwnerIdName, InstanceOwnerId, typeof(Guid));
        }

        static string ToMessage(Guid instanceOwnerId)
        {
            if (instanceOwnerId == Guid.Empty)
            {
                return SRCore.InstanceOwnerDefault;
            }
            return SRCore.InstanceOwnerSpecific(instanceOwnerId);
        }
    }
}
