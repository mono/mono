//
// MonoDefaultTlsProvider.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SECURITY_DEP
#if MONO_X509_ALIAS
extern alias PrebuiltSystem;
#endif
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_X509_ALIAS
using XHttpWebRequest = PrebuiltSystem::System.Net.HttpWebRequest;
using XSslProtocols = PrebuiltSystem::System.Security.Authentication.SslProtocols;
using XX509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#else
using XHttpWebRequest = System.Net.HttpWebRequest;
using XSslProtocols = System.Security.Authentication.SslProtocols;
using XX509CertificateCollection = System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
using MonoSecurity::Mono.Security.Protocol.Tls;
#else
using Mono.Security.Interface;
using Mono.Security.Protocol.Tls;
#endif

#if !MOBILE
using System.Reflection;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Net.Security.Private
{
	/*
	 * Strictly private - do not use outside the Mono.Net.Security directory.
	 */
	class MonoDefaultTlsProvider : MonoTlsProviderImpl
	{
		Type sslStream;
#if !MOBILE
		PropertyInfo piClient;
		PropertyInfo piServer;
		PropertyInfo piTrustFailure;
#endif

		public MonoDefaultTlsProvider ()
		{
#if MOBILE
			sslStream = typeof (HttpsClientStream);
#else
			// HttpsClientStream is an internal glue class in Mono.Security.dll
			sslStream = Type.GetType ("Mono.Security.Protocol.Tls.HttpsClientStream, " +
				Consts.AssemblyMono_Security, false);

			if (sslStream == null) {
				string msg = "Missing Mono.Security.dll assembly. " +
					"Support for SSL/TLS is unavailable.";

				throw new NotSupportedException (msg);
			}

			piClient = sslStream.GetProperty ("SelectedClientCertificate");
			piServer = sslStream.GetProperty ("ServerCertificate");
			piTrustFailure = sslStream.GetProperty ("TrustFailure");
#endif
		}

		public MonoTlsProvider Provider {
			get { return this; }
		}

		public override bool SupportsSslStream {
			get { return true; }
		}

		public override bool SupportsMonoExtensions {
			get { return false; }
		}

		public override bool SupportsTlsContext {
			get { return false; }
		}

		public override XSslProtocols SupportedProtocols {
			get { return XSslProtocols.Ssl3 | XSslProtocols.Tls; }
		}

		protected override IMonoSslStream CreateSslStreamImpl (
			Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings)
		{
			return new LegacySslStream (innerStream, leaveInnerStreamOpen, settings);
		}

		protected override IMonoTlsContext CreateTlsContextImpl (
			string hostname, bool serverMode, TlsProtocols protocolFlags,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool remoteCertRequired, MonoEncryptionPolicy encryptionPolicy,
			MonoTlsSettings settings)
		{
			throw new NotSupportedException ();
		}
	}
}
#endif

