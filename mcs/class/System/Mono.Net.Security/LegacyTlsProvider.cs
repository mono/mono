//
// LegacyTlsProvider.cs
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
using System.Security.Authentication;

namespace Mono.Net.Security
{
	/*
	 * Strictly private - do not use outside the Mono.Net.Security directory.
	 */
	class LegacyTlsProvider : MSI.MonoTlsProvider
	{
		public override Guid ID {
			get { return MonoTlsProviderFactory.LegacyId; }
		}

		public override string Name {
			get { return "legacy"; }
		}

		public override bool SupportsSslStream {
			get { return true; }
		}

		public override bool SupportsConnectionInfo {
			get { return false; }
		}

		public override bool SupportsMonoExtensions {
			get { return false; }
		}

		internal override bool SupportsCleanShutdown {
			get { return false; }
		}

		public override SslProtocols SupportedProtocols {
			get { return SslProtocols.Tls; }
		}

		public override MSI.IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MSI.MonoTlsSettings settings = null)
		{
			return SslStream.CreateMonoSslStream (innerStream, leaveInnerStreamOpen, this, settings);
		}

		internal override MSI.IMonoSslStream CreateSslStreamInternal (
			SslStream sslStream, Stream innerStream, bool leaveInnerStreamOpen,
			MSI.MonoTlsSettings settings)
		{
			return new Private.LegacySslStream (innerStream, leaveInnerStreamOpen, sslStream, this, settings);
		}

		internal override bool ValidateCertificate (
			MSI.ICertificateValidator2 validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref MSI.MonoSslPolicyErrors errors, ref int status11)
		{
			if (wantsChain)
				chain = SystemCertificateValidator.CreateX509Chain (certificates);
			var xerrors = (SslPolicyErrors)errors;
			var result = SystemCertificateValidator.Evaluate (validator.Settings, targetHost, certificates, chain, ref xerrors, ref status11);
			errors = (MSI.MonoSslPolicyErrors)xerrors;
			return result;
		}
	}
}
#endif

