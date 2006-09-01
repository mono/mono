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

using System;
using System.ComponentModel;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Threading;

namespace System.Workflow.Runtime
{
	public sealed class WorkflowInstance
	{
		private Guid guid;
		private WorkflowRuntime runtime;
		private Activity root_activity;
		private TimerEventSubscriptionCollection subscription_collection;
		private WorkflowQueuingService queuing_service;
		internal Timer timer_subscriptions;

		internal WorkflowInstance (Guid guid, WorkflowRuntime runtime, Activity root_activity)
		{
			this.guid = guid;
			this.runtime = runtime;
			this.root_activity = root_activity;
			subscription_collection = new TimerEventSubscriptionCollection ();
			queuing_service = new WorkflowQueuingService ();
		}

		// Properties
      		public Guid InstanceId {
      			get {
      				return guid;
      			}
      		}

      		public WorkflowRuntime WorkflowRuntime {
      			get {
      				return runtime;
      			}
      		}

		// TODO: This breaks .Net API signature compatibility
      		public TimerEventSubscriptionCollection TimerEventSubscriptionCollection {
      			get {
      				return subscription_collection;
      			}
      		}

      		// TODO: This breaks .Net API signature compatibility
      		public WorkflowQueuingService WorkflowQueuingService {
      			get {
      				return queuing_service;
      			}
      		}

		// Methods
		public void Abort ()
		{
			timer_subscriptions.Dispose ();
		}

		//public void ApplyWorkflowChanges (WorkflowChanges workflowChanges);

		public void EnqueueItem (IComparable queueName, object item, IPendingWork pendingWork, object workItem)
		{
			WorkflowQueue queue;

			// TODO: What to do with pendingWork and workItem?
			if (queuing_service.Exists (queueName)) {
				queue = queuing_service.GetWorkflowQueue (queueName);
			} else {
				queue = queuing_service.CreateWorkflowQueue (queueName, true);
			}

			queue.Enqueue (item);
		}

		public void EnqueueItemOnIdle (IComparable queueName, object item, IPendingWork pendingWork, object workItem)
		{

		}

		public override bool Equals (object obj)
		{
			WorkflowInstance wi = (WorkflowInstance) obj;

			if (wi == null) {
				return false;
			}

			return wi.InstanceId.Equals (guid);
		}

		public override int GetHashCode ()
		{
			return guid.GetHashCode ();
		}

		public Activity GetWorkflowDefinition ()
		{
			return root_activity;
		}

		public DateTime GetWorkflowNextTimerExpiration ()
		{
			TimerEventSubscription timer;
			timer = subscription_collection.Peek ();

			if (timer == null) {
				return DateTime.MaxValue;
			}

			return timer.ExpiresAt;
		}

		/*public ReadOnlyCollection<WorkflowQueueInfo> GetWorkflowQueueData();*/
		public void Load ()
		{

		}

		public void ReloadTrackingProfiles ()
		{

		}

		public void Resume ()
		{

		}

		public void Start ()
		{
			WorkflowSchedulerService sheduler;

			// init all activities
			ActivityExecutionContextManager manager = new ActivityExecutionContextManager (this);
			ActivityExecutionContext context = manager.CreateExecutionContext (GetWorkflowDefinition ());

			GetWorkflowDefinition ().InitializeInternal (context);

			sheduler = (WorkflowSchedulerService) runtime.GetService (typeof (WorkflowSchedulerService));
			//sheduler.Schedule (new WaitCallback (WorkflowProcessor.RunWorkflow), guid);

			WorkflowProcessor.RunWorkflow (guid);
		}

		public void Suspend (string error)
		{

		}

		public void Terminate (string error)
		{

		}

		public bool TryUnload ()
		{
			throw new NotImplementedException ();
		}

		public void Unload ()
		{

		}
	}
}

