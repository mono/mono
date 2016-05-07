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
    public class InstanceKeyCompleteException : InstancePersistenceCommandException
    {
        const string InstanceKeyName = "instancePersistenceInstanceKey";

        public InstanceKeyCompleteException()
            : this(SRCore.KeyNotReadyDefault, null)
        {
        }

        public InstanceKeyCompleteException(string message)
            : this(message, null)
        {
        }

        public InstanceKeyCompleteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceKeyCompleteException(XName commandName, InstanceKey instanceKey)
            : this(commandName, instanceKey, null)
        {
        }

        public InstanceKeyCompleteException(XName commandName, InstanceKey instanceKey, Exception innerException)
            : this(commandName, Guid.Empty, instanceKey, ToMessage(instanceKey), innerException)
        {
        }

        public InstanceKeyCompleteException(XName commandName, Guid instanceId, InstanceKey instanceKey, string message, Exception innerException)
            : base(commandName, instanceId, message, innerException)
        {
            InstanceKey = instanceKey;
        }

        [SecurityCritical]
        protected InstanceKeyCompleteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Guid guid = (Guid)info.GetValue(InstanceKeyName, typeof(Guid));
            InstanceKey = guid == Guid.Empty ? null : new InstanceKey(guid);
        }

        public InstanceKey InstanceKey { get; private set; }

        [Fx.Tag.SecurityNote(Critical = "Overrides critical inherited method")]
        [SecurityCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureGetObjectDataOverrides,
            Justification = "Method is SecurityCritical")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(InstanceKeyName, (InstanceKey != null && InstanceKey.IsValid) ? InstanceKey.Value : Guid.Empty, typeof(Guid));
        }

        static string ToMessage(InstanceKey instanceKey)
        {
            if (instanceKey != null && instanceKey.IsValid)
            {
                return SRCore.KeyCompleteSpecific(instanceKey.Value);
            }
            return SRCore.KeyCompleteDefault;
        }
    }
}
