using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;
using System.Security;
using System.Security.Permissions;

namespace System.Threading.Tasks
{
	// these members were copied from https://github.com/mono/mono/blob/7b4dfeebc40cf8c027819b8b7bd85a4e7c87ad50/mcs/class/referencesource/mscorlib/system/threading/Tasks/Task.cs#L220-L246
	partial class Task
	{
		// This dictonary relates the task id, from an operation id located in the Async Causality log to the actual
		// task. This is to be used by the debugger ONLY. Task in this dictionary represent current active tasks.
		private static readonly Dictionary<int, Task> s_currentActiveTasks = new Dictionary<int, Task> ();
		private static readonly Object s_activeTasksLock = new Object ();

		// These methods are a way to access the dictionary both from this class and for other classes that also
		// activate dummy tasks. Specifically the AsyncTaskMethodBuilder and AsyncTaskMethodBuilder<>
		[FriendAccessAllowed]
		internal static bool AddToActiveTasks (Task task)
		{
			Contract.Requires (task != null, "Null Task objects can't be added to the ActiveTasks collection");
			lock (s_activeTasksLock)
			{
				s_currentActiveTasks[task.Id] = task;
			}
			//always return true to keep signature as bool for backwards compatibility
			return true;
		}

		[FriendAccessAllowed]
		internal static void RemoveFromActiveTasks (int taskId)
		{
			lock (s_activeTasksLock)
			{
				s_currentActiveTasks.Remove (taskId);
			}
		}

		public void MarkAborted (ThreadAbortException e) {}

		// Copied from reference-sources
		[SecurityCritical]
		void ExecuteWithThreadLocal (ref Task currentTaskSlot)
		{
			// Remember the current task so we can restore it after running, and then
			Task previousTask = currentTaskSlot;
			try
			{
				// place the current task into TLS.
				currentTaskSlot = this;

				ExecutionContext ec = CapturedContext;
				if (ec == null)
				{
					// No context, just run the task directly.
					Execute ();
				}
				else
				{
					// Run the task.  We need a simple shim that converts the
					// object back into a Task object, so that we can Execute it.

					// Lazily initialize the callback delegate; benign ----
					var callback = s_ecCallback;
					if (callback == null) s_ecCallback = callback = new ContextCallback (ExecutionContextCallback);
#if PFX_LEGACY_3_5
					ExecutionContext.Run (ec, callback, this);
#else
					ExecutionContext.Run (ec, callback, this, true);
#endif
				}

				if (AsyncCausalityTracer.LoggingOn)
					AsyncCausalityTracer.TraceSynchronousWorkCompletion (CausalityTraceLevel.Required, CausalitySynchronousWork.Execution);

				Finish (true);
			}
			finally
			{
				currentTaskSlot = previousTask;
			}
		}
	}
}