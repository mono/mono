using System.Reflection;
using System.Security;
using System.Runtime.Versioning;

namespace System {

	public partial class AppDomain
	{
		internal static bool IsAppXModel ()
		{
			return false;
		}

		internal static bool IsAppXDesignMode ()
		{
			return false;
		}

		internal static void CheckReflectionOnlyLoadSupported()
		{
		}

		internal static void CheckLoadFromSupported()
		{
		}
	}
}
