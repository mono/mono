using System.Globalization;

namespace System
{
	partial class Environment
	{
		internal static string GetResourceString (string key)
		{
			return Messages.GetMessage (key);
		}

		internal static string GetResourceString (string key, CultureInfo culture)
		{
			return GetResourceString (key);
		}

		internal static string GetResourceString (string key, params object[] values)
		{
			key = Messages.GetMessage (key);
			return string.Format (CultureInfo.InvariantCulture, key, values);
		}
	}
}