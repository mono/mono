//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Runtime.Diagnostics;

    class Activity : IDisposable
    {
        protected Guid parentId;
        Guid currentId;
        bool mustDispose = false;

        protected Activity(Guid activityId, Guid parentId)
        {
            this.currentId = activityId;
            this.parentId = parentId;
            this.mustDispose = true;
            DiagnosticTraceBase.ActivityId = this.currentId;
        }

        internal static Activity CreateActivity(Guid activityId)
        {
            Activity retval = null;
            if (activityId != Guid.Empty)
            {
                Guid currentActivityId = DiagnosticTraceBase.ActivityId;
                if (activityId != currentActivityId)
                {
                    retval = new Activity(activityId, currentActivityId);
                }
            }
            return retval;
        }

        public virtual void Dispose()
        {
            if (this.mustDispose)
            {
                this.mustDispose = false;
                DiagnosticTraceBase.ActivityId = this.parentId;
            }
            GC.SuppressFinalize(this);
        }

        protected Guid Id
        {
            get { return this.currentId; }
        }
    }
}
