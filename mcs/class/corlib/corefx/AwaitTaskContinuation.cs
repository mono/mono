namespace System.Threading.Tasks
{
	partial class AwaitTaskContinuation
	{
		public void MarkAborted (ThreadAbortException e) {}
	}

	partial class CompletionActionInvoker
	{
		public void MarkAborted (ThreadAbortException e) {}
	}
}