// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http
{
	partial class SocketsHttpHandler : IMonoHttpClientHandler
	{
		bool IMonoHttpClientHandler.SupportsAutomaticDecompression => true;

		long IMonoHttpClientHandler.MaxRequestContentBufferSize {
			// This property is not supported. In the .NET Framework it was only used when the handler needed to 
			// automatically buffer the request content. That only happened if neither 'Content-Length' nor 
			// 'Transfer-Encoding: chunked' request headers were specified. So, the handler thus needed to buffer
			// in the request content to determine its length and then would choose 'Content-Length' semantics when
			// POST'ing. In .NET Core and UAP platforms, the handler will resolve the ambiguity by always choosing
			// 'Transfer-Encoding: chunked'. The handler will never automatically buffer in the request content.
			get {
				return 0; // Returning zero is appropriate since in .NET Framework it means no limit.
			}

			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				if (value > HttpContent.MaxBufferSize) {
					throw new ArgumentOutOfRangeException (nameof (value), value,
					    string.Format (CultureInfo.InvariantCulture, SR.net_http_content_buffersize_limit,
					    HttpContent.MaxBufferSize));
				}

				CheckDisposedOrStarted ();

				// No-op on property setter.
			}
		}

		// This is only used by MonoWebRequestHandler.
		void IMonoHttpClientHandler.SetWebRequestTimeout (TimeSpan timeout)
		{
		}

		Task<HttpResponseMessage> IMonoHttpClientHandler.SendAsync (HttpRequestMessage request, CancellationToken cancellationToken) => SendAsync (request, cancellationToken);
	}
}
