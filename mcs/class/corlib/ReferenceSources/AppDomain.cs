using System.Reflection;
using System.Security;
using System.Runtime.Versioning;

namespace System {

	public partial class AppDomain
	{
		#if UNITY_AOT
		[System.Runtime.CompilerServices.Intrinsic]
		#endif
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
