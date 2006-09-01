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

namespace System.Workflow.Runtime
{
	[Serializable]
	public class TimerEventSubscription
	{
		private Guid timerId;
		private Guid workflowInstanceId;
		private DateTime expiresAt;
		private IComparable name;

		protected TimerEventSubscription ()
		{

		}

		public TimerEventSubscription (Guid workflowInstanceId, DateTime expiresAt) : this ()
		{
			this.workflowInstanceId = workflowInstanceId;
			this.expiresAt = expiresAt;
			this.timerId = Guid.NewGuid ();
			this.name = timerId;
		}

		public TimerEventSubscription (Guid timerId, Guid workflowInstanceId, DateTime expiresAt) : this ()
		{
			this.timerId = timerId;
			this.workflowInstanceId = workflowInstanceId;
			this.expiresAt = expiresAt;
			this.name = timerId;
		}

		// Properties
		public virtual DateTime ExpiresAt {
			get { return expiresAt; }
		}

		public virtual IComparable QueueName {
			get {return name; }
			protected set {name = value; }
		}

		public virtual Guid SubscriptionId {
			get { return timerId; }
		}

		public virtual Guid WorkflowInstanceId {
			get { return workflowInstanceId; }
		}
	}
}


