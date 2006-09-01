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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Workflow.ComponentModel;

#if RUNTIME_DEP
using System.Workflow.Runtime;
#endif

namespace System.Workflow.ComponentModel
{
	public sealed class ActivityExecutionContextManager
	{
		private object workflow_instance;

		// TODO: Breaks API signature
		public ActivityExecutionContextManager (object state)
		{
			this.workflow_instance = state;
		}

		// Properties
		[MonoTODO]
		public ReadOnlyCollection <ActivityExecutionContext> ExecutionContexts  {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public IEnumerable <Guid> PersistedExecutionContexts {
			get {
				throw new NotImplementedException ();
			}
		}

		#if RUNTIME_DEP
		// TODO: This breaks API compatibility
		public WorkflowInstance Workflow {
			get {return (WorkflowInstance) workflow_instance; }
		}

		#endif

		// Methods
		public void CompleteExecutionContext (ActivityExecutionContext childContext)
		{
			CompleteExecutionContext (childContext, false); // TODO: Check if false is correct
		}

		[MonoTODO]
		public void CompleteExecutionContext (ActivityExecutionContext childContext, bool forcePersist)
		{
			throw new NotImplementedException ();
		}

		public ActivityExecutionContext CreateExecutionContext (Activity activity)
		{
			ActivityExecutionContext context = new ActivityExecutionContext (this, activity);
			return context;
		}

		[MonoTODO]
		public ActivityExecutionContext GetExecutionContext (Activity activity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ActivityExecutionContext GetPersistedExecutionContext (Guid contextGuid)
		{
			throw new NotImplementedException ();
		}
	}
}

