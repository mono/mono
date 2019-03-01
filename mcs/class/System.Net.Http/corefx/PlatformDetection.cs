namespace System
{
	static partial class PlatformDetection
	{
		public static bool IsOSX => true;
		public static bool IsDebian => false;
		internal static bool IsSsl2AndSsl3Supported => false;

		/*
		 * Use of these properties should be strictly limited to the `System.Net.Http` tests.
		 *
		 * Please do not move them into the shared PlatformDetection part or use them outside
		 * this directory without previously talking to me (Martin Baulig (mabaul@microsoft.com))
		 * as I plan to eventually remove / replace them with a more robust mechanism.
		 */
		public static Version OpenSslVersion => new Version (-1, 0);
		public static bool SupportsX509Chain => UsingBtls;
		public static bool SupportsCertRevocation => !UsingBtls;
		public static bool UsingBtls => string.Equals (Environment.GetEnvironmentVariable ("MONO_TLS_PROVIDER"), "btls");
	}
}
