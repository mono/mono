using System;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Schema;
    
namespace System.Workflow.Runtime
{
    #region State Persistence Exceptions

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowOwnershipException : Exception
    {
        private Guid _instanceId;
        public Guid InstanceId
        {
            get
            {
                return _instanceId;
            }
            set
            {
                _instanceId = value;
            }
        }
        public WorkflowOwnershipException()
            : base(ExecutionStringManager.WorkflowOwnershipException)
        { }

        public WorkflowOwnershipException(string message)
            : base(message)
        { }

        public WorkflowOwnershipException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public WorkflowOwnershipException(Guid instanceId)
            : base(ExecutionStringManager.WorkflowOwnershipException)
        {
            this.InstanceId = instanceId;
        }

        public WorkflowOwnershipException(Guid instanceId, string message)
            : base(message)
        {
            this.InstanceId = instanceId;
        }

        public WorkflowOwnershipException(Guid instanceId, string message, Exception innerException)
            : base(message, innerException)
        {
            this.InstanceId = instanceId;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected WorkflowOwnershipException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _instanceId = (Guid)info.GetValue("__instanceId__", typeof(Guid));
        }

        //ISerializable override to store custom state
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
                throw new ArgumentNullException("info");

            base.GetObjectData(info, context);
            info.AddValue("__instanceId__", _instanceId);
        }

    }

    #endregion
}
