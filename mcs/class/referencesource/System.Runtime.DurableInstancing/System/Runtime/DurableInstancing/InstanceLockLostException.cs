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
    public class InstanceLockLostException : InstancePersistenceCommandException
    {
        public InstanceLockLostException()
            : this(SRCore.InstanceLockLostDefault, null)
        {
        }

        public InstanceLockLostException(string message)
            : this(message, null)
        {
        }

        public InstanceLockLostException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceLockLostException(XName commandName, Guid instanceId)
            : this(commandName, instanceId, null)
        {
        }

        public InstanceLockLostException(XName commandName, Guid instanceId, Exception innerException)
            : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        public InstanceLockLostException(XName commandName, Guid instanceId, string message, Exception innerException)
            : base(commandName, instanceId, message, innerException)
        {
        }

        [SecurityCritical]
        protected InstanceLockLostException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        static string ToMessage(Guid instanceId)
        {
            if (instanceId != Guid.Empty)
            {
                return SRCore.InstanceLockLostSpecific(instanceId);
            }
            return SRCore.InstanceLockLostDefault;
        }
    }
}
