using System.Threading.Tasks;

namespace Internal.Threading.Tasks
{
	public static class AsyncCausalitySupport
	{
		public static void AddToActiveTasks(Task task)
		{
		}

		public static void RemoveFromActiveTasks(Task task)
		{
		}

		public static bool LoggingOn
		{
			get
			{
				return false;
			}
		}

		public static void TraceOperationCreation(Task task, string operationName)
		{
		}

		public static void TraceOperationCompletedSuccess(Task task)
		{
		}

		public static void TraceOperationCompletedError(Task task)
		{
		}

		public static void TraceSynchronousWorkStart(Task task)
		{
		}

		public static void TraceSynchronousWorkCompletion()
		{
		}
	}
}