namespace Microsoft.Win32
{
	static class Win32Native
	{
		internal const string ADVAPI32 = "advapi32.dll";

		internal const int ERROR_SUCCESS = 0x0;

		public static string GetMessage (int hr)
		{
			return "Error " + hr;
		}
	}
}