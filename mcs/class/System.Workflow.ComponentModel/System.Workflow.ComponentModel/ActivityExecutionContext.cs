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

#if RUNTIME_DEP
using System.Workflow.Runtime;
#endif

namespace System.Workflow.ComponentModel
{
	public sealed class ActivityExecutionContext : IServiceProvider, IDisposable
	{
		public static readonly DependencyProperty CurrentExceptionProperty;

		private Activity activity;
		private Guid guid;
		private ActivityExecutionContextManager manager;

		internal ActivityExecutionContext (ActivityExecutionContextManager manager, Activity activity)
		{
			this.activity = activity;
			this.manager = manager;
			guid = Guid.NewGuid ();
		}

		// Properties
		public Activity Activity {
			get {return activity; }
		}

		public Guid ContextGuid {
			get {return guid; }
		}

		public ActivityExecutionContextManager ExecutionContextManager {
			get {return manager; }
		}

		public T GetService <T> ()
		{
			return (T) GetService (typeof (T));
		}

		public object GetService (Type serviceType)
		{
		#if RUNTIME_DEP

			if (serviceType == typeof (WorkflowQueuingService)) {
				return manager.Workflow.WorkflowQueuingService;
			}

			return manager.Workflow.WorkflowRuntime.GetService (serviceType);
		#else
			throw new NotImplementedException ();
		#endif

		}

		// Methods
		public void CancelActivity (Activity activity)
		{

		}

		public void CloseActivity ()
		{

		}

		public void ExecuteActivity (Activity activity)
		{
			activity.Execute (this);
		}

		void System.IDisposable.Dispose ()
		{

		}

		//public void TrackData(object userData);
		//public void TrackData(string userDataKey, object userData);

	}
}

