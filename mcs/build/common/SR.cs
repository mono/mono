using System.Globalization;

static class AssemblyRef
{
	// FIXME
	internal const string SystemConfiguration = "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
	internal const string System = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	public const string EcmaPublicKey = "b77a5c561934e089";
	public const string MicrosoftPublicKey = "b03f5f7f11d50a3a";
 
	public const string SystemDesign = Consts.AssemblySystem_Design;
	public const string SystemDrawing = Consts.AssemblySystem_Drawing;
	public const string SystemWeb = Consts.AssemblySystem_Web;
	public const string SystemWebExtensions =  "System.Web.Extensions, Version=" + Consts.FxVersion + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
	public const string SystemWindowsForms = Consts.AssemblySystem_Windows_Forms;
}

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
}

namespace System.Runtime.CompilerServices
{
	class FriendAccessAllowed : Attribute
	{ }
}
