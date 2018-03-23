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

	internal static string Format(string resourceFormat, object p1, object p2, object p3)
	{
		return string.Format (CultureInfo.InvariantCulture, resourceFormat, p1, p2, p3);
	}

	internal static string GetResourceString (string str)
	{
#if INSIDE_CORLIB
		// a quick switch to avoid reflection for https://github.com/dotnet/corefx/blob/43d07018fd0c4176b20481e958bcd1ccadacc23a/src/Common/src/CoreLib/System/Text/Encoding.cs#L450
		switch (str)
		{
			case nameof (Globalization_cp_1200):
				return Globalization_cp_1200;
			case nameof (Globalization_cp_1201):
				return Globalization_cp_1201;
			case nameof (Globalization_cp_12000):
				return Globalization_cp_12000;
			case nameof (Globalization_cp_12001):
				return Globalization_cp_12001;
			case nameof (Globalization_cp_20127):
				return Globalization_cp_20127;
			case nameof (Globalization_cp_28591):
				return Globalization_cp_28591;
			case nameof (Globalization_cp_65000):
				return Globalization_cp_65000;
			case nameof (Globalization_cp_65001):
				return Globalization_cp_65001;
		}
#endif
		return str;
	}
}

#if !INSIDE_CORLIB
namespace System.Runtime.CompilerServices
{
	class FriendAccessAllowedAttribute : Attribute
	{ }
}
#endif
