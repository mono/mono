using System;

internal static partial class Interop
{
	internal static partial class Http
	{
		internal static bool GetSupportsHttp2Multiplexing () => false;

		internal static string GetVersionDescription () => throw new PlatformNotSupportedException ();

		internal static string GetSslVersionDescription () => throw new PlatformNotSupportedException ();

		internal const string OpenSsl10Description = "openssl/1.0";
		internal const string SecureTransportDescription = "SecureTransport";
		internal const string LibreSslDescription = "LibreSSL";
	}
}
