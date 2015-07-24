using System;
using System.Collections;
using System.IO;

namespace System.Diagnostics
{
	class AssertWrapper
	{
		public static void ShowAssert(string stackTrace, StackFrame frame, string message, string detailMessage)
		{
			new DefaultTraceListener ().Fail (message, detailMessage);
		}
	}
}

