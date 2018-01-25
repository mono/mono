using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
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
	}
}