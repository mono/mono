// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Net.Http
{
	partial class HttpClientHandler : HttpMessageHandler
	{
		static IMonoHttpClientHandler CreateDefaultHandler () => new MonoWebRequestHandler ();

		// NS2.1:
		public static System.Func<System.Net.Http.HttpRequestMessage, System.Security.Cryptography.X509Certificates.X509Certificate2, System.Security.Cryptography.X509Certificates.X509Chain, System.Net.Security.SslPolicyErrors, bool> DangerousAcceptAnyServerCertificateValidator => throw new PlatformNotSupportedException ();
	}
}
