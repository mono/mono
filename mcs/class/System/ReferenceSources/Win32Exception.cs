using System.Runtime.CompilerServices;

namespace System.ComponentModel
{
	partial class Win32Exception
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern string W32ErrorMessage (int error_code);

		private static string GetErrorMessage (int error)
		{
			return W32ErrorMessage (error);
		}
	}
}