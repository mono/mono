//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Persistence
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class InstanceLockException : PersistenceException
    {
        Guid id;

        public InstanceLockException()
            : this(SR2.GetString(SR2.CannotAcquireLockDefault), null)
        {
        }

        public InstanceLockException(string message)
            : this(message, null)
        {
        }

        public InstanceLockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceLockException(Guid id)
            : this(SR2.GetString(SR2.CannotAcquireLockSpecific, id))
        {
            this.id = id;
        }

        public InstanceLockException(Guid id, string message)
            : this(message, null)
        {
            this.id = id;
        }

        public InstanceLockException(Guid id, string message, Exception innerException)
            : base(message, innerException)
        {
            this.id = id;
        }

        public InstanceLockException(Guid id, Exception innerException)
            : this(SR2.GetString(SR2.CannotAcquireLockSpecific, id), innerException)
        {
            this.id = id;
        }

        protected InstanceLockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.id = (Guid) info.GetValue("id", typeof(Guid));
        }

        public Guid InstanceId
        {
            get { return this.id; }
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("id", id);
        }
    }
}
