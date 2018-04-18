using System.Collections;

namespace System.Net
{
	static partial class UnsafeNclNativeMethods
	{
		internal static unsafe class HttpApi 
		{
			const int HttpHeaderRequestMaximum  = (int)HttpRequestHeader.UserAgent + 1;
			const int HttpHeaderResponseMaximum = (int)HttpResponseHeader.WwwAuthenticate + 1;

			internal static class HTTP_REQUEST_HEADER_ID {
				internal static string ToString(int position) {
					return m_Strings[position];
				}

				private static string[] m_Strings = {
					"Cache-Control",
					"Connection",
					"Date",
					"Keep-Alive",
					"Pragma",
					"Trailer",
					"Transfer-Encoding",
					"Upgrade",
					"Via",
					"Warning",

					"Allow",
					"Content-Length",
					"Content-Type",
					"Content-Encoding",
					"Content-Language",
					"Content-Location",
					"Content-MD5",
					"Content-Range",
					"Expires",
					"Last-Modified",

					"Accept",
					"Accept-Charset",
					"Accept-Encoding",
					"Accept-Language",
					"Authorization",
					"Cookie",
					"Expect",
					"From",
					"Host",
					"If-Match",

					"If-Modified-Since",
					"If-None-Match",
					"If-Range",
					"If-Unmodified-Since",
					"Max-Forwards",
					"Proxy-Authorization",
					"Referer",
					"Range",
					"Te",
					"Translate",
					"User-Agent",
				};
			}

			internal static class HTTP_RESPONSE_HEADER_ID {
				private static Hashtable m_Hashtable;

				static HTTP_RESPONSE_HEADER_ID() {
					m_Hashtable = new Hashtable((int)Enum.HttpHeaderResponseMaximum);
					for (int i = 0; i < (int)Enum.HttpHeaderResponseMaximum; i++) {
						m_Hashtable.Add(m_Strings[i], i);
					}
				}

				internal static int IndexOfKnownHeader(string HeaderName) {
					object index = m_Hashtable[HeaderName];
					return index==null ? -1 : (int)index;
				 }

				internal static string ToString(int position) {
					return m_Strings[position];
				}
			}

			internal enum Enum {
				HttpHeaderCacheControl          = 0,    // general-header [section 4.5]
				HttpHeaderConnection            = 1,    // general-header [section 4.5]
				HttpHeaderDate                  = 2,    // general-header [section 4.5]
				HttpHeaderKeepAlive             = 3,    // general-header [not in rfc]
				HttpHeaderPragma                = 4,    // general-header [section 4.5]
				HttpHeaderTrailer               = 5,    // general-header [section 4.5]
				HttpHeaderTransferEncoding      = 6,    // general-header [section 4.5]
				HttpHeaderUpgrade               = 7,    // general-header [section 4.5]
				HttpHeaderVia                   = 8,    // general-header [section 4.5]
				HttpHeaderWarning               = 9,    // general-header [section 4.5]

				HttpHeaderAllow                 = 10,   // entity-header  [section 7.1]
				HttpHeaderContentLength         = 11,   // entity-header  [section 7.1]
				HttpHeaderContentType           = 12,   // entity-header  [section 7.1]
				HttpHeaderContentEncoding       = 13,   // entity-header  [section 7.1]
				HttpHeaderContentLanguage       = 14,   // entity-header  [section 7.1]
				HttpHeaderContentLocation       = 15,   // entity-header  [section 7.1]
				HttpHeaderContentMd5            = 16,   // entity-header  [section 7.1]
				HttpHeaderContentRange          = 17,   // entity-header  [section 7.1]
				HttpHeaderExpires               = 18,   // entity-header  [section 7.1]
				HttpHeaderLastModified          = 19,   // entity-header  [section 7.1]


				// Response Headers

				HttpHeaderAcceptRanges          = 20,   // response-header [section 6.2]
				HttpHeaderAge                   = 21,   // response-header [section 6.2]
				HttpHeaderEtag                  = 22,   // response-header [section 6.2]
				HttpHeaderLocation              = 23,   // response-header [section 6.2]
				HttpHeaderProxyAuthenticate     = 24,   // response-header [section 6.2]
				HttpHeaderRetryAfter            = 25,   // response-header [section 6.2]
				HttpHeaderServer                = 26,   // response-header [section 6.2]
				HttpHeaderSetCookie             = 27,   // response-header [not in rfc]
				HttpHeaderVary                  = 28,   // response-header [section 6.2]
				HttpHeaderWwwAuthenticate       = 29,   // response-header [section 6.2]

				HttpHeaderResponseMaximum       = 30,


				HttpHeaderMaximum               = 41
			}

			private static string[] m_Strings = {
				"Cache-Control",
				"Connection",
				"Date",
				"Keep-Alive",
				"Pragma",
				"Trailer",
				"Transfer-Encoding",
				"Upgrade",
				"Via",
				"Warning",

				"Allow",
				"Content-Length",
				"Content-Type",
				"Content-Encoding",
				"Content-Language",
				"Content-Location",
				"Content-MD5",
				"Content-Range",
				"Expires",
				"Last-Modified",

				"Accept-Ranges",
				"Age",
				"ETag",
				"Location",
				"Proxy-Authenticate",
				"Retry-After",
				"Server",
				"Set-Cookie",
				"Vary",
				"WWW-Authenticate",
			};
		}
	}
}