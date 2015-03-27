using System.Globalization;

static class AssemblyRef
{
	// FIXME
	internal const string SystemConfiguration = "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
	internal const string System = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	public const string EcmaPublicKey = "b77a5c561934e089";
#if NET_2_1
	public const string FrameworkPublicKeyFull = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
#else
	public const string FrameworkPublicKeyFull = "00000000000000000400000000000000";
#endif
	public const string MicrosoftPublicKey = "b03f5f7f11d50a3a";
 
 	public const string MicrosoftJScript = Consts.AssemblyMicrosoft_JScript;
 	public const string MicrosoftVSDesigner = Consts.AssemblyMicrosoft_VSDesigner;
	public const string SystemData = Consts.AssemblySystem_Data;
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
	class FriendAccessAllowedAttribute : Attribute
	{ }
}
