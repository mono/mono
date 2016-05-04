//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    class WorkflowInstanceProvider : DurableInstanceProvider
    {
        WorkflowInstanceContextProvider instanceContextProvider;

        public WorkflowInstanceProvider(WorkflowInstanceContextProvider instanceContextProvider)
            : base(instanceContextProvider)
        {
            this.instanceContextProvider = instanceContextProvider;
        }

        public override void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            WorkflowDurableInstance workflowDurableInstance = null;

            //If InstanceContext is taken down due to Exception(Like PersistenceException); 
            //Make sure we inform LifeTimeManager to cleanup the record.
            if (instanceContext.State == CommunicationState.Faulted || instanceContext.Aborted)
            {
                if (this.instanceContextProvider.InstanceLifeTimeManager != null)
                {
                    workflowDurableInstance = (WorkflowDurableInstance) instance;
                    this.instanceContextProvider.InstanceLifeTimeManager.CleanUp(workflowDurableInstance.InstanceId);
                }
            }
            base.ReleaseInstance(instanceContext, instance);
        }
    }
}
