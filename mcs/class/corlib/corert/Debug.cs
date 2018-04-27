namespace System.Diagnostics.Private
{
	static partial class Debug
	{
		static void ShowDialog (string stackTrace, string message, string detailMessage, string errorSource)
		{
			// FIXME should we g_error in this case?
		}

		static void WriteCore (string message)
		{
			// FIXME should we g_debug in this case?
		}
	}
}
