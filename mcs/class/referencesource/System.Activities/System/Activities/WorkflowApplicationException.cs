//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class WorkflowApplicationException : Exception
    {
        const string InstanceIdName = "instanceId";
        Guid instanceId;

        public WorkflowApplicationException()
            : base(SR.DefaultWorkflowApplicationExceptionMessage)
        {
        }

        public WorkflowApplicationException(string message)
            : base(message)
        {
        }

        public WorkflowApplicationException(string message, Guid instanceId)
            : base(message)
        {
            this.instanceId = instanceId;
        }

        public WorkflowApplicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowApplicationException(string message, Guid instanceId, Exception innerException)
            : base(message, innerException)
        {
            this.instanceId = instanceId;
        }

        protected WorkflowApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.instanceId = (Guid)info.GetValue(InstanceIdName, typeof(Guid));
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are overriding a critical method in the base class.")]
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(InstanceIdName, this.instanceId);
        }
    }
}
