//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Runtime;

    [Serializable]
    public class VersionMismatchException : Exception
    {
        public VersionMismatchException()
            : base()
        {
        }

        public VersionMismatchException(string message)
            : base(message)
        {
        }

        public VersionMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public VersionMismatchException(WorkflowIdentity expectedVersion, WorkflowIdentity actualVersion)
            : base(GetMessage(expectedVersion, actualVersion))
        {
            this.ExpectedVersion = expectedVersion;
            this.ActualVersion = actualVersion;
        }

        public VersionMismatchException(string message, WorkflowIdentity expectedVersion, WorkflowIdentity actualVersion)
            : base(message)
        {
            this.ExpectedVersion = expectedVersion;
            this.ActualVersion = actualVersion;
        }

        public VersionMismatchException(string message, WorkflowIdentity expectedVersion, WorkflowIdentity actualVersion, Exception innerException)
            : base(message, innerException)
        {
            this.ExpectedVersion = expectedVersion;
            this.ActualVersion = actualVersion;
        }

        protected VersionMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ExpectedVersion = (WorkflowIdentity)info.GetValue("expectedVersion", typeof(WorkflowIdentity));
            this.ActualVersion = (WorkflowIdentity)info.GetValue("actualVersion", typeof(WorkflowIdentity));
        }

        public WorkflowIdentity ExpectedVersion
        {
            get;
            private set;
        }

        public WorkflowIdentity ActualVersion
        {
            get;
            private set;
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are overriding a critical method in the base class.")]
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("expectedVersion", this.ExpectedVersion);
            info.AddValue("actualVersion", this.ActualVersion);
        }

        private static string GetMessage(WorkflowIdentity expectedVersion, WorkflowIdentity actualVersion)
        {
            if (actualVersion == null && expectedVersion != null)
            {
                return SR.WorkflowIdentityNullStateId(expectedVersion);
            }
            else if (actualVersion != null && expectedVersion == null)
            {
                return SR.WorkflowIdentityNullHostId(actualVersion);
            }
            else if (!object.Equals(expectedVersion, actualVersion))
            {
                return SR.WorkflowIdentityStateIdHostIdMismatch(actualVersion, expectedVersion);
            }
            else
            {
                return null;
            }
        }
    }    
}
