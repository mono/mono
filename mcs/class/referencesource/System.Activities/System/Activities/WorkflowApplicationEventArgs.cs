//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class WorkflowApplicationEventArgs : EventArgs
    {
        internal WorkflowApplicationEventArgs(WorkflowApplication application)
        {
            this.Owner = application;
        }

        internal WorkflowApplication Owner
        {
            get;
            private set;
        }

        public Guid InstanceId
        {
            get
            {
                return this.Owner.Id;
            }
        }

        public IEnumerable<T> GetInstanceExtensions<T>()
            where T : class
        {
            return this.Owner.InternalGetExtensions<T>();
        }
    }
}
