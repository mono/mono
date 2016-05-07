using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Globalization;
using System.Workflow;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowAuthorizationException : SystemException
    {
        public WorkflowAuthorizationException()
            : base() { }
        public WorkflowAuthorizationException(String activityName, String principalName)
            : base(SR.GetString(SR.WorkflowAuthorizationException, activityName, principalName))
        {
        }
        public WorkflowAuthorizationException(string message) : base(message) { }

        public WorkflowAuthorizationException(string message, Exception innerException) : base(message, innerException) { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected WorkflowAuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
