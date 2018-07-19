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
    public class InstanceNotFoundException : PersistenceException
    {
        Guid id;

        public InstanceNotFoundException()
            : this(SR2.GetString(SR2.InstanceNotFoundDefault), null)
        {
        }

        public InstanceNotFoundException(string message)
            : this(message, null)
        {
        }

        public InstanceNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InstanceNotFoundException(Guid id)
            : this(SR2.GetString(SR2.InstanceNotFoundSpecific, id))
        {
            this.id = id;
        }

        public InstanceNotFoundException(Guid id, string message)
            : this(message, null)
        {
            this.id = id;
        }

        public InstanceNotFoundException(Guid id, string message, Exception innerException)
            : base(message, innerException)
        {
            this.id = id;
        }

        public InstanceNotFoundException(Guid id, Exception innerException)
            : this(SR2.GetString(SR2.InstanceNotFoundSpecific, id), innerException)
        {
            this.id = id;
        }

        protected InstanceNotFoundException(SerializationInfo info, StreamingContext context)
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
