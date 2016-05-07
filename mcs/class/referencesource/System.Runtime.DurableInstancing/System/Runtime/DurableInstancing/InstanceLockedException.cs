//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Xml.Linq;

    [Serializable]
    public class InstanceLockedException : InstancePersistenceCommandException
    {
        const string InstanceOwnerIdName = "instancePersistenceInstanceOwnerId";
        const string SerializableInstanceOwnerMetadataName = "instancePersistenceSerializableInstanceOwnerMetadata";

        public InstanceLockedException()
            : this(SRCore.CannotAcquireLockDefault, null)
        {
        }

        public InstanceLockedException(string message)
            : this(message, null)
        {
        }

        public InstanceLockedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId)
            : this(commandName, instanceId, null)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, Exception innerException)
            : this(commandName, instanceId, ToMessage(instanceId), innerException)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, string message, Exception innerException)
            : this(commandName, instanceId, Guid.Empty, null, message, innerException)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, Guid instanceOwnerId, IDictionary<XName, object> serializableInstanceOwnerMetadata)
            : this(commandName, instanceId, instanceOwnerId, serializableInstanceOwnerMetadata, null)
        {
        }

        public InstanceLockedException(XName commandName, Guid instanceId, Guid instanceOwnerId, IDictionary<XName, object> serializableInstanceOwnerMetadata, Exception innerException)
            : this(commandName, instanceId, instanceOwnerId, serializableInstanceOwnerMetadata, ToMessage(instanceId, instanceOwnerId), innerException)
        {
        }

        // Copying the dictionary snapshots it and makes sure the IDictionary implementation is serializable.
        public InstanceLockedException(XName commandName, Guid instanceId, Guid instanceOwnerId, IDictionary<XName, object> serializableInstanceOwnerMetadata, string message, Exception innerException)
            : base(commandName, instanceId, message, innerException)
        {
            InstanceOwnerId = instanceOwnerId;
            if (serializableInstanceOwnerMetadata != null)
            {
                Dictionary<XName, object> copy = new Dictionary<XName, object>(serializableInstanceOwnerMetadata);
                SerializableInstanceOwnerMetadata = new ReadOnlyDictionaryInternal<XName, object>(copy);
            }
        }

        [SecurityCritical]
        protected InstanceLockedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            InstanceOwnerId = (Guid)info.GetValue(InstanceOwnerIdName, typeof(Guid));
            SerializableInstanceOwnerMetadata = (ReadOnlyDictionaryInternal<XName, object>)info.GetValue(SerializableInstanceOwnerMetadataName, typeof(ReadOnlyDictionaryInternal<XName, object>));
        }

        public Guid InstanceOwnerId { get; private set; }

        public IDictionary<XName, object> SerializableInstanceOwnerMetadata { get; private set; }

        [Fx.Tag.SecurityNote(Critical = "Overrides critical inherited method")]
        [SecurityCritical]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureGetObjectDataOverrides,
            Justification = "Method is SecurityCritical")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(InstanceOwnerIdName, InstanceOwnerId, typeof(Guid));
            info.AddValue(SerializableInstanceOwnerMetadataName, SerializableInstanceOwnerMetadata, typeof(ReadOnlyDictionaryInternal<XName, object>));
        }

        static string ToMessage(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
            {
                return SRCore.CannotAcquireLockDefault;
            }
            return SRCore.CannotAcquireLockSpecific(instanceId);
        }

        static string ToMessage(Guid instanceId, Guid instanceOwnerId)
        {
            if (instanceId == Guid.Empty)
            {
                return SRCore.CannotAcquireLockDefault;
            }
            if (instanceOwnerId == Guid.Empty)
            {
                return SRCore.CannotAcquireLockSpecific(instanceId);
            }
            return SRCore.CannotAcquireLockSpecificWithOwner(instanceId, instanceOwnerId);
        }
    }
}
