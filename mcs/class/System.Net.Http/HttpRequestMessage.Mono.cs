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

			// Mono URI handling which does not distinguish between file and url absolute paths without scheme
			if (uri.Scheme == Uri.UriSchemeFile && uri.OriginalString.StartsWith ("/", StringComparison.Ordinal))
				return true;

			return HttpUtilities.IsHttpUri (uri);
		}
	}
}
