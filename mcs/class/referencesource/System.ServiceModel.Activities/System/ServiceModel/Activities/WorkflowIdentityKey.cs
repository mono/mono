// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
        
    internal class WorkflowIdentityKey : IEquatable<WorkflowIdentityKey>
    {
        public WorkflowIdentityKey(WorkflowIdentity definitionIdentity)
        {
            this.Identity = definitionIdentity;
        }

        public WorkflowIdentity Identity
        {
            get;
            private set;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            if (this.Identity != null)
            {
                hashCode ^= this.Identity.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            WorkflowIdentityKey workflowIdentityKey = obj as WorkflowIdentityKey;
            if (workflowIdentityKey == null)
            {
                return false;
            }
            else
            {
                return this.Equals(workflowIdentityKey);
            }
        }

        public bool Equals(WorkflowIdentityKey other)
        {
            if (other != null)
            {
                if (this.Identity != null)
                {
                    return this.Identity.Equals(other.Identity);
                }
                else
                {
                    return object.ReferenceEquals(this.Identity, other.Identity);
                }
            }

            return false;
        }
    }
}
