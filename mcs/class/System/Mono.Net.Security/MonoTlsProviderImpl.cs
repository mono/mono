//
// MonoTlsProviderImpl.cs
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

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
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
	abstract class MonoTlsProviderImpl : MSI.MonoTlsProvider, IMonoTlsProvider
	{
		MSI.MonoTlsProvider IMonoTlsProvider.Provider {
			get { return this; }
		}

		IMonoSslStream IMonoTlsProvider.CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MSI.MonoTlsSettings settings)
		{
			return CreateSslStreamImpl (innerStream, leaveInnerStreamOpen, settings);
		}

		protected abstract IMonoSslStream CreateSslStreamImpl (
			Stream innerStream, bool leaveInnerStreamOpen,
			MSI.MonoTlsSettings settings);

		public override MSI.IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MSI.MonoTlsSettings settings = null)
		{
			var sslStream = CreateSslStreamImpl (innerStream, leaveInnerStreamOpen, settings);
			return new MonoSslStreamImpl (sslStream);
		}

		MSI.IMonoTlsContext IMonoTlsProvider.CreateTlsContext (
			string hostname, bool serverMode, MSI.TlsProtocols protocolFlags,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool remoteCertRequired, bool checkCertName, bool checkCertRevocationStatus,
			MSI.MonoEncryptionPolicy encryptionPolicy, MSI.MonoTlsSettings settings)
		{
			return CreateTlsContextImpl (
				hostname, serverMode, protocolFlags,
				serverCertificate, clientCertificates,
				remoteCertRequired, encryptionPolicy, settings);
		}

		protected abstract MSI.IMonoTlsContext CreateTlsContextImpl (
			string hostname, bool serverMode, MSI.TlsProtocols protocolFlags,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool remoteCertRequired, MSI.MonoEncryptionPolicy encryptionPolicy,
			MSI.MonoTlsSettings settings);

		internal override MSI.IMonoTlsContext CreateTlsContext (
			string hostname, bool serverMode, MSI.TlsProtocols protocolFlags,
			X509Certificate serverCertificate, X509CertificateCollection clientCertificates,
			bool remoteCertRequired, MSI.MonoEncryptionPolicy encryptionPolicy,
			MSI.MonoTlsSettings settings)
		{
			return CreateTlsContextImpl (
				hostname, serverMode, (MSI.TlsProtocols)protocolFlags,
				serverCertificate, clientCertificates,
				remoteCertRequired, (MSI.MonoEncryptionPolicy)encryptionPolicy,
				settings);
		}
	}
}

#endif
