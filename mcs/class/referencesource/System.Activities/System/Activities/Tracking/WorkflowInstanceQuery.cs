//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Tracking
{
    using System.Collections.ObjectModel;

    public class WorkflowInstanceQuery : TrackingQuery
    {
        Collection<string> states;

        public WorkflowInstanceQuery()
        {
        }

        public Collection<string> States
        {
            get
            {
                if (this.states == null)
                {
                    this.states = new Collection<string>();
                }
                return this.states;
            }
        }

        internal bool HasStates
        {
            get
            {
                return this.states != null && this.states.Count > 0;
            }
        }

    }
}
