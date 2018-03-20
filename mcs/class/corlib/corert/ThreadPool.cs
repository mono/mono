namespace System.Threading
{
	partial class ThreadPool
	{
		internal static bool IsThreadPoolThread => Thread.CurrentThread.IsThreadPoolThread;
	}
}