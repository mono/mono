//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Threading;
    using System.Xml.Linq;

    // Wrapper over instance data retrieved from the Instance Store but not yet loaded into a WorkflowApplication.
    // Once this instance is loaded into a WFApp using WFApp.Load(), this object is stale and trying to abort or reload it wil throw.
    // Free-threaded: needs to be resillient to simultaneous loads/aborts on multiple threads
    public class WorkflowApplicationInstance
    {
        private int state;

        internal WorkflowApplicationInstance(
            WorkflowApplication.PersistenceManagerBase persistenceManager, 
            IDictionary<XName, InstanceValue> values, 
            WorkflowIdentity definitionIdentity)
        {
            this.PersistenceManager = persistenceManager;
            this.Values = values;
            this.DefinitionIdentity = definitionIdentity;
            this.state = (int)State.Initialized;
        }

        private enum State
        {
            Initialized,
            Loaded,
            Aborted
        }

        public WorkflowIdentity DefinitionIdentity
        {
            get;
            private set;
        }

        public InstanceStore InstanceStore
        {
            get
            {
                return this.PersistenceManager.InstanceStore;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.PersistenceManager.InstanceId;
            }
        }

        internal WorkflowApplication.PersistenceManagerBase PersistenceManager
        {
            get;
            private set;
        }

        internal IDictionary<XName, InstanceValue> Values
        {
            get;
            private set;
        }

        public void Abandon()
        {
            this.Abandon(ActivityDefaults.DeleteTimeout);
        }

        public void Abandon(TimeSpan timeout)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            this.MarkAsAbandoned();
            WorkflowApplication.DiscardInstance(this.PersistenceManager, timeout);
        }

        public IAsyncResult BeginAbandon(AsyncCallback callback, object state)
        {
            return this.BeginAbandon(ActivityDefaults.DeleteTimeout, callback, state);
        }

        public IAsyncResult BeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);

            this.MarkAsAbandoned();
            return WorkflowApplication.BeginDiscardInstance(this.PersistenceManager, timeout, callback, state);
        }

        public void EndAbandon(IAsyncResult asyncResult)
        {
            WorkflowApplication.EndDiscardInstance(asyncResult);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "Approved Design. Returning a bool makes the intent much clearer than something that just returns a list.")]
        public bool CanApplyUpdate(DynamicUpdateMap updateMap, out IList<ActivityBlockingUpdate> activitiesBlockingUpdate)
        {
            if (updateMap == null)
            {
                throw FxTrace.Exception.ArgumentNull("updateMap");
            }

            activitiesBlockingUpdate = WorkflowApplication.GetActivitiesBlockingUpdate(this, updateMap);
            return activitiesBlockingUpdate == null || activitiesBlockingUpdate.Count == 0;
        }

        internal void MarkAsLoaded()
        {
            int oldState = Interlocked.CompareExchange(ref this.state, (int)State.Loaded, (int)State.Initialized);
            this.ThrowIfLoadedOrAbandoned((State)oldState);
        }

        private void MarkAsAbandoned()
        {
            int oldState = Interlocked.CompareExchange(ref this.state, (int)State.Aborted, (int)State.Initialized);
            this.ThrowIfLoadedOrAbandoned((State)oldState);
        }

        private void ThrowIfLoadedOrAbandoned(State oldState)
        {
            if (oldState == State.Loaded)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowApplicationInstanceLoaded));
            }

            if (oldState == State.Aborted)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowApplicationInstanceAbandoned));
            }
        }
    }
}
