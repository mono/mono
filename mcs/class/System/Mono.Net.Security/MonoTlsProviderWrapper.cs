//
// MonoTlsProviderWrapper.cs
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

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif
#if MONO_X509_ALIAS
using XHttpWebRequest = PrebuiltSystem::System.Net.HttpWebRequest;
using XX509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
#else
using XHttpWebRequest = System.Net.HttpWebRequest;
using XX509CertificateCollection = System.Security.Cryptography.X509Certificates.X509CertificateCollection;
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
	 * 
	 * This is used by MonoTlsProviderFactory.InstallProvider() to wrap and masquerade
	 * a user-supplied @MonoTlsProvider as @IMonoTlsProvider.
	 */
	class MonoTlsProviderWrapper : IMonoTlsProvider
	{
		MSI.MonoTlsProvider provider;

		public MonoTlsProviderWrapper (MSI.MonoTlsProvider provider)
		{
			this.provider = provider;
		}

		public MSI.MonoTlsProvider Provider {
			get { return provider; }
		}

		public IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MSI.MonoTlsSettings settings)
		{
			var sslStream = provider.CreateSslStream (innerStream, leaveInnerStreamOpen, settings);
			var monoSslStreamImpl = sslStream as MonoSslStreamImpl;
			if (monoSslStreamImpl != null)
				return monoSslStreamImpl.Impl;
			return new MonoSslStreamWrapper (sslStream);
		}

		public MSI.IMonoTlsContext CreateTlsContext (
			string hostname, bool serverMode, MSI.TlsProtocols protocolFlags,
			X509Certificate serverCertificate, XX509CertificateCollection clientCertificates,
			bool remoteCertRequired, bool checkCertName, bool checkCertRevocationStatus,
			MSI.MonoEncryptionPolicy encryptionPolicy, MSI.MonoTlsSettings settings)
		{
			return provider.CreateTlsContext (
				hostname, serverMode, protocolFlags,
				serverCertificate, (XX509CertificateCollection)(object)clientCertificates,
				remoteCertRequired, (MSI.MonoEncryptionPolicy)encryptionPolicy,
				settings);
		}
	}
}

#endif
