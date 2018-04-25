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
    public class InstanceCollisionException : InstancePersistenceCommandException
    {
        public InstanceCollisionException()
            : this(SRCore.InstanceCollisionDefault, null)
        {
        }

        public InstanceCollisionException(string message)
            : this(message, null)
        {
        }

        public InstanceCollisionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceCollisionException(XName commandName, Guid instanceId)
            : this(commandName, instanceId, null)
        {
        }

        public InstanceCollisionException(XName commandName, Guid instanceId, Exception innerException)
            : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        public InstanceCollisionException(XName commandName, Guid instanceId, string message, Exception innerException)
            : base(commandName, instanceId, message, innerException)
        {
        }

        [SecurityCritical]
        protected InstanceCollisionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        static string ToMessage(Guid instanceId)
        {
            if (instanceId != Guid.Empty)
            {
                return SRCore.InstanceCollisionSpecific(instanceId);
            }
            return SRCore.InstanceCollisionDefault;
        }
    }
}
