namespace System.Net.Http
{
	partial class HttpRequestMessage
	{
		static bool IsAllowedAbsoluteUri (Uri uri)
		{
			if (!uri.IsAbsoluteUri)
				return true;

#if WASM
			if (uri.Scheme == "blob")
				return true;
#endif

			return HttpUtilities.IsHttpUri (uri);
		}
	}
}
