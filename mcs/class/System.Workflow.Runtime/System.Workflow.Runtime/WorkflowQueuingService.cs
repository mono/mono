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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace System.Workflow.Runtime
{
	public sealed class WorkflowQueuingService
	{
		private IDictionary <IComparable, WorkflowQueue> queues;
		public static readonly DependencyProperty PendingMessagesProperty;

		static WorkflowQueuingService ()
		{
      			PendingMessagesProperty = DependencyProperty.RegisterAttached ("PendingMessages",
      				typeof (Queue), typeof (WorkflowQueuingService), new PropertyMetadata ());
      		}

		internal WorkflowQueuingService ()
		{
			 queues = new Dictionary <IComparable, WorkflowQueue> ();
		}

		// Methods
		public WorkflowQueue CreateWorkflowQueue (IComparable queueName, bool transactional)
		{
			WorkflowQueue queue;

			if (Exists (queueName)) {
				throw new InvalidOperationException ("A queue with this name already exists.");
			}

			queue = new WorkflowQueue (this, queueName);
			queues.Add (queueName, queue);

			//Console.WriteLine ("CreateWorkflowQueue {0}", queueName);
			return queue;
		}

		public void DeleteWorkflowQueue (IComparable queueName)
		{
			queues.Remove (queueName);
		}

		public bool Exists (IComparable queueName)
		{
			WorkflowQueue queue;
			return queues.TryGetValue (queueName, out queue);
		}

		public WorkflowQueue GetWorkflowQueue (IComparable queueName)
		{
			return queues [queueName];
		}
	}
}

