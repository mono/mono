using System.Globalization;

internal sealed class AssemblyRef
{
	// FIXME
	internal const string SystemConfiguration = "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, ProcessorArchitecture=MSIL";
	internal const string System = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, ProcessorArchitecture=MSIL";
	internal const string SystemWeb = "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
}

internal sealed partial class SR
{
	internal static string GetString(string name, params object[] args)
	{
		return GetString (CultureInfo.InvariantCulture, name, args);
	}

	internal static string GetString(CultureInfo culture, string name, params object[] args)
	{
		return string.Format (name, args);
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

namespace System.Runtime.CompilerServices
{
	class FriendAccessAllowed : Attribute
	{ }
}
