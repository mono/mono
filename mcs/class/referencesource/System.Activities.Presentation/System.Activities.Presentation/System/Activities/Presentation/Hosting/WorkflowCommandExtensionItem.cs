//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    //WorkflowCommandExtensionItem - this class allows user to update each workflow's designer command 
    //(i.e. input gestures collection). user has to provide IWorkflowCommandExtensionCallback implementation
    //to get notifications flowing in.
    public sealed class WorkflowCommandExtensionItem : ContextItem
    {
        public WorkflowCommandExtensionItem()
        {
        }

        public WorkflowCommandExtensionItem(IWorkflowCommandExtensionCallback callback)
        {
            if (null == callback)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("callback"));
            }
            this.CommandExtensionCallback = callback;
        }

        public override Type ItemType
        {
            get { return typeof(WorkflowCommandExtensionItem); }
        }

        internal IWorkflowCommandExtensionCallback CommandExtensionCallback
        {
            get;
            private set;
        }
    }
}
