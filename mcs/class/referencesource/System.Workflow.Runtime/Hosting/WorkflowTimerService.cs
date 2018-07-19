//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.     All rights    reserved.
//------------------------------------------------------------
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Threading;

namespace System.Workflow.Runtime.Hosting
{
    class WorkflowTimerService : WorkflowRuntimeService, ITimerService
    {
        public WorkflowTimerService()
            : base()
        {
        }

        public void ScheduleTimer(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            WorkflowSchedulerService schedulerService = this.Runtime.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService;
            schedulerService.Schedule(callback, workflowInstanceId, whenUtc, timerId);
        }

        public void CancelTimer(Guid timerId)
        {
            WorkflowSchedulerService schedulerService = this.Runtime.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService;
            schedulerService.Cancel(timerId);
        }
    }
}

