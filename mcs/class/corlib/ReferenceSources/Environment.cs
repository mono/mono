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

		internal static String GetRuntimeResourceString (string key)
		{
			return key;
		}

		internal static String GetRuntimeResourceString (string key, params object[] values)
		{
			return string.Format (CultureInfo.InvariantCulture, key, values);
		}

		internal static string GetResourceStringEncodingName (int codePage)
		{
			switch (codePage) {
			case 1200: return GetResourceString ("Globalization.cp_1200");
			case 1201: return GetResourceString ("Globalization.cp_1201");
			case 12000: return GetResourceString ("Globalization.cp_12000");
			case 12001: return GetResourceString ("Globalization.cp_12001");
			case 20127: return GetResourceString ("Globalization.cp_20127");
			case 65000: return GetResourceString ("Globalization.cp_65000");
			case 65001: return GetResourceString ("Globalization.cp_65001");
			default: return codePage.ToString (CultureInfo.InvariantCulture);
			}
		}

		internal static bool IsWindows8OrAbove {
			get {
				return false;
			}
		}
	}
}