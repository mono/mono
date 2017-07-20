namespace System.Diagnostics.Private
{
	static partial class Debug
	{
		static void ShowAssertDialog (string stackTrace, string message, string detailMessage)
		{
			// FIXME should we g_error in this case?
		}

		static void WriteCore (string message)
		{
			// FIXME should we g_debug in this case?
		}
	}
}
