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
using System.Collections.Generic;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime
{
	public sealed class WorkflowQueue
	{
		private Queue <object> queue;
		private WorkflowQueuingService service;
		private IComparable queue_name;

		internal WorkflowQueue (WorkflowQueuingService service, IComparable queue_name)
		{
			queue = new Queue <object> ();
			this.service = service;
			this.queue_name = queue_name;
		}

		// Properties
		public int Count {
			get { return queue.Count; }
		}

		//public bool Enabled { get; set; }
		public IComparable QueueName {
			get { return queue_name; }
		}

		public WorkflowQueuingService QueuingService {
			get {return service; }
		}

		// Events
		public event EventHandler <QueueEventArgs> QueueItemArrived;
		public event EventHandler <QueueEventArgs> QueueItemAvailable;

		// Methods
		public object Dequeue ()
		{
			return queue.Dequeue ();
		}

		public void Enqueue (object item)
		{
			queue.Enqueue (item);

			if (QueueItemArrived != null) {
				QueueItemArrived (this, new QueueEventArgs (queue_name));
			}		
		}

		public object Peek ()
		{
			return queue.Peek ();
		}

		public void RegisterForQueueItemArrived (IActivityEventListener<QueueEventArgs> eventListener)
		{

		}

		public void RegisterForQueueItemAvailable (IActivityEventListener<QueueEventArgs> eventListener)
		{

		}

		public void RegisterForQueueItemAvailable (IActivityEventListener<QueueEventArgs> eventListener, string subscriberQualifiedName)
		{

		}

		public void UnregisterForQueueItemArrived (IActivityEventListener<QueueEventArgs> eventListener)
		{

		}

		public void UnregisterForQueueItemAvailable (IActivityEventListener<QueueEventArgs> eventListener)
		{

		}
	}
}

