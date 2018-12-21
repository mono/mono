namespace System
{
	partial struct DateTime
	{
		internal const bool s_systemSupportsLeapSeconds = false;

		internal static DateTime FromFileTimeLeapSecondsAware (long fileTime) => default (DateTime);
		internal static long ToFileTimeLeapSecondsAware (long ticks) => default (long);

		// IsValidTimeWithLeapSeconds is not expected to be called at all for now on non-Windows platforms
		internal static bool IsValidTimeWithLeapSeconds (int year, int month, int day, int hour, int minute, int second, DateTimeKind kind) => false;
	}
}