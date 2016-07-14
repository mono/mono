using System.Globalization;

namespace System
{
	partial class Environment
	{
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
	}
}