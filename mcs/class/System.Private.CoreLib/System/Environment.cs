using System.Globalization;

namespace System
{
	partial class Environment
	{
#region referencesource dependencies
		internal static string GetResourceString (string key)
		{
			return key;
		}

		internal static string GetResourceString (string key, CultureInfo culture)
		{
			return key;
		}

		internal static string GetResourceString (string key, params object[] values)
		{
			return string.Format (CultureInfo.InvariantCulture, key, values);
		}

		internal static String GetStackTrace (Exception e, bool needFileInfo)
		{
			throw new NotImplementedException ();
		}

		internal static int GetPageSize () => 0;
#endregion

		public static string GetEnvironmentVariable (string variable)
		{
			throw new NotImplementedException ();
		}

		internal static string GetResourceStringLocal (string key)
		{
			throw new NotImplementedException ();
		}

		public static string NewLine
		{
			get
			{
#if PLATFORM_WINDOWS
				return "\r\n";
#else
				return "\n";
#endif
			}
		}

		public static bool HasShutdownStarted
		{
			get 
			{
				return false;
			}
		}

		internal static bool UserInteractive { get; } = false;

		public static int CurrentManagedThreadId { get; } = 1;

		public static int ProcessorCount { get; } = 1;

		public static int TickCount { get; } = 1;

		public static void FailFast (string message)
		{
		}

		public static void FailFast(string message, Exception exception)
		{
		}

		public static void FailFast (string message, Exception exception, string errorMessage)
		{
		}
	}
}