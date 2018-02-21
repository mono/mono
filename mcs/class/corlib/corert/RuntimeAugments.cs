using System;

namespace Internal.Runtime.Augments
{
	partial class RuntimeAugments
	{
		public static void ReportUnhandledException (Exception exception)
		{
			throw exception;
		}
	}
}