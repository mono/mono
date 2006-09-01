// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//


using System.Xml;
using System.Threading;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime.Hosting
{
	public class DefaultWorkflowSchedulerService : WorkflowSchedulerService
	{
		public DefaultWorkflowSchedulerService ()
		{

		}

		protected internal override void Cancel (Guid timerId)
		{

		}

		protected internal override void Schedule (WaitCallback callback, Guid workflowInstanceId)
		{
			ThreadPool.QueueUserWorkItem (callback, workflowInstanceId);
		}

		protected internal override void Schedule (WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
		{
			//WorkflowInstance wi = WorkflowRuntime.GetInstanceFromGuid (workflowInstanceId);

			//wi.TimerEventSubscriptionCollection.Add
			//	(new TimerEventSubscription (workflowInstanceId, timerId));
		}

	}
}

