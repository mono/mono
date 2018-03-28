//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    public class WorkflowCreationContext
    {
        Dictionary<string, object> workflowArguments;

        public WorkflowCreationContext()
        {
        }

        public IDictionary<string, object> WorkflowArguments
        {
            get
            {
                if (this.workflowArguments == null)
                {
                    this.workflowArguments = new Dictionary<string, object>();
                }
                return this.workflowArguments;
            }
        }

        // internally we can handle null and optimize out the allocation
        internal IDictionary<string, object> RawWorkflowArguments
        {
            get
            {
                return this.workflowArguments;
            }
        }

        [DataMember]
        public bool CreateOnly
        {
            get;
            set;
        }

        [DataMember]
        public bool IsCompletionTransactionRequired
        {
            get;
            set;
        }

        protected internal virtual IAsyncResult OnBeginWorkflowCompleted(
            ActivityInstanceState completionState, IDictionary<string, object> workflowOutputs,
            Exception terminationException, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected internal virtual void OnEndWorkflowCompleted(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected internal virtual void OnAbort()
        {
        }
    }
}
