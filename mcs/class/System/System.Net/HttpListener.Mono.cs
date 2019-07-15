//
// HttpListener.Mono.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
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
//
#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.Authenticode;
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Authenticode;
using Mono.Security.Interface;
#endif
using MNS = Mono.Net.Security;
#endif

using System.IO;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace System.Net {
	partial class HttpListener {
#if SECURITY_DEP
		MonoTlsProvider tlsProvider;
		MonoTlsSettings tlsSettings;
		X509Certificate certificate;

		internal HttpListener (X509Certificate certificate, MonoTlsProvider tlsProvider, MonoTlsSettings tlsSettings)
			: this ()
		{
			this.certificate = certificate;
			this.tlsProvider = tlsProvider;
			this.tlsSettings = tlsSettings;
		}
#endif

		internal X509Certificate LoadCertificateAndKey (IPAddress addr, int port)
		{
#if SECURITY_DEP
			lock (_internalLock) {
				if (certificate != null)
					return certificate;

				// Actually load the certificate
				try {
					string dirname = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
					string path = Path.Combine (dirname, ".mono");
					path = Path.Combine (path, "httplistener");
					string cert_file = Path.Combine (path, String.Format ("{0}.cer", port));
					if (!File.Exists (cert_file))
						return null;
					string pvk_file = Path.Combine (path, String.Format ("{0}.pvk", port));
					if (!File.Exists (pvk_file))
						return null;
					var cert = new X509Certificate2 (cert_file);
					var privateKey = PrivateKey.CreateFromFile (pvk_file).RSA;
					certificate = new X509Certificate2 ((X509Certificate2Impl)cert.Impl.CopyWithPrivateKey (privateKey));
					return certificate;
				} catch {
					// ignore errors
					certificate = null;
					return null;
				}
			}
#else
			throw new PlatformNotSupportedException ();
#endif
		}

		internal SslStream CreateSslStream (Stream innerStream, bool ownsStream, RemoteCertificateValidationCallback callback)
		{
#if SECURITY_DEP
			lock (_internalLock) {
				if (tlsProvider == null)
					tlsProvider = MonoTlsProviderFactory.GetProvider ();
				var settings = (tlsSettings ?? MonoTlsSettings.DefaultSettings).Clone ();
				settings.RemoteCertificateValidationCallback = MNS.Private.CallbackHelpers.PublicToMono (callback);
				return new SslStream (innerStream, ownsStream, tlsProvider, settings);
			}
#else
			throw new PlatformNotSupportedException ();
#endif
		}
	}
}
