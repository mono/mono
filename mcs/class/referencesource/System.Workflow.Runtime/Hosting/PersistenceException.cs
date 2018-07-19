using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Workflow;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime.Hosting
{
    #region Runtime Exceptions
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class PersistenceException : SystemException
    {
        public PersistenceException() : base(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.PersistenceException)) { }
        public PersistenceException(string message) : base(message) { }
        public PersistenceException(string message, Exception innerException) : base(message, innerException) { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected PersistenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    #endregion
}
