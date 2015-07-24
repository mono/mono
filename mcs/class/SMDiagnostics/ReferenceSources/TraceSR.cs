using System.Globalization;

static partial class TraceSR
{
	public const string ThrowingException = "throwing exception.";
	public const string StringNullOrEmpty = "Argument string is null or empty.";
	public const string GenericCallbackException = "Callback exception has occured.";
	public const string TraceHandledException = "Trace handled exception.";
	public const string TraceCodeTraceTruncatedQuotaExceeded = "TraceTruncatedQuotaExceeded";
	public const string TraceCodeAppDomainUnload = "AppDomainUnload";
	public const string UnhandledException = "Unhandled exception.";
	public const string TraceCodeEventLog = "EventLog";
	public const string WriteCharsInvalidContent = "invalid content.";

	internal static string GetString(string name, params object[] args)
	{
		return GetString (CultureInfo.InvariantCulture, name, args);
	}

	internal static string GetString(CultureInfo culture, string name, params object[] args)
	{
		return string.Format (culture, name, args);
	}

	internal static string GetString(string name)
	{
		return name;
	}

	internal static string GetString(CultureInfo culture, string name)
	{
		return name;
	}
}

