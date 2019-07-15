using System.Globalization;

static partial class SR
{
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

	internal static string Format(string resourceFormat, params object[] args)
	{
		if (args != null) {
			return string.Format (CultureInfo.InvariantCulture, resourceFormat, args);
		}

		return resourceFormat;
	}

	internal static string Format(string resourceFormat, object p1)
	{
		return string.Format (CultureInfo.InvariantCulture, resourceFormat, p1);
	}

	internal static string Format(string resourceFormat, object p1, object p2)
	{
		return string.Format (CultureInfo.InvariantCulture, resourceFormat, p1, p2);
	}

	internal static string Format(CultureInfo ci, string resourceFormat, object p1, object p2)
	{
		return string.Format (ci, resourceFormat, p1, p2);
	}

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		return string.Format (CultureInfo.InvariantCulture, resourceFormat, p1, p2, p3);
	}

	internal static string GetResourceString (string str) => str;
}

#if !INSIDE_CORLIB
namespace System.Runtime.CompilerServices
{
	class FriendAccessAllowedAttribute : Attribute
	{ }
}
#endif
