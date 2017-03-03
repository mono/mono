
namespace System.Diagnostics.Private
{
	static partial class Debug
	{
		internal static IDebugLogger s_logger = new MonoDebugLogger();

		internal sealed class MonoDebugLogger : IDebugLogger
		{
			public void ShowAssertDialog(string stackTrace, string message, string detailMessage)
			{
				// FIXME should we g_error in this case?
			}

			public void WriteCore(string message)
			{
				// FIXME should we g_debug in this case?
			}
		}
	}
}
