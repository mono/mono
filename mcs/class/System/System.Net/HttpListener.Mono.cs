﻿//
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
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Authenticode;
using MSI = Mono.Security.Interface;
#endif

using System.IO;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MNS = Mono.Net.Security;

namespace System.Net {
	partial class HttpListener {
		MSI.MonoTlsProvider tlsProvider;
		MSI.MonoTlsSettings tlsSettings;
		X509Certificate certificate;

		internal HttpListener (X509Certificate certificate, MSI.MonoTlsProvider tlsProvider, MSI.MonoTlsSettings tlsSettings)
			: this ()
		{
			this.certificate = certificate;
			this.tlsProvider = tlsProvider;
			this.tlsSettings = tlsSettings;
		}

		internal X509Certificate LoadCertificateAndKey (IPAddress addr, int port)
		{
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
					cert.PrivateKey = PrivateKey.CreateFromFile (pvk_file).RSA;
					certificate = cert;
					return certificate;
				} catch {
					// ignore errors
					certificate = null;
					return null;
				}
			}
		}

		internal SslStream CreateSslStream (Stream innerStream, bool ownsStream, RemoteCertificateValidationCallback callback)
		{
			lock (_internalLock) {
				if (tlsProvider == null)
					tlsProvider = MSI.MonoTlsProviderFactory.GetProvider ();
				if (tlsSettings == null)
					tlsSettings = MSI.MonoTlsSettings.CopyDefaultSettings ();
				if (tlsSettings.RemoteCertificateValidationCallback == null)
					tlsSettings.RemoteCertificateValidationCallback = MNS.Private.CallbackHelpers.PublicToMono (callback);
				var sslStream = tlsProvider.CreateSslStream (innerStream, ownsStream, tlsSettings);
				return sslStream.SslStream;
			}
		}
	}
}
#endif
